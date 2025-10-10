using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [SerializeField] private int mapSize = 10;
    [SerializeField] private float nodeSpacing = 1f;

    private Node[,] grid;

    public void GenerateMap()
    {
        grid = new Node[mapSize, mapSize];

        for (int x = 0; x < mapSize; x++)
        {
            for (int z = 0; z < mapSize; z++)
            {
                Vector3 pos = new Vector3(x * nodeSpacing, 0, z * nodeSpacing);
                grid[x, z] = new Node(pos, true); // 全マス通行可
            }
        }

        Debug.Log("[MapManager] マップ生成完了（A*対応）");
    }

    public Node GetClosestNode(Vector3 position)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(position.x / nodeSpacing), 0, grid.GetLength(0) - 1);
        int z = Mathf.Clamp(Mathf.RoundToInt(position.z / nodeSpacing), 0, grid.GetLength(1) - 1);
        return grid[x, z];
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;

                int x = node.gridX + dx;
                int z = node.gridZ + dz;

                if (x >= 0 && x < grid.GetLength(0) && z >= 0 && z < grid.GetLength(1))
                {
                    Node neighbor = grid[x, z];
                    if (neighbor.walkable)
                        neighbors.Add(neighbor);
                }
            }
        }
        return neighbors;
    }
}

public class Node
{
    public Vector3 worldPosition;
    public bool walkable;
    public int gridX, gridZ;
    public float gCost, hCost;
    public Node parent;

    public float fCost => gCost + hCost;

    public Node(Vector3 pos, bool walkable)
    {
        this.worldPosition = pos;
        this.walkable = walkable;
    }
}

//using UnityEngine;

//public class MapManager : MonoBehaviour
//{
//    [SerializeField] private int mapSize = 10;
//    [SerializeField] private float nodeSpacing = 1f;

//    private bool[,] walkableGrid;

//    public void GenerateMap()
//    {
//        walkableGrid = new bool[mapSize, mapSize];

//        for (int x = 0; x < mapSize; x++)
//        {
//            for (int z = 0; z < mapSize; z++)
//            {
//                walkableGrid[x, z] = true; // 全マス通行可能（A*実装時に変更予定）
//            }
//        }

//        Debug.Log("[MapManager] マップ生成完了");
//    }

//    public bool IsWalkable(Vector3 position)
//    {
//        int x = Mathf.RoundToInt(position.x);
//        int z = Mathf.RoundToInt(position.z);

//        if (x < 0 || x >= mapSize || z < 0 || z >= mapSize)
//            return false;

//        return walkableGrid[x, z];
//    }
//}
