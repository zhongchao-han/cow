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

    public Tilemap tilemap;         // Inspector 拖你的 Tilemap
    public Tilemap obstacleTilemap; // 如果有障碍物独立 Tilemap，可拖这里，没有可与上面同用

    public float moveSpeed = 3f;    // 移动速度

    private Queue<Vector3> pathPoints = new Queue<Vector3>();
    private bool moving = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float frameTimer;
    private int frameIndex;
    private enum Direction { Up, Down, Left, Right }
    private Direction currentDirection = Direction.Down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        ResizeSprite();
    }

    void Update()
    {
        // 鼠标左键点击格子，计算A*路径
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector3Int gridStart = tilemap.WorldToCell(transform.position);
            Vector3Int gridGoal = tilemap.WorldToCell(mouseWorldPos);

            List<Vector3> path = FindPathAStar(gridStart, gridGoal);
            if (path != null && path.Count > 0)
            {
                pathPoints = new Queue<Vector3>(path);
                moving = true;
            }
        }

        // 自动沿路径点移动
        if (moving && pathPoints.Count > 0)
        {
            Vector3 target = pathPoints.Peek();
            Vector2 dir = (target - transform.position).normalized;
            SetDirectionByVector(dir);

            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.05f)
                pathPoints.Dequeue();
        }
        else
        {
            moving = false;
        }

        AnimateWalk();
    }

    // ====== 关键 A* 算法部分 ======
    List<Vector3> FindPathAStar(Vector3Int startCell, Vector3Int goalCell)
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

        while (open.Count > 0)
        {
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
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // 曼哈顿距离
    }

    bool IsCellWalkable(Vector3Int cell)
    {
        // 如果 obstacleTilemap 有障碍Tile，返回 false；否则只判断 tilemap
        if (obstacleTilemap != null && obstacleTilemap.HasTile(cell))
            return false;
        // 也可根据 tilemap 上是否有Tile来判断是否可走
        return !tilemap.HasTile(cell) || (tilemap.HasTile(cell) && (obstacleTilemap==null || !obstacleTilemap.HasTile(cell)));
    }

    List<Vector3> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int curr)
    {
        List<Vector3> totalPath = new List<Vector3>();
        totalPath.Add(tilemap.GetCellCenterWorld(curr));
        while (cameFrom.ContainsKey(curr))
        {
            curr = cameFrom[curr];
            totalPath.Insert(0, tilemap.GetCellCenterWorld(curr));
        }
        return totalPath;
    }

    // 简易优先队列
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

    // 方向自动判断（根据移动向量设置动画朝向）
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
            case Direction.Up:    return upSprites;
            case Direction.Down:  return downSprites;
            case Direction.Left:  return leftSprites;
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
