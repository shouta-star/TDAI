using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    [SerializeField] private LayerMask obstacleMask;   // 障害物のレイヤー
    [SerializeField] private float stepSize = 1.0f;    // 1ステップの長さ
    [SerializeField] private int maxNodes = 1000;      // セーフティ上限
    [SerializeField] private float reachThreshold = 1f;// ゴール到達距離
    [SerializeField] private float agentRadius = 0.25f;// エージェントの半径（衝突余裕）
    [SerializeField, Range(8, 180)] private int directionResolution = 72; // 探索方向数（全方向）

    private class Node
    {
        public Vector3 pos;
        public Node parent;  // null 許可
        public float g;      // 開始からの実コスト
        public float f;      // f = g + h
    }

    public Vector3[] GetPath(Vector3 startWorld, Vector3 goalWorld)
    {
        startWorld.y = goalWorld.y = 0f;

        if (Vector3.Distance(startWorld, goalWorld) <= reachThreshold)
            return new[] { goalWorld };

        var open = new List<Node>();
        var closed = new HashSet<Vector3>();

        var start = new Node
        {
            pos = startWorld,
            parent = null,
            g = 0f,
            f = Heuristic(startWorld, goalWorld)
        };
        open.Add(start);

        int iterations = 0;

        while (open.Count > 0 && iterations++ < maxNodes)
        {
            Node current = GetLowestF(open);
            open.Remove(current);
            closed.Add(current.pos);

            // ゴール到達判定
            if (Vector3.Distance(current.pos, goalWorld) <= reachThreshold)
                return ReconstructPath(current, goalWorld);

            // 近傍展開
            foreach (var next in Expand360(current, goalWorld))
            {
                if (closed.Contains(next.pos)) continue;

                // current → next に障害物があるかチェック
                Vector3 dir = (next.pos - current.pos);
                float dist = dir.magnitude;
                if (dist > 0f)
                {
                    dir /= dist;
                    if (Physics.SphereCast(current.pos, agentRadius, dir, out _, dist, obstacleMask))
                        continue;
                }

                Node same = open.Find(n => Approximately(n.pos, next.pos));
                if (same != null)
                {
                    if (next.g < same.g)
                    {
                        same.g = next.g;
                        same.f = next.f;
                        same.parent = current;
                    }
                }
                else
                {
                    open.Add(next);
                }
            }
        }

        Debug.LogWarning("[A*] 経路が見つかりませんでした。フォールバック（直行）します。");
        return new[] { goalWorld };
    }

    private float Heuristic(Vector3 a, Vector3 b) => Vector3.Distance(a, b);

    private Node GetLowestF(List<Node> list)
    {
        Node best = list[0];
        float bestF = best.f;
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].f < bestF)
            {
                best = list[i];
                bestF = list[i].f;
            }
        }
        return best;
    }

    // 360°方向に分割して探索
    private IEnumerable<Node> Expand360(Node current, Vector3 goal)
    {
        float angleStep = 360f / directionResolution;
        for (int i = 0; i < directionResolution; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 np = current.pos + dir * stepSize;

            float g = current.g + stepSize;
            float f = g + Heuristic(np, goal);

            yield return new Node
            {
                pos = np,
                parent = current,
                g = g,
                f = f
            };
        }
    }

    private Vector3[] ReconstructPath(Node goalNode, Vector3 goalWorld)
    {
        var path = new List<Vector3> { goalWorld };
        Node n = goalNode;
        while (n != null)
        {
            path.Add(n.pos);
            n = n.parent;
        }
        path.Reverse();
        return path.ToArray();
    }

    private bool Approximately(Vector3 a, Vector3 b)
        => (a - b).sqrMagnitude <= 1e-4f;
}


//using System.Collections.Generic;
//using UnityEngine;

