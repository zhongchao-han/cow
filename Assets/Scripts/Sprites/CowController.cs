using UnityEngine;
using UnityEngine.Tilemaps;

public enum CowState
{
    Idle,
    EatingGrass,
    EatingCorn,
    Escaping
}

public class CowController : MonoBehaviour
{
    [Header("基础设置")]
    public float moveSpeed = 1.5f;
    public Tilemap grassTilemap;
    public Tilemap cornTilemap;
    public Tilemap obstacleTilemap;
    public TileBase blockingTile;

    [Header("玩家相关")]
    public Transform player;
    public bool playerHoldingStick = false;

    [Header("动画")]
    public Animator animator;

    private CowState currentState = CowState.Idle;
    private Vector3Int currentTargetCell;
    private Vector3Int lastBlockedCell;
    private float stateTimer = 0f;

    void Start()
    {
        UpdateObstacleTile();
    }

    void Update()
    {
        UpdateObstacleTile();
        SenseEnvironment();
        HandleState();
    }

    void SenseEnvironment()
    {
        if (Vector3.Distance(player.position, transform.position) < 2.5f && playerHoldingStick)
        {
            currentState = CowState.Escaping;
            return;
        }

        Vector3Int cowCell = grassTilemap.WorldToCell(transform.position);

        if (FindNearbyTile(cornTilemap, cowCell, out currentTargetCell))
        {
            currentState = CowState.EatingCorn;
            return;
        }

        if (FindNearbyTile(grassTilemap, cowCell, out currentTargetCell))
        {
            currentState = CowState.EatingGrass;
            return;
        }

        currentState = CowState.Idle;
    }

    void HandleState()
    {
        switch (currentState)
        {
            case CowState.EatingCorn:
                MoveTo(currentTargetCell);
                break;

            case CowState.EatingGrass:
                WanderAndGraze();
                break;

            case CowState.Escaping:
                EscapeFromPlayer();
                break;

            case CowState.Idle:
                WanderRandomly();
                break;
        }
    }

    void MoveTo(Vector3Int cell)
    {
        Vector3 targetPos = grassTilemap.GetCellCenterWorld(cell);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        animator?.SetBool("IsWalking", true);

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            animator?.SetTrigger("Eat");
            Debug.Log("牛正在吃东西");
            stateTimer = Random.Range(2f, 4f);
            currentState = CowState.Idle;
        }
    }

    void WanderAndGraze()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            Vector3Int randomDir = new Vector3Int(Random.Range(-1, 2), Random.Range(-1, 2), 0);
            currentTargetCell = grassTilemap.WorldToCell(transform.position) + randomDir;
            stateTimer = 2f;
        }
        MoveTo(currentTargetCell);
    }

    void EscapeFromPlayer()
    {
        Vector3 dir = (transform.position - player.position).normalized;
        transform.position += dir * moveSpeed * 2f * Time.deltaTime;
        animator?.SetBool("IsWalking", true);

        if (Vector3.Distance(transform.position, player.position) > 5f)
        {
            currentState = CowState.Idle;
        }
    }

    void WanderRandomly()
    {
        animator?.SetBool("IsWalking", false);
    }

    bool FindNearbyTile(Tilemap map, Vector3Int origin, out Vector3Int foundCell)
    {
        Vector2Int[] dirs = new Vector2Int[] {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (var dir in dirs)
        {
            Vector3Int neighbor = origin + new Vector3Int(dir.x, dir.y, 0);
            if (map.HasTile(neighbor))
            {
                foundCell = neighbor;
                return true;
            }
        }

        foundCell = origin;
        return false;
    }

    void UpdateObstacleTile()
    {
        Vector3Int currentCell = obstacleTilemap.WorldToCell(transform.position);

        if (currentCell != lastBlockedCell)
        {
            if (obstacleTilemap.HasTile(lastBlockedCell))
                obstacleTilemap.SetTile(lastBlockedCell, null);

            obstacleTilemap.SetTile(currentCell, blockingTile);
            lastBlockedCell = currentCell;
        }
    }
}
