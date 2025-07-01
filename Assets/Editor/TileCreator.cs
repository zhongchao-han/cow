#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class TileCreator
{
    [MenuItem("Assets/Create/Custom/Empty Blocking Tile")]
    public static void CreateTileAsset()
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.colliderType = Tile.ColliderType.Grid;

        AssetDatabase.CreateAsset(tile, "Assets/BlockingTile.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = tile;
    }
}
#endif
