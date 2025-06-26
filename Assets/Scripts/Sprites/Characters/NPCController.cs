using UnityEngine;

public class NPCController : MonoBehaviour
{
    public DialoguePanelController dialoguePanel; // 拖到 Inspector 上

    public NPCProfile profile;
    public Transform[] waypoints; // 按照dailyRoutine顺序设定的位置
    private int currentRoutineIndex = 0;

    public float moveSpeed = 2f;
    private float nextActionTime = 0f;
    private float routineInterval = 3600f; // 1小时（可根据需要调整）

    private void Start()
    {
        MoveToNextWaypoint();
    }

    private void Update()
    {
        // 每隔routineInterval执行一次行动
        if (Time.time > nextActionTime)
        {
            currentRoutineIndex = (currentRoutineIndex + 1) % profile.dailyRoutine.Length;
            MoveToNextWaypoint();
            nextActionTime = Time.time + routineInterval;
        }

        // 其他行为：检测主角靠近
        if (Vector3.Distance(PlayerMovementController.Instance.transform.position, transform.position) < 2f)
        {
            GreetPlayer();
        }
    }

    void MoveToNextWaypoint()
    {
        // 简化：直接传送，可改为逐步移动
        transform.position = waypoints[currentRoutineIndex].position;
    }

    void GreetPlayer()
    {
        dialoguePanel.ShowDialogue(
            null, // 可以传 Sprite 头像
            profile.npcName, // 名字
            profile.greetings[Random.Range(0, profile.greetings.Length)] // 随机打招呼内容
        );
    }
}