//public class AIManager : MonoBehaviour
//{
//    [SerializeField] private LayerMask obstacleMask;   // 障害物のレイヤー
//    [SerializeField] private float stepSize = 1.0f;    // 1ステップの長さ
//    [SerializeField] private int maxNodes = 1000;      // セーフティ上限
//    [SerializeField] private float reachThreshold = 1f;// ゴール到達距離
//    [SerializeField] private float agentRadius = 0.25f;// エージェントの半径（衝突余裕）

//    // ★ struct → class（自己参照OK）
//    private class Node
//    {
//        public Vector3 pos;
//        public Node parent;  // null 許可
//        public float g;      // 開始からの実コスト
//        public float f;      // f = g + h
//    }

//    public Vector3[] GetPath(Vector3 startWorld, Vector3 goalWorld)
//    {
//        // Yをそろえて水平面で探索（必要なければ削ってOK）
//        startWorld.y = goalWorld.y = 0f;

//        if (Vector3.Distance(startWorld, goalWorld) <= reachThreshold)
//            return new[] { goalWorld };

//        // Open/Closed は位置ベースで重複排除
//        var open = new List<Node>();
//        var closed = new HashSet<Vector3>();

//        var start = new Node
//        {
//            pos = startWorld,
//            parent = null,
//            g = 0f,
//            f = Heuristic(startWorld, goalWorld)
//        };
//        open.Add(start);

//        int iterations = 0;

//        while (open.Count > 0 && iterations++ < maxNodes)
//        {
//            Node current = GetLowestF(open);
//            open.Remove(current);
//            closed.Add(current.pos);

//            // ゴール到達判定
//            if (Vector3.Distance(current.pos, goalWorld) <= reachThreshold)
//                return ReconstructPath(current, goalWorld);

//            // 近傍展開
//            foreach (var next in Expand(current, goalWorld))
//            {
//                // 既に閉じた位置はスキップ
//                if (closed.Contains(next.pos)) continue;

//                // current → next の区間に障害物があるなら無視
//                Vector3 dir = (next.pos - current.pos);
//                float dist = dir.magnitude;
//                if (dist > 0f)
//                {
//                    dir /= dist;
//                    // カプセル形状で少し太めにチェック（半径ぶん余裕）
//                    if (Physics.SphereCast(current.pos, agentRadius, dir, out _, dist, obstacleMask))
//                        continue;
//                }

//                // 既に open に同座標があれば、より良いコストの方を残す
//                Node same = open.Find(n => Approximately(n.pos, next.pos));
//                if (same != null)
//                {
//                    if (next.g < same.g)
//                    {
//                        same.g = next.g;
//                        same.f = next.f;
//                        same.parent = current;
//                    }
//                }
//                else
//                {
//                    open.Add(next);
//                }
//            }
//        }

//        Debug.LogWarning("[A*] 経路が見つかりませんでした。直行を返します。");
//        return new[] { goalWorld };
//    }

//    private float Heuristic(Vector3 a, Vector3 b) => Vector3.Distance(a, b);

//    private Node GetLowestF(List<Node> list)
//    {
//        Node best = list[0];
//        float bestF = best.f;
//        for (int i = 1; i < list.Count; i++)
//        {
//            if (list[i].f < bestF)
//            {
//                best = list[i];
//                bestF = best.f;
//            }
//        }
//        return best;
//    }

//    // 8方向へ stepSize 分だけ展開（連続空間A*）
//    private IEnumerable<Node> Expand(Node current, Vector3 goal)
//    {
//        // 水平8方向
//        Vector3[] dirs =
//        {
//            Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
//            (Vector3.forward + Vector3.left).normalized,
//            (Vector3.forward + Vector3.right).normalized,
//            (Vector3.back + Vector3.left).normalized,
//            (Vector3.back + Vector3.right).normalized
//        };

//        foreach (var d in dirs)
//        {
//            Vector3 np = current.pos + d * stepSize;
//            float g = current.g + stepSize;
//            float f = g + Heuristic(np, goal);

