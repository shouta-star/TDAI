using UnityEngine;
using System;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public Transform mapRoot;      // すべての Tile をぶら下げた親
    public int sizeX;
    public int sizeZ ;
    public float nodeSpacing = 1f;

    [Header("Entrances & Goal")]
    public Transform entrance;     // 単一入口の例（複数にするなら配列で持つ）
    public Transform goal;

    // 索引
    private TileComponent[,] tiles;

    // 変更通知（単発 / バッチ）
    public struct TileChangedArgs { public int x, z; public TileType from, to; }
    public event Action<TileChangedArgs> TileChanged;
    public event Action<List<TileChangedArgs>> TilesChanged;

    public void GenerateIndexFromScene()
    {
        // 手動配置された Tile をシーンから集めてグリッドに索引化
        tiles = new TileComponent[sizeX, sizeZ];

        var allTiles = mapRoot.GetComponentsInChildren<TileComponent>(true);
        int registered = 0;
        foreach (var t in allTiles)
        {
            if (InRange(t.gridX, t.gridZ))
            {
                tiles[t.gridX, t.gridZ] = t;
                // 位置合わせ（中心）
                var pos = new Vector3(t.gridX * nodeSpacing, t.transform.position.y, t.gridZ * nodeSpacing);
                t.transform.position = pos;
                registered++;
            }
            else
            {
                Debug.LogWarning($"[MapManager] Tile out of range ({t.gridX},{t.gridZ})");
            }
        }
        Debug.Log($"[MapManager] Indexed {registered}/{allTiles.Length} tiles.");
    }

    private void Awake()
    {
        if (!mapRoot) mapRoot = transform;
    }

    private void Start()
    {
        if (tiles == null) GenerateIndexFromScene();
    }

    public bool InRange(int x, int z) => (x >= 0 && x < sizeX && z >= 0 && z < sizeZ);

    // ==== A* 用 軽量API ====
    public bool IsWalkable(int x, int z)
    {
        if (!InRange(x, z)) return false;
        var t = tiles[x, z];
        return t != null && t.walkable;
    }

    public float GetMoveCost(int x, int z)
    {
        if (!InRange(x, z)) return Mathf.Infinity;
        var t = tiles[x, z];
        return (t != null) ? t.moveCost : Mathf.Infinity;
    }

    //public IEnumerable<Vector2Int> GetNeighbors(Vector2Int node, bool diagonal = true)
    //{
    //    // 8方向 or 4方向
    //    for (int dx = -1; dx <= 1; dx++)
    //    {
    //        for (int dz = -1; dz <= 1; dz++)
    //        {
    //            if (dx == 0 && dz == 0) continue;
    //            if (!diagonal && Mathf.Abs(dx) + Mathf.Abs(dz) != 1) continue;

    //            int nx = node.x + dx;
    //            int nz = node.y + dz;
    //            if (!InRange(nx, nz)) continue;
    //            yield return new Vector2Int(nx, nz);
    //        }
    //    }
    //}
    public IEnumerable<Vector2Int> GetNeighbors(Vector2Int node, bool diagonal = true)
    {
        if (diagonal)
        {
            // 8方向
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dz == 0) continue;

                    int nx = node.x + dx;
                    int nz = node.y + dz;
                    if (!InRange(nx, nz)) continue;
                    yield return new Vector2Int(nx, nz);
                }
            }
        }
        else
        {
            // 4方向のみ（上下左右）
            yield return new Vector2Int(node.x + 1, node.y);
            yield return new Vector2Int(node.x - 1, node.y);
            yield return new Vector2Int(node.x, node.y + 1);
            yield return new Vector2Int(node.x, node.y - 1);
        }
    }

    public Vector3 GridToWorld(int x, int z)
    {
        return new Vector3(x * nodeSpacing, 0f, z * nodeSpacing);
    }

    public bool WorldToGrid(Vector3 world, out int x, out int z)
    {
        x = Mathf.RoundToInt(world.x / nodeSpacing);
        z = Mathf.RoundToInt(world.z / nodeSpacing);
        return InRange(x, z);
    }

    public bool TryGetTile(int x, int z, out TileComponent tile)
    {
        tile = null;
        if (!InRange(x, z)) return false;
        tile = tiles[x, z];
        return tile != null;
    }

    // ==== 変更API ====
    public bool CanChange(int x, int z, TileType toType, out string reason)
    {
        reason = null;
        if (!InRange(x, z)) { reason = "範囲外"; return false; }
        if (!TryGetTile(x, z, out var tile)) { reason = "タイル未登録"; return false; }

        // 例: Start/Goal は変更禁止（必要に応じて緩和）
        if (tile.type == TileType.Start || tile.type == TileType.Goal)
        { reason = "Start/Goal は変更不可"; return false; }

        // 仮適用して到達可能性チェック（封鎖防止）
        var fromType = tile.type;
        var fromWalkable = tile.walkable;
        var fromCost = tile.moveCost;

        // 仮の論理値
        var (tmpWalkable, tmpCost) = Simulate(toType);
        tile.walkable = tmpWalkable;
        tile.moveCost = tmpCost;

        bool reachable = CheckReachableAfterChange();

        // 戻す
        tile.walkable = fromWalkable;
        tile.moveCost = fromCost;

        if (!reachable)
        {
            reason = "入口からGoalまでの経路が途切れるため不可";
            return false;
        }

        return true;
    }

    public bool ApplyChange(int x, int z, TileType toType)
    {
        if (!InRange(x, z)) return false;
        if (!TryGetTile(x, z, out var tile)) return false;

        var from = tile.type;

        // 実適用（見た目含む）
        tile.ApplyType(toType);

        TileChanged?.Invoke(new TileChangedArgs { x = x, z = z, from = from, to = toType });
        return true;
    }

    public void ApplyChangesBatch(List<(int x, int z, TileType to)> requests)
    {
        var argsList = new List<TileChangedArgs>(requests.Count);
        foreach (var r in requests)
        {
            if (!TryGetTile(r.x, r.z, out var tile)) continue;
            var from = tile.type;
            tile.ApplyType(r.to);
            argsList.Add(new TileChangedArgs { x = r.x, z = r.z, from = from, to = r.to });
        }
        if (argsList.Count > 0) TilesChanged?.Invoke(argsList);
    }

    private (bool walkable, float cost) Simulate(TileType to)
    {
        switch (to)
        {
            case TileType.Floor: return (true, 1f);
            case TileType.Wall: return (false, Mathf.Infinity);
            case TileType.Danger: return (true, 1f);
            case TileType.Slow: return (true, 2f);
            case TileType.Start: return (true, 1f);
            case TileType.Goal: return (true, 1f);
        }
        return (true, 1f);
    }

    // ===== 到達可能性チェック（BFS）=====
    private bool CheckReachableAfterChange()
    {
        // 単一入口前提（複数なら全入口→Goal の OR）
        if (!WorldToGrid(entrance.position, out int sx, out int sz)) return false;
        if (!WorldToGrid(goal.position, out int gx, out int gz)) return false;

        if (!IsWalkable(sx, sz) || !IsWalkable(gx, gz)) return false;

        var q = new Queue<Vector2Int>();
        var seen = new bool[sizeX, sizeZ];
        q.Enqueue(new Vector2Int(sx, sz));
        seen[sx, sz] = true;

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur.x == gx && cur.y == gz) return true;

            foreach (var nb in GetNeighbors(cur, diagonal: false))
            {
                if (seen[nb.x, nb.y]) continue;
                if (!IsWalkable(nb.x, nb.y)) continue;
                seen[nb.x, nb.y] = true;
                q.Enqueue(nb);
            }
        }
        return false;
    }
}


