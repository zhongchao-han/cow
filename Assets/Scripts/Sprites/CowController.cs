using UnityEngine;
using UnityEngine.Tilemaps;

public class CowController : MonoBehaviour
{
    [Header("基础设置")]
    public float moveSpeed = 1.5f;
    public Tilemap obstacleTilemap;
    public TileBase blockingTile;

    [Header("玩家相关")]
    public Transform player;
    public bool playerHoldingStick = false;

    [Header("动画")]
    public Animator animator;

    private Vector3Int lastBlockedCell;
    private Vector3Int escapeTargetCell;
    private bool isEscaping = false;

    void Update()
    {
        UpdateObstacleTile();

        // 玩家靠近且拿棍子，开始逃跑
        if (!isEscaping && Vector3.Distance(player.position, transform.position) < 2.5f && playerHoldingStick)
        {
            StartEscapeFromPlayer();
        }

        if (isEscaping)
        {
            MoveToEscapeTarget();
        }
        else
        {
            animator?.SetBool("IsWalking", false);
        }
    }

    void StartEscapeFromPlayer()
    {
        Vector3 dir = (transform.position - player.position);

        // 只允许上下左右方向
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            dir = new Vector3(Mathf.Sign(dir.x), 0, 0);
        else
            dir = new Vector3(0, Mathf.Sign(dir.y), 0);

        // 当前 tile 为尾部 tile
        Vector3Int tailCell = obstacleTilemap.WorldToCell(transform.position);
        Vector3Int offset = new Vector3Int((int)dir.x, (int)dir.y, 0);

        int escapeDistance = 4;
        escapeTargetCell = tailCell + offset * escapeDistance;

        isEscaping = true;

        UpdateAnimatorDirection(dir);
        animator?.SetBool("IsWalking", true);
    }

    void MoveToEscapeTarget()
    {
        Vector3Int tailCell = escapeTargetCell;

        Vector3Int headCell;
        if (transform.localScale.x > transform.localScale.y)
        {
            // 横向头部
            headCell = tailCell + new Vector3Int((int)Mathf.Sign(transform.localScale.x), 0, 0);
        }
        else
        {
            // 纵向头部
            headCell = tailCell + new Vector3Int(0, (int)Mathf.Sign(transform.localScale.y), 0);
        }

        Vector3 headWorld = obstacleTilemap.GetCellCenterWorld(headCell);
        Vector3 tailWorld = obstacleTilemap.GetCellCenterWorld(tailCell);
        Vector3 targetWorld = (headWorld + tailWorld) / 2f;

        transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * 2f * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWorld) < 0.05f)
        {
            transform.position = targetWorld;
            animator?.SetBool("IsWalking", false);
            isEscaping = false;
        }
    }

    void UpdateAnimatorDirection(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            animator.SetFloat("MoveX", dir.x > 0 ? 1 : -1);
            animator.SetFloat("MoveY", 0);
            transform.localScale = new Vector3(2f, 1f, 1f); // 横向拉伸
        }
        else
        {
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("MoveY", dir.y > 0 ? 1 : -1);
            transform.localScale = new Vector3(1f, 2f, 1f); // 纵向拉伸
        }

        Debug.Log($"方向: {dir}, MoveX: {animator.GetFloat("MoveX")}, MoveY: {animator.GetFloat("MoveY")}");
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