//            yield return new Node
//            {
//                pos = new Vector3(np.x, 0f, np.z),
//                parent = current,
//                g = g,
//                f = f
//            };
//        }
//    }

//    private Vector3[] ReconstructPath(Node goalNode, Vector3 goalWorld)
//    {
//        var path = new List<Vector3> { goalWorld };
//        Node n = goalNode;
//        while (n != null)
//        {
//            path.Add(n.pos);
//            n = n.parent;
//        }
//        path.Reverse();
//        return path.ToArray();
//    }

//    // 浮動小数の誤差で同一点判定を緩和
//    private bool Approximately(Vector3 a, Vector3 b)
//        => (a - b).sqrMagnitude <= 1e-6f;
//}

////using UnityEngine;
////using System.Collections.Generic;

////public class AIManager : MonoBehaviour
////{
////    [SerializeField] private GameObject agentPrefab;
////    [SerializeField] private Transform spawnPoint;
////    [SerializeField] private Transform goalPoint;
////    [SerializeField] private int spawnCount = 1;

////    private List<AgentController> agents = new List<AgentController>();
////    public int AgentCount => agents.Count;

////    private void Awake()
////    {
////        // MapManager不要
////    }

////    public void SpawnAgents()
////    {
////        agents.Clear();

////        for (int i = 0; i < spawnCount; i++)
////        {
////            var go = Instantiate(agentPrefab, spawnPoint.position, Quaternion.identity);
////            var agent = go.GetComponent<AgentController>();
////            agent.name = $"Agent_{i}";
////            agent.SetGoal(goalPoint);
////            agents.Add(agent);
////        }

////        Debug.Log($"[AIManager] Spawned {agents.Count} agents.");
////    }

////    /// <summary>
////    /// 仮の経路探索：スタートからゴールまで直線で返す
////    /// （後でA*またはNavMesh対応に置き換え可能）
////    /// </summary>
////    public Vector3[] GetPath(Vector3 startWorld, Vector3 goalWorld)
////    {
////        // とりあえず直線で1ステップ移動
////        return new Vector3[] { goalWorld };
////    }
////}


//////using UnityEngine;
//////using System.Collections.Generic;

//////public class AIManager : MonoBehaviour
//////{
//////    [SerializeField] private GameObject agentPrefab;
//////    [SerializeField] private Transform spawnPoint;
//////    [SerializeField] private Transform goalPoint;
//////    [SerializeField] private int spawnCount = 1;

//////    private List<AgentController> agents = new List<AgentController>();

//////    public int AgentCount => agents.Count;

//////    private void Awake()
//////    {

//////    }

//////    //public void SpawnAgents()
//////    //{
//////    //    agents.Clear();
//////    //    for (int i = 0; i < spawnCount; i++)
//////    //    {
//////    //        var go = Instantiate(agentPrefab, spawnPoint.position, Quaternion.identity);
//////    //        var agent = go.GetComponent<AgentController>();
//////    //        agent.name = $"Agent_{i}";
//////    //        agent.SetGoal(goalPoint);
//////    //        agents.Add(agent);
//////    //    }
//////    //    Debug.Log($"[AIManager] Spawned {agents.Count} agents.");
//////    //}
//////    public void SpawnAgents()
//////    {
//////        agents.Clear();
//////        for (int i = 0; i < spawnCount; i++)
//////        {
//////            var go = Instantiate(agentPrefab, spawnPoint.position, Quaternion.identity);

//////            // ★ 生成直後にマス中心にスナップ（これが重要）
//////            if (map.WorldToGrid(go.transform.position, out int gx, out int gz))
//////            {
//////                go.transform.position = map.GridToWorld(gx, gz);
//////            }

//////            var agent = go.GetComponent<AgentController>();
//////            agent.name = $"Agent_{i}";
//////            agent.SetGoal(goalPoint);
//////            agents.Add(agent);
//////        }
//////        //Debug.Log($"[AIManager] Spawned {agents.Count} agents.");
//////    }


