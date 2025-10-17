using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    [SerializeField] private float stepSize = 1.0f;
    [SerializeField] private int maxNodes = 1000;
    [SerializeField] private float reachThreshold = 1f;
    [SerializeField, Range(8, 180)] private int directionResolution = 72;

    [Header("Path Request Settings")]
    [SerializeField] private float processInterval = 0.02f; // 経路処理間隔(秒)

    public static AIManager Instance { get; private set; }

    private float lastProcessTime = 0f;
    private readonly Queue<AgentController> pathRequestQueue = new Queue<AgentController>();

    private class Obstacle
    {
        public Vector3 pos;
        public float radius;
    }
    private readonly List<Obstacle> obstacles = new List<Obstacle>();

    private void Awake() => Instance = this;

    private void Update()
    {
        // 経路リクエストを順に処理
        if (pathRequestQueue.Count > 0 && Time.time - lastProcessTime >= processInterval)
        {
            var agent = pathRequestQueue.Dequeue();
            ProcessPath(agent);
            lastProcessTime = Time.time;
        }
    }

    // ===========================================================
    // 経路リクエストの受付
    // ===========================================================
    public void EnqueuePathRequest(AgentController agent)
    {
        if (agent == null) return;
        if (!pathRequestQueue.Contains(agent))
            pathRequestQueue.Enqueue(agent);
    }

    private void ProcessPath(AgentController agent)
    {
        Vector3 start = agent.transform.position;
        Vector3 goal = agent.GetGoalPosition();

        var data = agent.GetData();
        var path = GetPath(start, goal, data.aiType);
        if (path != null && path.Length > 0)
        {
            agent.SetPath(path);
        }
    }

    // ===========================================================
    // 動的障害物管理
    // ===========================================================
    public void OnDynamicObstacleChanged(Vector3 position, float radius, bool isBlocked)
    {
        position.y = 0f;

        if (isBlocked)
            obstacles.Add(new Obstacle { pos = position, radius = radius });
        else
            obstacles.RemoveAll(o => Vector3.Distance(o.pos, position) < 0.01f);

        foreach (var agent in FindObjectsOfType<AgentController>())
        {
            var data = agent.GetData();
            if (data != null && data.aiType == AIType.DStar)
                agent.OnDynamicMapChanged(position, isBlocked);
        }
    }

    private bool IsObstacleBetween(Vector3 a, Vector3 b)
    {
        foreach (var obs in obstacles)
        {
            Vector3 closest = ClosestPointOnSegment(obs.pos, a, b);
            if (Vector3.Distance(obs.pos, closest) <= obs.radius)
                return true;
        }
        return false;
    }

    private Vector3 ClosestPointOnSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    // ===========================================================
    // 経路探索本体（A* / D*）
    // ===========================================================
    public Vector3[] GetPath(Vector3 startWorld, Vector3 goalWorld, AIType type)
    {
        switch (type)
        {
            case AIType.DStar:
                return GetPathDStar(startWorld, goalWorld);
            case AIType.AStar:
            default:
                return GetPathAStar(startWorld, goalWorld);
        }
    }

    private Vector3[] GetPathAStar(Vector3 startWorld, Vector3 goalWorld)
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

            if (Vector3.Distance(current.pos, goalWorld) <= reachThreshold)
                return ReconstructPath(current, goalWorld);

            foreach (var next in Expand360(current, goalWorld))
            {
                if (closed.Contains(next.pos)) continue;
                if (IsObstacleBetween(current.pos, next.pos)) continue;

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
        return new[] { goalWorld };
    }

    private Vector3[] GetPathDStar(Vector3 startWorld, Vector3 goalWorld)
    {
        var path = GetPathAStar(startWorld, goalWorld);
        if (path == null || path.Length == 0) return path;

        for (int i = 0; i < path.Length - 1; i++)
        {
            if (IsObstacleBetween(path[i], path[i + 1]))
                return GetPathAStar(path[i], goalWorld);
        }
        return path;
    }

    private class Node
    {
        public Vector3 pos;
        public Node parent;
        public float g;
        public float f;
    }

    private float Heuristic(Vector3 a, Vector3 b) => Vector3.Distance(a, b);
    private Node GetLowestF(List<Node> list)
    {
        Node best = list[0];
        for (int i = 1; i < list.Count; i++)
            if (list[i].f < best.f)
                best = list[i];
        return best;
    }

    private IEnumerable<Node> Expand360(Node current, Vector3 goal)
    {
        float step = 360f / directionResolution;
        for (int i = 0; i < directionResolution; i++)
        {
            float angle = i * step;
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 np = current.pos + dir * stepSize;

            float g = current.g + stepSize;
            float f = g + Heuristic(np, goal);

            yield return new Node { pos = np, parent = current, g = g, f = f };
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

    private bool Approximately(Vector3 a, Vector3 b) => (a - b).sqrMagnitude <= 1e-4f;
    public bool HasObstacleBetween(Vector3 a, Vector3 b) => IsObstacleBetween(a, b);
}


//using System.Collections.Generic;
//using UnityEngine;

//public class AIManager : MonoBehaviour
//{
//    [SerializeField] private float stepSize = 1.0f;
//    [SerializeField] private int maxNodes = 1000;
//    [SerializeField] private float reachThreshold = 1f;
//    [SerializeField, Range(8, 180)] private int directionResolution = 72;

//    public static AIManager Instance { get; private set; }

//    // 障害物を位置＋半径で管理
//    private class Obstacle
//    {
//        public Vector3 pos;
//        public float radius;
//    }
//    private readonly List<Obstacle> obstacles = new List<Obstacle>();

//    private void Awake() => Instance = this;

//    // ===========================================================
//    //  動的障害物登録（MovingWallから通知）
//    // ===========================================================
//    public void OnDynamicObstacleChanged(Vector3 position, float radius, bool isBlocked)
//    {
//        position.y = 0f;

//        if (isBlocked)
//        {
//            obstacles.Add(new Obstacle { pos = position, radius = radius });
//            Debug.Log($"[AIManager] 壁追加 at {position}");
//        }
//        else
//        {
//            obstacles.RemoveAll(o => Vector3.Distance(o.pos, position) < 0.01f);
//            Debug.Log($"[AIManager] 壁解除 at {position}");
//        }

//        // D*のみ即時再探索
//        foreach (var agent in FindObjectsOfType<AgentController>())
//        {
//            var data = agent.GetData();
//            if (data != null && data.aiType == AIType.DStar)
//                agent.OnDynamicMapChanged(position, isBlocked);
//        }
//    }

//    // ===========================================================
//    //  経路探索中の障害物チェック（Collider非依存）
//    // ===========================================================
//    private bool IsObstacleBetween(Vector3 a, Vector3 b)
//    {
//        foreach (var obs in obstacles)
//        {
//            Vector3 closest = ClosestPointOnSegment(obs.pos, a, b);
//            if (Vector3.Distance(obs.pos, closest) <= obs.radius)
//                return true;
//        }
//        return false;
//    }

//    private Vector3 ClosestPointOnSegment(Vector3 p, Vector3 a, Vector3 b)
//    {
//        Vector3 ab = b - a;
//        float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
//        t = Mathf.Clamp01(t);
//        return a + ab * t;
//    }

//    // ===========================================================
//    //  A* / D* 経路探索の切り替え
//    // ===========================================================
//    public Vector3[] GetPath(Vector3 startWorld, Vector3 goalWorld, AIType type)
//    {
//        switch (type)
//        {
//            case AIType.DStar:
//                return GetPathDStar(startWorld, goalWorld);
//            case AIType.AStar:
//            default:
//                return GetPathAStar(startWorld, goalWorld);
//        }
//    }

//    // ===========================================================
//    //  A*アルゴリズム
//    // ===========================================================
//    private Vector3[] GetPathAStar(Vector3 startWorld, Vector3 goalWorld)
//    {
//        startWorld.y = goalWorld.y = 0f;

//        if (Vector3.Distance(startWorld, goalWorld) <= reachThreshold)
//            return new[] { goalWorld };

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

//            if (Vector3.Distance(current.pos, goalWorld) <= reachThreshold)
//                return ReconstructPath(current, goalWorld);

//            foreach (var next in Expand360(current, goalWorld))
//            {
//                if (closed.Contains(next.pos)) continue;

//                if (IsObstacleBetween(current.pos, next.pos))
//                    continue;

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

//        Debug.LogWarning("[A*] 経路が見つかりませんでした。");
//        return new[] { goalWorld };
//    }

//    // ===========================================================
//    //  D*アルゴリズム（簡易再探索版）
//    // ===========================================================
//    private Vector3[] GetPathDStar(Vector3 startWorld, Vector3 goalWorld)
//    {
//        var path = GetPathAStar(startWorld, goalWorld);
//        if (path == null || path.Length == 0) return path;

//        for (int i = 0; i < path.Length - 1; i++)
//        {
//            if (IsObstacleBetween(path[i], path[i + 1]))
//            {
//                Debug.Log("[D*] 動的障害物を検知 → 再探索");
//                return GetPathAStar(path[i], goalWorld);
//            }
//        }
//        return path;
//    }

//    // ===========================================================
//    //  経路探索用ノード構造体
//    // ===========================================================
//    private class Node
//    {
//        public Vector3 pos;
//        public Node parent;
//        public float g;
//        public float f;
//    }

//    // ===========================================================
//    //  共通ユーティリティ
//    // ===========================================================
//    private float Heuristic(Vector3 a, Vector3 b) => Vector3.Distance(a, b);

//    private Node GetLowestF(List<Node> list)
//    {
//        Node best = list[0];
//        for (int i = 1; i < list.Count; i++)
//            if (list[i].f < best.f)
//                best = list[i];
//        return best;
//    }

//    private IEnumerable<Node> Expand360(Node current, Vector3 goal)
//    {
//        float step = 360f / directionResolution;
//        for (int i = 0; i < directionResolution; i++)
//        {
//            float angle = i * step;
//            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
//            Vector3 np = current.pos + dir * stepSize;

//            float g = current.g + stepSize;
//            float f = g + Heuristic(np, goal);

//            yield return new Node
//            {
//                pos = np,
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

//    private bool Approximately(Vector3 a, Vector3 b)
//        => (a - b).sqrMagnitude <= 1e-4f;

//    // ===========================================================
//    //  外部アクセス用ヘルパー（AgentControllerから使用）
//    // ===========================================================
//    public bool HasObstacleBetween(Vector3 a, Vector3 b)
//    {
//        return IsObstacleBetween(a, b);
//    }
//}


////using System.Collections.Generic;
////using UnityEngine;

////public class AIManager : MonoBehaviour
////{
////    [SerializeField] private float stepSize = 1.0f;
////    [SerializeField] private int maxNodes = 1000;
////    [SerializeField] private float reachThreshold = 1f;
////    [SerializeField, Range(8, 180)] private int directionResolution = 72;

////    public static AIManager Instance { get; private set; }

////    // Colliderを使わない代わりに障害物位置リストを保持
////    private readonly HashSet<Vector3> obstaclePositions = new HashSet<Vector3>();

////    private void Awake()
////    {
////        Instance = this;
////    }

////    // ====== Nodeクラス ======
////    private class Node
////    {
////        public Vector3 pos;
////        public Node parent;
////        public float g;
////        public float f;
////    }

////    // ===========================================================
////    //  動的障害物登録（MovingWallから通知を受け取る）
////    // ===========================================================
////    public void OnDynamicObstacleChanged(Vector3 position, bool isBlocked)
////    {
////        position.y = 0f; // 統一

////        if (isBlocked)
////        {
////            obstaclePositions.Add(position);
////            Debug.Log($"[AIManager] 壁追加 at {position}");
////        }
////        else
////        {
////            obstaclePositions.Remove(position);
////            Debug.Log($"[AIManager] 壁解除 at {position}");
////        }

////        // D*のみ即時再探索を発動
////        foreach (var agent in FindObjectsOfType<AgentController>())
////        {
////            var data = agent.GetData();
////            if (data != null && data.aiType == AIType.DStar)
////            {
////                agent.OnDynamicMapChanged(position, isBlocked);
////            }
////        }
////    }

////    // ===========================================================
////    //  A* / D* 経路探索の切り替え
////    // ===========================================================
////    public Vector3[] GetPath(Vector3 startWorld, Vector3 goalWorld, AIType type)
////    {
////        switch (type)
////        {
////            case AIType.DStar:
////                return GetPathDStar(startWorld, goalWorld);
////            case AIType.AStar:
////            default:
////                return GetPathAStar(startWorld, goalWorld);
////        }
////    }

////    // ===========================================================
////    //  A*アルゴリズム
////    // ===========================================================
////    private Vector3[] GetPathAStar(Vector3 startWorld, Vector3 goalWorld)
////    {
////        startWorld.y = goalWorld.y = 0f;

////        if (Vector3.Distance(startWorld, goalWorld) <= reachThreshold)
////            return new[] { goalWorld };

////        var open = new List<Node>();
////        var closed = new HashSet<Vector3>();

////        var start = new Node
////        {
////            pos = startWorld,
////            parent = null,
////            g = 0f,
////            f = Heuristic(startWorld, goalWorld)
////        };
////        open.Add(start);

////        int iterations = 0;

////        while (open.Count > 0 && iterations++ < maxNodes)
////        {
////            Node current = GetLowestF(open);
////            open.Remove(current);
////            closed.Add(current.pos);

////            if (Vector3.Distance(current.pos, goalWorld) <= reachThreshold)
////                return ReconstructPath(current, goalWorld);

////            foreach (var next in Expand360(current, goalWorld))
////            {
////                if (closed.Contains(next.pos)) continue;

////                // 障害物に衝突する場合スキップ
////                if (IsObstacleBetween(current.pos, next.pos))
////                    continue;

////                Node same = open.Find(n => Approximately(n.pos, next.pos));
////                if (same != null)
////                {
////                    if (next.g < same.g)
////                    {
////                        same.g = next.g;
////                        same.f = next.f;
////                        same.parent = current;
////                    }
////                }
////                else
////                {
////                    open.Add(next);
////                }
////            }
////        }

////        Debug.LogWarning("[A*] 経路が見つかりませんでした。");
////        return new[] { goalWorld };
////    }

////    // ===========================================================
////    //  D*アルゴリズム（簡易再探索型）
////    // ===========================================================
////    private Vector3[] GetPathDStar(Vector3 startWorld, Vector3 goalWorld)
////    {
////        var path = GetPathAStar(startWorld, goalWorld);
////        if (path == null || path.Length == 0) return path;

////        for (int i = 0; i < path.Length - 1; i++)
////        {
////            if (IsObstacleBetween(path[i], path[i + 1]))
////            {
////                Debug.Log("[D*] 動的障害物を検知 → 再探索");
////                return GetPathAStar(path[i], goalWorld);
////            }
////        }
////        return path;
////    }

////    // ===========================================================
////    //  障害物判定（Collider非依存）
////    // ===========================================================
////    private bool IsObstacleBetween(Vector3 a, Vector3 b)
////    {
////        foreach (var obs in obstaclePositions)
////        {
////            if (Vector3.Distance(obs, (a + b) / 2f) < stepSize * 0.5f)
////                return true;
////        }
////        return false;
////    }

////    // ===========================================================
////    //  共通ユーティリティ
////    // ===========================================================
////    private float Heuristic(Vector3 a, Vector3 b) => Vector3.Distance(a, b);

////    private Node GetLowestF(List<Node> list)
////    {
////        Node best = list[0];
////        for (int i = 1; i < list.Count; i++)
////            if (list[i].f < best.f)
////                best = list[i];
////        return best;
////    }

////    private IEnumerable<Node> Expand360(Node current, Vector3 goal)
////    {
////        float step = 360f / directionResolution;
////        for (int i = 0; i < directionResolution; i++)
////        {
////            float angle = i * step;
////            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
////            Vector3 np = current.pos + dir * stepSize;

////            float g = current.g + stepSize;
////            float f = g + Heuristic(np, goal);

////            yield return new Node
////            {
////                pos = np,
////                parent = current,
////                g = g,
////                f = f
////            };
////        }
////    }

////    private Vector3[] ReconstructPath(Node goalNode, Vector3 goalWorld)
////    {
////        var path = new List<Vector3> { goalWorld };
////        Node n = goalNode;
////        while (n != null)
////        {
////            path.Add(n.pos);
////            n = n.parent;
////        }
////        path.Reverse();
////        return path.ToArray();
////    }

////    private bool Approximately(Vector3 a, Vector3 b)
////        => (a - b).sqrMagnitude <= 1e-4f;
////}



//////using System.Collections.Generic;
//////using UnityEngine;

//////public class AIManager : MonoBehaviour
//////{
//////    [SerializeField] private LayerMask obstacleMask;   // 障害物のレイヤー
//////    [SerializeField] private float stepSize = 1.0f;    // 1ステップの長さ
//////    [SerializeField] private int maxNodes = 1000;      // セーフティ上限
//////    [SerializeField] private float reachThreshold = 1f;// ゴール到達距離
//////    [SerializeField] private float agentRadius = 0.25f;// エージェントの半径（衝突余裕）
//////    [SerializeField, Range(8, 180)] private int directionResolution = 72; // 探索方向数（全方向）

//////    // ====== Nodeクラス ======
//////    private class Node
//////    {
//////        public Vector3 pos;
//////        public Node parent;  // null 許可
//////        public float g;      // 開始からの実コスト
//////        public float f;      // f = g + h
//////    }

//////    // ===========================================================
//////    //  公開API：AIType に応じてA* / D*を切り替える
//////    // ===========================================================
//////    public Vector3[] GetPath(Vector3 startWorld, Vector3 goalWorld, AIType type)
//////    {
//////        switch (type)
//////        {
//////            case AIType.DStar:
//////                return GetPathDStar(startWorld, goalWorld);
//////            case AIType.AStar:
//////            default:
//////                return GetPathAStar(startWorld, goalWorld);
//////        }
//////    }

//////    // ===========================================================
//////    //  A* アルゴリズム本体（既存処理をそのまま移植）
//////    // ===========================================================
//////    private Vector3[] GetPathAStar(Vector3 startWorld, Vector3 goalWorld)
//////    {
//////        startWorld.y = goalWorld.y = 0f;

//////        if (Vector3.Distance(startWorld, goalWorld) <= reachThreshold)
//////            return new[] { goalWorld };

//////        var open = new List<Node>();
//////        var closed = new HashSet<Vector3>();

//////        var start = new Node
//////        {
//////            pos = startWorld,
//////            parent = null,
//////            g = 0f,
//////            f = Heuristic(startWorld, goalWorld)
//////        };
//////        open.Add(start);

//////        int iterations = 0;

//////        while (open.Count > 0 && iterations++ < maxNodes)
//////        {
//////            Node current = GetLowestF(open);
//////            open.Remove(current);
//////            closed.Add(current.pos);

//////            // ゴール到達判定
//////            if (Vector3.Distance(current.pos, goalWorld) <= reachThreshold)
//////                return ReconstructPath(current, goalWorld);

//////            // 近傍展開
//////            foreach (var next in Expand360(current, goalWorld))
//////            {
//////                if (closed.Contains(next.pos)) continue;

//////                // current → next に障害物があるかチェック
//////                Vector3 dir = (next.pos - current.pos);
//////                float dist = dir.magnitude;
//////                if (dist > 0f)
//////                {
//////                    dir /= dist;
//////                    if (Physics.SphereCast(current.pos, agentRadius, dir, out _, dist, obstacleMask))
//////                        continue;
//////                }

//////                Node same = open.Find(n => Approximately(n.pos, next.pos));
//////                if (same != null)
//////                {
//////                    if (next.g < same.g)
//////                    {
//////                        same.g = next.g;
//////                        same.f = next.f;
//////                        same.parent = current;
//////                    }
//////                }
//////                else
//////                {
//////                    open.Add(next);
//////                }
//////            }
//////        }

//////        Debug.LogWarning("[A*] 経路が見つかりませんでした。フォールバック（直行）します。");
//////        return new[] { goalWorld };
//////    }

//////    // ===========================================================
//////    //  D* アルゴリズム（簡易動的再探索版）
//////    // ===========================================================
//////    private Vector3[] GetPathDStar(Vector3 startWorld, Vector3 goalWorld)
//////    {
//////        // 初回はA*で探索
//////        var path = GetPathAStar(startWorld, goalWorld);
//////        if (path == null || path.Length == 0) return path;

//////        var verifiedPath = new List<Vector3>();

//////        for (int i = 0; i < path.Length - 1; i++)
//////        {
//////            Vector3 current = path[i];
//////            Vector3 next = path[i + 1];
//////            Vector3 dir = next - current;
//////            float dist = dir.magnitude;

//////            // 経路上に障害物が新たに出現していないか再チェック
//////            if (Physics.SphereCast(current, agentRadius, dir.normalized, out var hit, dist, obstacleMask))
//////            {
//////                Debug.Log("[D*] 動的障害物を検知 → 再探索を実行");
//////                var newPath = GetPathAStar(current, goalWorld); // 局所再探索
//////                if (newPath != null && newPath.Length > 0)
//////                {
//////                    verifiedPath.AddRange(newPath);
//////                    return verifiedPath.ToArray();
//////                }
//////            }

//////            verifiedPath.Add(current);
//////        }

//////        verifiedPath.Add(goalWorld);
//////        return verifiedPath.ToArray();
//////    }

//////    // ===========================================================
//////    //  共通ユーティリティ群
//////    // ===========================================================
//////    private float Heuristic(Vector3 a, Vector3 b) => Vector3.Distance(a, b);

//////    private Node GetLowestF(List<Node> list)
//////    {
//////        Node best = list[0];
//////        float bestF = best.f;
//////        for (int i = 1; i < list.Count; i++)
//////        {
//////            if (list[i].f < bestF)
//////            {
//////                best = list[i];
//////                bestF = list[i].f;
//////            }
//////        }
//////        return best;
//////    }

//////    // 360°方向に分割して探索
//////    private IEnumerable<Node> Expand360(Node current, Vector3 goal)
//////    {
//////        float angleStep = 360f / directionResolution;
//////        for (int i = 0; i < directionResolution; i++)
//////        {
//////            float angle = i * angleStep;
//////            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
//////            Vector3 np = current.pos + dir * stepSize;

//////            float g = current.g + stepSize;
//////            float f = g + Heuristic(np, goal);

//////            yield return new Node
//////            {
//////                pos = np,
//////                parent = current,
//////                g = g,
//////                f = f
//////            };
//////        }
//////    }

//////    private Vector3[] ReconstructPath(Node goalNode, Vector3 goalWorld)
//////    {
//////        var path = new List<Vector3> { goalWorld };
//////        Node n = goalNode;
//////        while (n != null)
//////        {
//////            path.Add(n.pos);
//////            n = n.parent;
//////        }
//////        path.Reverse();
//////        return path.ToArray();
//////    }

//////    private bool Approximately(Vector3 a, Vector3 b)
//////        => (a - b).sqrMagnitude <= 1e-4f;
//////}


////////using System.Collections.Generic;
////////using UnityEngine;

////////public class AIManager : MonoBehaviour
////////{
////////    [SerializeField] private LayerMask obstacleMask;   // 障害物のレイヤー
////////    [SerializeField] private float stepSize = 1.0f;    // 1ステップの長さ
////////    [SerializeField] private int maxNodes = 1000;      // セーフティ上限
////////    [SerializeField] private float reachThreshold = 1f;// ゴール到達距離
////////    [SerializeField] private float agentRadius = 0.25f;// エージェントの半径（衝突余裕）
////////    [SerializeField, Range(8, 180)] private int directionResolution = 72; // 探索方向数（全方向）

////////    private class Node
////////    {
////////        public Vector3 pos;
////////        public Node parent;  // null 許可
////////        public float g;      // 開始からの実コスト
////////        public float f;      // f = g + h
////////    }

////////    public Vector3[] GetPath(Vector3 startWorld, Vector3 goalWorld)
////////    {
////////        startWorld.y = goalWorld.y = 0f;

////////        if (Vector3.Distance(startWorld, goalWorld) <= reachThreshold)
////////            return new[] { goalWorld };

////////        var open = new List<Node>();
////////        var closed = new HashSet<Vector3>();

////////        var start = new Node
////////        {
////////            pos = startWorld,
////////            parent = null,
////////            g = 0f,
////////            f = Heuristic(startWorld, goalWorld)
////////        };
////////        open.Add(start);

////////        int iterations = 0;

////////        while (open.Count > 0 && iterations++ < maxNodes)
////////        {
////////            Node current = GetLowestF(open);
////////            open.Remove(current);
////////            closed.Add(current.pos);

////////            // ゴール到達判定
////////            if (Vector3.Distance(current.pos, goalWorld) <= reachThreshold)
////////                return ReconstructPath(current, goalWorld);

////////            // 近傍展開
////////            foreach (var next in Expand360(current, goalWorld))
////////            {
////////                if (closed.Contains(next.pos)) continue;

////////                // current → next に障害物があるかチェック
////////                Vector3 dir = (next.pos - current.pos);
////////                float dist = dir.magnitude;
////////                if (dist > 0f)
////////                {
////////                    dir /= dist;
////////                    if (Physics.SphereCast(current.pos, agentRadius, dir, out _, dist, obstacleMask))
////////                        continue;
////////                }

////////                Node same = open.Find(n => Approximately(n.pos, next.pos));
////////                if (same != null)
////////                {
////////                    if (next.g < same.g)
////////                    {
////////                        same.g = next.g;
////////                        same.f = next.f;
////////                        same.parent = current;
////////                    }
////////                }
////////                else
////////                {
////////                    open.Add(next);
////////                }
////////            }
////////        }

////////        Debug.LogWarning("[A*] 経路が見つかりませんでした。フォールバック（直行）します。");
////////        return new[] { goalWorld };
////////    }

////////    private float Heuristic(Vector3 a, Vector3 b) => Vector3.Distance(a, b);

////////    private Node GetLowestF(List<Node> list)
////////    {
////////        Node best = list[0];
////////        float bestF = best.f;
////////        for (int i = 1; i < list.Count; i++)
////////        {
////////            if (list[i].f < bestF)
////////            {
////////                best = list[i];
////////                bestF = list[i].f;
////////            }
////////        }
////////        return best;
////////    }

////////    // 360°方向に分割して探索
////////    private IEnumerable<Node> Expand360(Node current, Vector3 goal)
////////    {
////////        float angleStep = 360f / directionResolution;
////////        for (int i = 0; i < directionResolution; i++)
////////        {
////////            float angle = i * angleStep;
////////            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
////////            Vector3 np = current.pos + dir * stepSize;

////////            float g = current.g + stepSize;
////////            float f = g + Heuristic(np, goal);

////////            yield return new Node
////////            {
////////                pos = np,
////////                parent = current,
////////                g = g,
////////                f = f
////////            };
////////        }
////////    }

////////    private Vector3[] ReconstructPath(Node goalNode, Vector3 goalWorld)
////////    {
////////        var path = new List<Vector3> { goalWorld };
////////        Node n = goalNode;
////////        while (n != null)
////////        {
////////            path.Add(n.pos);
////////            n = n.parent;
////////        }
////////        path.Reverse();
////////        return path.ToArray();
////////    }

////////    private bool Approximately(Vector3 a, Vector3 b)
////////        => (a - b).sqrMagnitude <= 1e-4f;
////////}