#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;

public class TileBatchCreator
{
    [MenuItem("Assets/Create/Custom/Generate Corn & Grass Tiles (4 each)")]
    public static void CreateStageTiles()
    {
        string folderPath = "Assets/Crops/";
        Directory.CreateDirectory(folderPath);

        // 创建 Corn Stages
        for (int i = 0; i < 4; i++)
        {
            Tile cornTile = ScriptableObject.CreateInstance<Tile>();
            cornTile.colliderType = Tile.ColliderType.None;

            string path = Path.Combine(folderPath, $"Corn_Stage{i}.asset");
            AssetDatabase.CreateAsset(cornTile, path);
        }

        // 创建 Grass Stages
        for (int i = 0; i < 4; i++)
        {
            Tile grassTile = ScriptableObject.CreateInstance<Tile>();
            grassTile.colliderType = Tile.ColliderType.None;

            string path = Path.Combine(folderPath, $"Grass_Stage{i}.asset");
            AssetDatabase.CreateAsset(grassTile, path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("完成", "已创建 8 个 Tile 资源于 Assets/Crops/", "好");
    }
}
#endif
