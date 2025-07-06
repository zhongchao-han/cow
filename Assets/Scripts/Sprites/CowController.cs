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

    void Update()
    {
        UpdateObstacleTile();

        if (Vector3.Distance(player.position, transform.position) < 2.5f && playerHoldingStick)
        {
            EscapeFromPlayer();
        }
        else
        {
            animator?.SetBool("IsWalking", false);
        }
    }

    void EscapeFromPlayer()
    {
        // 投影到 2D 平面，避免 z 坐标干扰
        Vector2 delta = transform.position - player.position;
        Vector3 dir;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            dir = new Vector3(Mathf.Sign(delta.x), 0, 0); // 左右
        else
            dir = new Vector3(0, Mathf.Sign(delta.y), 0); // 上下

        UpdateAnimatorDirection(dir);

        transform.position += dir * moveSpeed * 2f * Time.deltaTime;
        animator?.SetBool("IsWalking", true);
    }


    void UpdateAnimatorDirection(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            animator.SetFloat("MoveX", dir.x > 0 ? 1 : -1);
            animator.SetFloat("MoveY", 0);
            transform.localScale = new Vector3(2f, 1f, 1f); // 横向
        }
        else
        {
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("MoveY", dir.y > 0 ? 1 : -1);
            transform.localScale = new Vector3(1f, 2f, 1f); // 纵向
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
