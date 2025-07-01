#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NPCProfile))]
public class NPCProfileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        NPCProfile profile = (NPCProfile)target;
        if (GUILayout.Button("导入CSV作息表"))
        {
            // 你可以实现CSV读取并填充dailySchedule
        }
    }
}
#endif
