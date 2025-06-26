
[System.Serializable]
public class NPCProfile
{
    public string npcName;
    public string backstory;
    public string[] dailyRoutine; // 每小时/半小时一格，写活动或地点名
    public string[] hobbies;
    public string[] greetings; // 不同情景的打招呼内容
    // 更多字段，比如好感度、亲密度等
}
