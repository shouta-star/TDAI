using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] private int mapSize = 10;
    [SerializeField] private float nodeSpacing = 1f;

    private bool[,] walkableGrid;

    public void GenerateMap()
    {
        walkableGrid = new bool[mapSize, mapSize];

        for (int x = 0; x < mapSize; x++)
        {
            for (int z = 0; z < mapSize; z++)
            {
                walkableGrid[x, z] = true; // 全マス通行可能（A*実装時に変更予定）
            }
        }

        Debug.Log("[MapManager] マップ生成完了");
    }

    public bool IsWalkable(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x);
        int z = Mathf.RoundToInt(position.z);

        if (x < 0 || x >= mapSize || z < 0 || z >= mapSize)
            return false;

        return walkableGrid[x, z];
    }
}
