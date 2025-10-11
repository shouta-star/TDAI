using UnityEditor;
using UnityEngine;

public class TileGridAssigner : EditorWindow
{
    private MapManager mapManager;

    [MenuItem("Tools/Assign Grid Index to Tiles")]
    public static void ShowWindow()
    {
        GetWindow<TileGridAssigner>("Grid Index Assigner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tile Grid 自動割り当てツール", EditorStyles.boldLabel);

        mapManager = (MapManager)EditorGUILayout.ObjectField("MapManager", mapManager, typeof(MapManager), true);

        if (mapManager == null)
        {
            EditorGUILayout.HelpBox("MapManager を指定してください。", MessageType.Info);
            return;
        }

        if (GUILayout.Button("自動割り当てを実行"))
        {
            AssignGridIndices();
        }
    }

    private void AssignGridIndices()
    {
        if (mapManager.mapRoot == null)
        {
            Debug.LogError("[TileGridAssigner] mapRoot が設定されていません。");
            return;
        }

        Undo.RecordObjects(mapManager.mapRoot.GetComponentsInChildren<TileComponent>(), "Assign Grid Indices");

        float spacing = mapManager.nodeSpacing;
        int count = 0;

        foreach (var tile in mapManager.mapRoot.GetComponentsInChildren<TileComponent>())
        {
            Vector3 pos = tile.transform.position;
            // spacingで割って四捨五入し整数に
            int gx = Mathf.RoundToInt(pos.x / spacing);
            int gz = Mathf.RoundToInt(pos.z / spacing);

            tile.gridX = gx;
            tile.gridZ = gz;
            count++;
        }

        Debug.Log($"[TileGridAssigner] {count} 枚の Tile に gridX/gridZ を設定しました。");
        EditorUtility.SetDirty(mapManager);
    }
}