//////    //// ===== A* : MapManager の軽量APIを使った実装 =====
//////    //public Vector3[] GetPath(Vector3 startWorld, Vector3 goalWorld)
//////    //{
//////    //    if (!map.WorldToGrid(startWorld, out int sx, out int sz)) return new[] { goalWorld };
//////    //    if (!map.WorldToGrid(goalWorld, out int gx, out int gz)) return new[] { goalWorld };

//////    //    var start = new Vector2Int(sx, sz);
//////    //    var goal = new Vector2Int(gx, gz);

//////    //    var open = new PriorityQueue<Vector2Int>();
//////    //    var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
//////    //    var g = new Dictionary<Vector2Int, float>();
//////    //    var f = new Dictionary<Vector2Int, float>();

//////    //    g[start] = 0f;
//////    //    f[start] = Heuristic(start, goal);
//////    //    open.Enqueue(start, f[start]);

//////    //    Debug.Log($"[A*] StartGrid=({sx},{sz}) GoalGrid=({gx},{gz})");

//////    //    while (open.Count > 0)
//////    //    {
//////    //        var current = open.Dequeue();
//////    //        if (current == goal) return ReconstructPath(cameFrom, current);

//////    //        foreach (var nb in map.GetNeighbors(current, diagonal: false))
//////    //        {
//////    //            if (!map.IsWalkable(nb.x, nb.y)) continue;

//////    //            float step = map.GetMoveCost(nb.x, nb.y); // cost=1 or 2（Slow）
//////    //            float tentative = g[current] + step;

//////    //            if (!g.TryGetValue(nb, out float old) || tentative < old)
//////    //            {
//////    //                cameFrom[nb] = current;
//////    //                g[nb] = tentative;
//////    //                f[nb] = tentative + Heuristic(nb, goal);
//////    //                open.EnqueueOrDecreaseKey(nb, f[nb]);
//////    //            }
//////    //        }
//////    //    }

//////    //    // 見つからない場合のフォールバック（直線）
//////    //    return new[] { goalWorld };
//////    //}
//////    public Vector3[] GetPath(Vector3 startWorld, Vector3 goalWorld)
//////    {
//////        if (!map.WorldToGrid(startWorld, out int sx, out int sz))
//////        {
//////            Debug.LogWarning("[A*] StartGridが範囲外");
//////            return new[] { startWorld }; // ★ワープ防止：ゴールに行かない
//////        }
//////        if (!map.WorldToGrid(goalWorld, out int gx, out int gz))
//////        {
//////            Debug.LogWarning("[A*] GoalGridが範囲外");
//////            return new[] { startWorld };
//////        }

//////        var start = new Vector2Int(sx, sz);
//////        var goal = new Vector2Int(gx, gz);

//////        var open = new PriorityQueue<Vector2Int>();
//////        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
//////        var g = new Dictionary<Vector2Int, float>();
//////        var f = new Dictionary<Vector2Int, float>();

//////        g[start] = 0f;
//////        f[start] = Heuristic(start, goal);
//////        open.Enqueue(start, f[start]);

//////        Debug.Log($"[A*] StartGrid=({sx},{sz}) GoalGrid=({gx},{gz})");

//////        while (open.Count > 0)
//////        {
//////            var current = open.Dequeue();
//////            if (current == goal)
//////            {
//////                var path = ReconstructPath(cameFrom, current);
//////                Debug.Log($"[A*] Path length = {path.Length}");
//////                return path;
//////            }

//////            foreach (var nb in map.GetNeighbors(current, diagonal: false)) // ★上下左右のみ
//////            {
//////                if (!map.IsWalkable(nb.x, nb.y)) continue;

//////                float step = map.GetMoveCost(nb.x, nb.y);
//////                float tentative = g[current] + step;

