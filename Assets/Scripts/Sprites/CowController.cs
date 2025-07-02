using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;

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

    [Header("作物 tile 阶段")]
    public TileBase[] cornStages; // 完整 -> 半吃 -> 杆
    public TileBase[] grassStages; // 高草 -> 矮草 -> 土地

    [Header("视野")]
    public int sightRange = 5;

    private CowState currentState = CowState.Idle;
    private Vector3Int currentTargetCell;
    private Vector3Int lastBlockedCell;
    private float stateTimer = 0f;

    private float idleTimer = 0f;
    private Vector3Int idleTarget;
    private bool hasIdleTarget = false;

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

        if (FindTileInRange(cornTilemap, cornStages, cowCell, out currentTargetCell))
        {
            currentState = CowState.EatingCorn;
            return;
        }

        if (FindTileInRange(grassTilemap, grassStages, cowCell, out currentTargetCell))
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
            animator?.SetBool("IsWalking", false);
            animator?.SetTrigger("Eat");

            if (cornTilemap.HasTile(cell))
            {
                StartCoroutine(EatTileInStages(cornTilemap, cell, cornStages));
            }
            else if (grassTilemap.HasTile(cell))
            {
                EatGrassAt(cell);
            }

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
        idleTimer -= Time.deltaTime;

        if (!hasIdleTarget || Vector3.Distance(transform.position, GetCellBottomCenter(idleTarget)) < 0.1f)
        {
            if (Random.value < 0.5f)
            {
                idleTarget = GetRandomNearbyCell();
                hasIdleTarget = true;
                idleTimer = Random.Range(2f, 4f);
                animator?.SetBool("IsWalking", true);
            }
            else
            {
                animator?.SetBool("IsWalking", false);
                hasIdleTarget = false;
            }
        }

        if (hasIdleTarget)
        {
            Vector3 targetPos = GetCellBottomCenter(idleTarget);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * 0.5f * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            {
                hasIdleTarget = false;
                animator?.SetBool("IsWalking", false);
            }
        }
    }

    Vector3Int GetRandomNearbyCell()
    {
        Vector3Int current = grassTilemap.WorldToCell(transform.position);
        List<Vector3Int> candidates = new List<Vector3Int>();

        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector3Int check = current + new Vector3Int(dx, dy, 0);
                if (IsCellWalkable(check))
                {
                    candidates.Add(check);
                }
            }
        }

        if (candidates.Count > 0)
            return candidates[Random.Range(0, candidates.Count)];
        else
            return current;
    }

    bool IsCellWalkable(Vector3Int cell)
    {
        return !obstacleTilemap.HasTile(cell);
    }

    bool FindTileInRange(Tilemap map, TileBase[] stages, Vector3Int origin, out Vector3Int targetCell)
    {
        for (int dx = -sightRange; dx <= sightRange; dx++)
        {
            for (int dy = -sightRange; dy <= sightRange; dy++)
            {
                Vector3Int offset = new Vector3Int(dx, dy, 0);
                Vector3Int check = origin + offset;

                TileBase tile = map.GetTile(check);
                if (tile == stages[0])
                {
                    targetCell = check;
                    return true;
                }
            }
        }
        targetCell = origin;
        return false;
    }

    IEnumerator EatTileInStages(Tilemap map, Vector3Int cell, TileBase[] stages)
    {
        currentState = CowState.Idle;
        animator?.SetBool("IsWalking", false);
        animator?.SetTrigger("Eat");

        for (int i = 1; i < stages.Length; i++)
        {
            yield return new WaitForSeconds(0.4f);
            map.SetTile(cell, stages[i]);
        }
    }

    void EatGrassAt(Vector3Int cell)
    {
        TileBase current = grassTilemap.GetTile(cell);
        int index = System.Array.IndexOf(grassStages, current);
        if (index >= 0 && index < grassStages.Length - 1)
        {
            grassTilemap.SetTile(cell, grassStages[index + 1]);
        }
    }

    Vector3 GetCellBottomCenter(Vector3Int cell)
    {
        Vector3 cellCenter = grassTilemap.GetCellCenterWorld(cell);
        float tileHeight = grassTilemap.cellSize.y;
        return cellCenter - new Vector3(0, tileHeight / 2f, 0);
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
