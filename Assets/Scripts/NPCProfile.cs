// NPCProfile.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NPCProfile", menuName = "NPC/NPCProfile")]
public class NPCProfile : ScriptableObject
{
    public string npcName;
    [TextArea] public string backgroundStory;
    public List<ScheduleEntry> dailySchedule;
    public List<string> likes;
    public List<string> dislikes;
    public List<string> greetingDialogues;
}

[System.Serializable]
public class ScheduleEntry
{
    [Range(0, 23)] public int hour;
    public string locationTag;   // 位置标记（如"Farm", "Home"）
    public string action;        // 行为描述（如"Farming", "Sleeping"）
}