//////                if (!g.TryGetValue(nb, out float old) || tentative < old)
//////                {
//////                    cameFrom[nb] = current;
//////                    g[nb] = tentative;
//////                    f[nb] = tentative + Heuristic(nb, goal);
//////                    open.EnqueueOrDecreaseKey(nb, f[nb]);
//////                }
//////            }
//////        }

//////        Debug.LogWarning("[A*] 経路が見つかりませんでした。フォールバックします。");
//////        return new[] { startWorld }; // ★その場に留まる
//////    }


//////    private float Heuristic(Vector2Int a, Vector2Int b)
//////    {
//////        // 4近傍マンハッタン
//////        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
//////    }

//////    private Vector3[] ReconstructPath(Dictionary<Vector2Int, Vector2Int> came, Vector2Int cur)
//////    {
//////        var list = new List<Vector3>();
//////        while (came.TryGetValue(cur, out var prev))
//////        {
//////            list.Add(map.GridToWorld(cur.x, cur.y));
//////            cur = prev;
//////        }
//////        list.Reverse();

//////        // ★ 経路の確認ログを出す
//////        Debug.Log("AAA");
//////        Debug.Log($"[A*] Path length = {list.Count}");
//////        foreach (var p in list)
//////            Debug.Log($"[A*] → {p}");

//////        return list.ToArray();
//////    }

//////    // ===== 最小限の優先度付きキュー =====
//////    private class PriorityQueue<T>
//////    {
//////        private readonly List<(T item, float pri)> heap = new();

//////        public int Count => heap.Count;

//////        public void Enqueue(T item, float priority)
//////        {
//////            heap.Add((item, priority));
//////            Up(heap.Count - 1);
//////        }

//////        public void EnqueueOrDecreaseKey(T item, float priority)
//////        {
//////            int idx = heap.FindIndex(p => EqualityComparer<T>.Default.Equals(p.item, item));
//////            if (idx >= 0 && priority < heap[idx].pri)
//////            {
//////                heap[idx] = (item, priority);
//////                Up(idx);
//////            }
//////            else if (idx < 0)
//////            {
//////                Enqueue(item, priority);
//////            }
//////        }

//////        public T Dequeue()
//////        {
//////            var root = heap[0].item;
//////            var last = heap[^1];
//////            heap.RemoveAt(heap.Count - 1);
//////            if (heap.Count > 0)
//////            {
//////                heap[0] = last;
//////                Down(0);
//////            }
//////            return root;
//////        }

//////        private void Up(int i)
//////        {
//////            while (i > 0)
//////            {
//////                int p = (i - 1) / 2;
//////                if (heap[p].pri <= heap[i].pri) break;
//////                (heap[i], heap[p]) = (heap[p], heap[i]);
//////                i = p;
//////            }
//////        }

//////        private void Down(int i)
//////        {
//////            int n = heap.Count;
//////            while (true)
//////            {
//////                int l = i * 2 + 1, r = l + 1, s = i;
//////                if (l < n && heap[l].pri < heap[s].pri) s = l;
//////                if (r < n && heap[r].pri < heap[s].pri) s = r;
//////                if (s == i) break;
//////                (heap[i], heap[s]) = (heap[s], heap[i]);
//////                i = s;
//////            }
//////        }
//////    }
//////}


////////using UnityEngine;
////////using System.Collections.Generic;

////////public class AIManager : MonoBehaviour
////////{
////////    [SerializeField] private GameObject agentPrefab;
////////    [SerializeField] private Transform spawnPoint;
////////    [SerializeField] private Transform goalPoint;
////////    [SerializeField] private int spawnCount = 1;

////////    private List<AgentController> agents = new List<AgentController>();
////////    private MapManager mapManager;

////////    public int AgentCount => agents.Count;

////////    private void Awake()
////////    {
////////        mapManager = FindObjectOfType<MapManager>();
////////    }

