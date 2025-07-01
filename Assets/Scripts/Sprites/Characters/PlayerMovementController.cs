using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;

    [Header("Animation Frames (Up/Down: 2 frames; Left/Right: 3 frames)")]
    [SerializeField] private List<Sprite> upSprites = new List<Sprite>(2);
    [SerializeField] private List<Sprite> downSprites = new List<Sprite>(2);
    [SerializeField] private List<Sprite> leftSprites = new List<Sprite>(3);
    [SerializeField] private List<Sprite> rightSprites = new List<Sprite>(3);

    [Header("Frame Timing")]
    [SerializeField] private float frameDuration = 0.2f;

    [Header("Sprite World Size (Units)")]
    [SerializeField] private Vector2 spriteWorldSize = new Vector2(1f, 2f);

    public Tilemap tilemap;         // 主Tilemap
    public List<Tilemap> obstacleTilemaps; // 在 Inspector 中拖多个 Tilemap

    public CowController cowController;

    public float moveSpeed = 3f;    // 移动速度

    // 用格子坐标队列存路径
    private Queue<Vector3Int> cellPathPoints = new Queue<Vector3Int>();
    private bool moving = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float frameTimer;
    private int frameIndex;
    private enum Direction { Up, Down, Left, Right }
    private Direction currentDirection = Direction.Down;
    public bool holdingStick;

    public static PlayerMovementController Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        ResizeSprite();
    }

    void Update()
    {
        // 鼠标左键点击格子，A*寻路
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector3Int gridGoal = tilemap.WorldToCell(mouseWorldPos);

            // 目标格子如果不可走，直接return，防止死锁卡死
            if (!IsCellWalkable(gridGoal))
            {
                Debug.Log("目标为障碍或不可行走，忽略本次寻路");
                return;
            }

            // 角色脚底所在的格子
            Vector3Int gridStart = tilemap.WorldToCell(GetFootPosition());

            List<Vector3Int> path = FindPathAStar(gridStart, gridGoal);
            if (path != null && path.Count > 0)
            {
                cellPathPoints = new Queue<Vector3Int>(path);
                moving = true;
            }
        }

        // 沿路径自动移动（每一步都对齐格子底部中心）
        if (moving && cellPathPoints.Count > 0)
        {
            Vector3Int gridTarget = cellPathPoints.Peek();
            Vector3 footTarget = GetCellBottomCenter(gridTarget);

            Vector2 dir = (footTarget - transform.position).normalized;
            SetDirectionByVector(dir);

            transform.position = Vector3.MoveTowards(transform.position, footTarget, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, footTarget) < 0.05f)
            {
                transform.position = footTarget; // 吸附，避免误差
                cellPathPoints.Dequeue();
            }
        }
        else
        {
            moving = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            holdingStick = !holdingStick;

        cowController.playerHoldingStick = holdingStick;

        AnimateWalk();
    }

    // 获取角色“脚底”世界坐标（假设Sprite Pivot在底部）
    private Vector3 GetFootPosition()
    {
        return transform.position;
    }

    // 获取格子的底边中心世界坐标
    private Vector3 GetCellBottomCenter(Vector3Int cell)
    {
        Vector3 cellCenter = tilemap.GetCellCenterWorld(cell);
        float tileHeight = tilemap.cellSize.y;
        return cellCenter - new Vector3(0, tileHeight / 2f, 0);
    }

    // ====== A* 算法（返回格子坐标序列，带最大步数保护） ======
    List<Vector3Int> FindPathAStar(Vector3Int startCell, Vector3Int goalCell)
    {
        HashSet<Vector3Int> closed = new HashSet<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        Dictionary<Vector3Int, float> gScore = new Dictionary<Vector3Int, float>();
        PriorityQueue<Vector3Int> open = new PriorityQueue<Vector3Int>();
        gScore[startCell] = 0;
        open.Enqueue(startCell, 0);

        Vector2Int[] dirs = new Vector2Int[] {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        int maxSteps = 5000; // 防死锁，地图大可再调高
        int steps = 0;

        while (open.Count > 0)
        {
            steps++;
            if (steps > maxSteps)
            {
                Debug.LogWarning("A*步骤超限，疑似死循环/障碍包围，寻路中断！");
                return null;
            }

            Vector3Int curr = open.Dequeue();
            if (curr == goalCell)
                return ReconstructPath(cameFrom, curr);

            closed.Add(curr);
            foreach (var dir in dirs)
            {
                Vector3Int neighbor = curr + new Vector3Int(dir.x, dir.y, 0);
                if (closed.Contains(neighbor) || !IsCellWalkable(neighbor))
                    continue;

                float tentativeG = gScore[curr] + 1;
                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = curr;
                    gScore[neighbor] = tentativeG;
                    float f = tentativeG + Heuristic(neighbor, goalCell);
                    open.Enqueue(neighbor, f);
                }
            }
        }
        return null;
    }

    float Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    bool IsCellWalkable(Vector3Int cell)
    {
        foreach (Tilemap map in obstacleTilemaps)
        {
            if (map != null && map.HasTile(cell))
            {
                Debug.DrawLine(tilemap.GetCellCenterWorld(cell), tilemap.GetCellCenterWorld(cell) + Vector3.up * 0.5f, Color.red, 1f);
                return false;
            }
        }
        return true;
    }



    List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int curr)
    {
        List<Vector3Int> totalPath = new List<Vector3Int>();
        totalPath.Add(curr);
        while (cameFrom.ContainsKey(curr))
        {
            curr = cameFrom[curr];
            totalPath.Insert(0, curr);
        }
        return totalPath;
    }

    // ===== 简易优先队列 =====
    public class PriorityQueue<T>
    {
        List<KeyValuePair<T, float>> data = new List<KeyValuePair<T, float>>();
        public int Count { get { return data.Count; } }
        public void Enqueue(T item, float priority)
        {
            data.Add(new KeyValuePair<T, float>(item, priority));
        }
        public T Dequeue()
        {
            int bestIndex = 0;
            float bestPriority = data[0].Value;
            for (int i = 1; i < data.Count; i++)
            {
                if (data[i].Value < bestPriority)
                {
                    bestPriority = data[i].Value;
                    bestIndex = i;
                }
            }
            T bestItem = data[bestIndex].Key;
            data.RemoveAt(bestIndex);
            return bestItem;
        }
    }

    // 动画与朝向
    void SetDirectionByVector(Vector2 dir)
    {
        if (dir == Vector2.zero) return;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            currentDirection = dir.x > 0 ? Direction.Right : Direction.Left;
        else
            currentDirection = dir.y > 0 ? Direction.Up : Direction.Down;
    }

    private void AnimateWalk()
    {
        List<Sprite> sprites = GetSpritesList();
        if (sprites == null || sprites.Count == 0)
            return;

        if (!moving)
        {
            frameIndex = 0;
            frameTimer = 0;
            sr.sprite = sprites[0];
            return;
        }

        frameTimer += Time.deltaTime;
        if (frameTimer >= frameDuration)
        {
            frameTimer = 0;
            frameIndex = (frameIndex + 1) % sprites.Count;
        }
        sr.sprite = sprites[frameIndex];
    }

    private List<Sprite> GetSpritesList()
    {
        switch (currentDirection)
        {
            case Direction.Up: return upSprites;
            case Direction.Down: return downSprites;
            case Direction.Left: return leftSprites;
            case Direction.Right: return rightSprites;
        }
        return downSprites;
    }

    private void ResizeSprite()
    {
        if (sr.sprite == null) return;
        Vector2 spriteSize = sr.sprite.bounds.size;
        Vector3 newScale = new Vector3(
            spriteWorldSize.x / spriteSize.x,
            spriteWorldSize.y / spriteSize.y,
            1f
        );
        transform.localScale = newScale;
    }

}