//using UnityEngine;
//using System.Collections.Generic;

//public class MapManager : MonoBehaviour
//{
//    [SerializeField] private int mapSize = 10;
//    [SerializeField] private float nodeSpacing = 1f;

//    private Node[,] grid;

//    public void GenerateMap()
//    {
//        grid = new Node[mapSize, mapSize];

//        for (int x = 0; x < mapSize; x++)
//        {
//            for (int z = 0; z < mapSize; z++)
//            {
//                Vector3 pos = new Vector3(x * nodeSpacing, 0, z * nodeSpacing);
//                grid[x, z] = new Node(pos, true); // 全マス通行可
//            }
//        }

//        Debug.Log("[MapManager] マップ生成完了（A*対応）");
//    }

//    public Node GetClosestNode(Vector3 position)
//    {
//        int x = Mathf.Clamp(Mathf.RoundToInt(position.x / nodeSpacing), 0, grid.GetLength(0) - 1);
//        int z = Mathf.Clamp(Mathf.RoundToInt(position.z / nodeSpacing), 0, grid.GetLength(1) - 1);
//        return grid[x, z];
//    }

//    public List<Node> GetNeighbors(Node node)
//    {
//        List<Node> neighbors = new List<Node>();

//        for (int dx = -1; dx <= 1; dx++)
//        {
//            for (int dz = -1; dz <= 1; dz++)
//            {
//                if (dx == 0 && dz == 0) continue;

//                int x = node.gridX + dx;
//                int z = node.gridZ + dz;

//                if (x >= 0 && x < grid.GetLength(0) && z >= 0 && z < grid.GetLength(1))
//                {
//                    Node neighbor = grid[x, z];
//                    if (neighbor.walkable)
//                        neighbors.Add(neighbor);
//                }
//            }
//        }
//        return neighbors;
//    }
//}

//public class Node
//{
//    public Vector3 worldPosition;
//    public bool walkable;
//    public int gridX, gridZ;
//    public float gCost, hCost;
//    public Node parent;

//    public float fCost => gCost + hCost;

//    public Node(Vector3 pos, bool walkable)
//    {
//        this.worldPosition = pos;
//        this.walkable = walkable;
//    }
//}