////////    public void SpawnAgents()
////////    {
////////        for (int i = 0; i < spawnCount; i++)
////////        {
////////            GameObject agentObj = Instantiate(agentPrefab, spawnPoint.position, Quaternion.identity);
////////            AgentController agent = agentObj.GetComponent<AgentController>();
////////            agent.name = $"Agent_{i}";
////////            agent.SetGoal(goalPoint);
////////            agents.Add(agent);
////////        }

////////        Debug.Log($"[AIManager] エージェントを {spawnCount} 体生成しました");
////////    }

////////    // ======== A*アルゴリズム本体 =========
////////    public Vector3[] GetPath(Vector3 start, Vector3 goal)
////////    {
////////        Node startNode = mapManager.GetClosestNode(start);
////////        Node goalNode = mapManager.GetClosestNode(goal);

////////        List<Node> openSet = new List<Node>();
////////        HashSet<Node> closedSet = new HashSet<Node>();
////////        openSet.Add(startNode);

////////        while (openSet.Count > 0)
////////        {
////////            Node currentNode = openSet[0];
////////            for (int i = 1; i < openSet.Count; i++)
////////            {
////////                if (openSet[i].fCost < currentNode.fCost ||
////////                   (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
////////                {
////////                    currentNode = openSet[i];
////////                }
////////            }

////////            openSet.Remove(currentNode);
////////            closedSet.Add(currentNode);

////////            if (currentNode == goalNode)
////////            {
////////                return RetracePath(startNode, goalNode);
////////            }

////////            foreach (Node neighbor in mapManager.GetNeighbors(currentNode))
////////            {
////////                if (closedSet.Contains(neighbor)) continue;

////////                float newCost = currentNode.gCost + Vector3.Distance(currentNode.worldPosition, neighbor.worldPosition);
////////                if (newCost < neighbor.gCost || !openSet.Contains(neighbor))
////////                {
////////                    neighbor.gCost = newCost;
////////                    neighbor.hCost = Vector3.Distance(neighbor.worldPosition, goalNode.worldPosition);
////////                    neighbor.parent = currentNode;

////////                    if (!openSet.Contains(neighbor))
////////                        openSet.Add(neighbor);
////////                }
////////            }
////////        }

////////        Debug.LogWarning("[AIManager] 経路が見つかりませんでした");
////////        return new Vector3[] { goal }; // フォールバック
////////    }

////////    private Vector3[] RetracePath(Node startNode, Node endNode)
////////    {
////////        List<Vector3> path = new List<Vector3>();
////////        Node currentNode = endNode;

////////        while (currentNode != startNode)
////////        {
////////            path.Add(currentNode.worldPosition);
////////            currentNode = currentNode.parent;
////////        }

////////        path.Reverse();
////////        return path.ToArray();
////////    }
////////}


//////////using UnityEngine;
//////////using System.Collections.Generic;

//////////public class AIManager : MonoBehaviour
//////////{
//////////    [SerializeField] private GameObject agentPrefab;
//////////    [SerializeField] private Transform spawnPoint;
//////////    [SerializeField] private Transform goalPoint;
//////////    [SerializeField] private int spawnCount = 1;

//////////    private List<AgentController> agents = new List<AgentController>();

//////////    public int AgentCount => agents.Count;

//////////    public void SpawnAgents()
//////////    {
//////////        for (int i = 0; i < spawnCount; i++)
//////////        {
//////////            GameObject agentObj = Instantiate(agentPrefab, spawnPoint.position, Quaternion.identity);
//////////            AgentController agent = agentObj.GetComponent<AgentController>();
//////////            agent.name = $"Agent_{i}";
//////////            agent.SetGoal(goalPoint);
//////////            agents.Add(agent);
//////////        }

//////////        Debug.Log($"[AIManager] エージェントを {spawnCount} 体生成しました");
//////////    }

//////////    // 仮の経路探索：スタートからゴールまで直線
//////////    public Vector3[] GetPath(Vector3 start, Vector3 goal)
//////////    {
//////////        return new Vector3[] { goal };
//////////    }
//////////}
