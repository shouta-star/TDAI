using UnityEngine;
using System.Collections.Generic;

public class HeatmapManager : MonoBehaviour
{
    public static HeatmapManager Instance;

    private List<Vector3> positions = new List<Vector3>();

    private void Awake()
    {
        Instance = this;
    }

    public void RecordPosition(Vector3 pos)
    {
        positions.Add(pos);
    }

    public void GenerateHeatmap()
    {
        Debug.Log($"[HeatmapManager] 通過点数: {positions.Count}");
        // 実際のヒートマップ生成処理は後で追加
    }
}
