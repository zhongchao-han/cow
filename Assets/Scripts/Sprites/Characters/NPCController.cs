using UnityEngine;
using System.Collections;

public enum NPCState { Idle, Moving, Working, Sleeping, Greeting }

public class NPCController : MonoBehaviour
{
    public DialoguePanelController dialoguePanel;
    public NPCProfile profile;
    public Transform[] locations; // 按顺序：Farm, Home, Market...
    public float moveSpeed = 1.2f;
    public float greetDistance = 1.5f;
    public Transform player;

    private NPCState currentState = NPCState.Idle;
    private int currentScheduleIndex = 0;
    private Transform targetLocation;

    void Start()
    {
        if (profile == null) return;
        SetCurrentSchedule();
    }

    void Update()
    {
        CheckScheduleChange();
        HandleState();
        CheckPlayerNearby();
    }

    void CheckScheduleChange()
    {
        int hour = GameTimeManager.Instance.Hour; // 你的世界时间系统
        // 切换作息
        if (profile.dailySchedule.Count > 0 && hour == profile.dailySchedule[currentScheduleIndex].hour)
        {
            SetCurrentSchedule();
        }
    }

    void SetCurrentSchedule()
    {
        var entry = profile.dailySchedule[currentScheduleIndex];
        targetLocation = GetLocationByTag(entry.locationTag);
        if (entry.action == "Sleeping") currentState = NPCState.Sleeping;
        else if (entry.action == "Working") currentState = NPCState.Working;
        else currentState = NPCState.Moving;
    }

    void HandleState()
    {
        switch (currentState)
        {
            case NPCState.Moving:
                if (targetLocation != null)
                    MoveTo(targetLocation.position);
                break;
            case NPCState.Working:
                // 播放动画等
                break;
            case NPCState.Sleeping:
                // 显示睡觉动画
                break;
            case NPCState.Greeting:
                // 不移动，播放对话
                break;
            case NPCState.Idle:
            default:
                // 站立动画
                break;
        }
    }

    void MoveTo(Vector3 pos)
    {
        transform.position = Vector3.MoveTowards(transform.position, pos, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, pos) < 0.05f)
        {
            currentState = NPCState.Idle;
        }
    }

    void CheckPlayerNearby()
    {
        if (Vector3.Distance(transform.position, player.position) < greetDistance)
        {
            if (currentState != NPCState.Greeting && CanGreet())
            {
                StartCoroutine(GreetPlayer());
            }
        }
    }

    IEnumerator GreetPlayer()
    {
        currentState = NPCState.Greeting;
        // 触发对话UI，调用你自己的对话系统
        dialoguePanel.ShowDialogue(
            null, // 可以传 Sprite 头像
            profile.npcName, // 名字
            profile.greetingDialogues[Random.Range(0, profile.greetingDialogues.Count)] // 随机打招呼内容
        );
        yield return new WaitForSeconds(2f); // 假设2秒对话
        currentState = NPCState.Idle;
    }

    bool CanGreet()
    {
        // 你可以加条件，比如夜里不打招呼
        int hour = GameTimeManager.Instance.Hour;
        return hour >= 7 && hour <= 21;
    }

    Transform GetLocationByTag(string tag)
    {
        // 比如用名字找Transform，简化写法
        foreach (var t in locations)
        {
            if (t.name == tag)
                return t;
        }
        return null;
    }
}
