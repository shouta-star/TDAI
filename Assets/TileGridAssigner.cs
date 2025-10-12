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

        // ← ここで一度だけ取得して使い回す（非アクティブも拾う）
        var tiles = mapManager.mapRoot.GetComponentsInChildren<TileComponent>(true);

        Undo.RecordObjects(tiles, "Assign Grid Indices");

        float spacing = mapManager.nodeSpacing;
        int count = 0;

        // 1周目: gridX/gridZ を割り当て
        foreach (var t in tiles)
        {
            if (!t) continue;
            Vector3 pos = t.transform.position;

            // はみ出し防止なら FloorToInt 推奨（RoundToInt のままでも可）
            int gx = Mathf.FloorToInt(pos.x / spacing);
            int gz = Mathf.FloorToInt(pos.z / spacing);

            t.gridX = gx;
            t.gridZ = gz;
            count++;
        }

        // 2周目: 最小/最大を計測してログ
        int maxX = int.MinValue, maxZ = int.MinValue, minX = int.MaxValue, minZ = int.MaxValue;
        foreach (var t in tiles)
        {
            if (!t) continue;
            if (t.gridX > maxX) maxX = t.gridX;
            if (t.gridZ > maxZ) maxZ = t.gridZ;
            if (t.gridX < minX) minX = t.gridX;
            if (t.gridZ < minZ) minZ = t.gridZ;
        }
        Debug.Log($"[Debug] MaxGridX={maxX}, MaxGridZ={maxZ} / MinGridX={minX}, MinGridZ={minZ}");

        Debug.Log($"[TileGridAssigner] {count} 枚の Tile に gridX/gridZ を設定しました。");
        EditorUtility.SetDirty(mapManager);

        // ★次にやること：MapManager の SizeX/SizeZ を Max+1 に合わせる
        //   （例）MaxGridX=30 ⇒ SizeX=31、MaxGridZ=20 ⇒ SizeZ=21
    }
}


//using UnityEditor;
//using UnityEngine;

//public class TileGridAssigner : EditorWindow
//{
//    private MapManager mapManager;

//    [MenuItem("Tools/Assign Grid Index to Tiles")]
//    public static void ShowWindow()
//    {
//        GetWindow<TileGridAssigner>("Grid Index Assigner");
//    }

//    private void OnGUI()
//    {
//        GUILayout.Label("Tile Grid 自動割り当てツール", EditorStyles.boldLabel);

//        mapManager = (MapManager)EditorGUILayout.ObjectField("MapManager", mapManager, typeof(MapManager), true);

//        if (mapManager == null)
//        {
//            EditorGUILayout.HelpBox("MapManager を指定してください。", MessageType.Info);
//            return;
//        }

//        if (GUILayout.Button("自動割り当てを実行"))
//        {
//            AssignGridIndices();
//        }
//    }

//    private void AssignGridIndices()
//    {
//        if (mapManager.mapRoot == null)
//        {
//            Debug.LogError("[TileGridAssigner] mapRoot が設定されていません。");
//            return;
//        }

//        Undo.RecordObjects(mapManager.mapRoot.GetComponentsInChildren<TileComponent>(), "Assign Grid Indices");

//        float spacing = mapManager.nodeSpacing;
//        int count = 0;

//        foreach (var tile in mapManager.mapRoot.GetComponentsInChildren<TileComponent>())
//        {
//            Vector3 pos = tile.transform.position;
//            // spacingで割って四捨五入し整数に
//            int gx = Mathf.RoundToInt(pos.x / spacing);
//            int gz = Mathf.RoundToInt(pos.z / spacing);

//            tile.gridX = gx;
//            tile.gridZ = gz;
//            count++;
//        }

//        Debug.Log($"[TileGridAssigner] {count} 枚の Tile に gridX/gridZ を設定しました。");
//        EditorUtility.SetDirty(mapManager);
//    }
//}
