using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Unity.Profiling;
using Stopwatch = System.Diagnostics.Stopwatch;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    [System.Serializable]
    public class Obstacle
    {
        public Vector3 pos;
        public float radius;
    }

    [Header("探索設定")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float reachThreshold = 0.5f;

    [Header("360°サンプリング設定")]
    [SerializeField, Range(8, 72)] private int directionSamples = 100;
    [SerializeField] private float stepLength = 1f;

    [Header("デバッグ表示")]
    [SerializeField] private bool showGizmos = true;

    private List<Obstacle> obstacles = new List<Obstacle>();
    private List<Node> dstarNodes = new List<Node>();
    private Vector3[] lastAStarPath;
    private Vector3[] lastDStarPath;
    private bool initialObstacleRegistered = false;

    // Profiler マーカー
    private static readonly ProfilerMarker markerAStar = new ProfilerMarker("AStar_Pathfind");
    private static readonly ProfilerMarker markerDStar = new ProfilerMarker("DStar_Pathfind");
    private static readonly ProfilerMarker markerNavMesh = new ProfilerMarker("NavMesh_Pathfind");

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //============================================================
    // ノード定義
    //============================================================
    private class Node
    {
        public Vector3 pos;
        public float g;
        public float f;
        public Node parent;
    }

    //============================================================
    // 経路探索の選択
    //============================================================
    public Vector3[] GetPath(Vector3 startWorld, Vector3 goalWorld, AIType type)
    {
        switch (type)
        {
            case AIType.AStar: return GetPathAStar(startWorld, goalWorld);
            case AIType.DStar: return GetPathDStar(startWorld, goalWorld);
            case AIType.NavMesh: return GetPathNavMesh(startWorld, goalWorld);
            default: return new Vector3[0];
        }
    }

    //============================================================
    // A*
    //============================================================
    private Vector3[] GetPathAStar(Vector3 startWorld, Vector3 goalWorld)
    {
        Stopwatch sw = Stopwatch.StartNew();
        using (markerAStar.Auto())
        {
            Debug.Log("[A*] 新規経路探索を実行");
            var open = new List<Node>();
            var closed = new HashSet<Vector3>();
            var nodes = new Dictionary<Vector3, Node>();

            Node start = new Node { pos = startWorld, g = 0, f = Heuristic(startWorld, goalWorld) };
            nodes[startWorld] = start;
            open.Add(start);

            int loopCount = 0;
            const int MAX_LOOP = 5000;

            while (open.Count > 0)
            {
                if (++loopCount > MAX_LOOP)
                {
                    Debug.LogError("[A*] Infinite loop detected!");
                    break;
                }

                Node current = open.OrderBy(n => n.f).First();
                open.Remove(current);
                closed.Add(current.pos);

                if (Vector3.Distance(current.pos, goalWorld) <= reachThreshold)
                {
                    lastAStarPath = ReconstructPath(current, goalWorld);
                    break;
                }

                foreach (var nextPos in ExpandNeighbors(current.pos))
                {
                    if (closed.Contains(nextPos)) continue;
                    if (IsObstacleBetween(current.pos, nextPos)) continue;

                    float tentativeG = current.g + Vector3.Distance(current.pos, nextPos);
                    if (!nodes.TryGetValue(nextPos, out Node next))
                    {
                        next = new Node { pos = nextPos, g = Mathf.Infinity };
                        nodes[nextPos] = next;
                    }

                    if (tentativeG < next.g)
                    {
                        next.parent = current;
                        next.g = tentativeG;
                        next.f = tentativeG + Heuristic(next.pos, goalWorld);
                        if (!open.Contains(next)) open.Add(next);
                    }
                }
            }
        }
        sw.Stop();

        // CPU時間をCSV出力
        CSVLogger.Log(
            type: "Performance",
            name: "AIManager",
            state: "Pathfinding",
            action: "Executed",
            target: "AStar",
            targetPos: Vector3.zero,
            currentPos: Vector3.zero,
            nextPos: Vector3.zero,
            aiType: "AStar",
            cpuMs: (float)sw.Elapsed.TotalMilliseconds
        );

        return lastAStarPath;
    }

    //============================================================
    // D*
    //============================================================
    private Vector3[] GetPathDStar(Vector3 startWorld, Vector3 goalWorld)
    {
        Stopwatch sw = Stopwatch.StartNew();
        using (markerDStar.Auto())
        {
            Debug.Log("[D*] 新規経路探索を実行");

            var open = new List<Node>();
            var closed = new HashSet<Vector3>();
            var nodes = new Dictionary<Vector3, Node>();

            Node start = new Node { pos = startWorld, g = 0, f = Heuristic(startWorld, goalWorld) };
            nodes[startWorld] = start;
            open.Add(start);

            int loopCount = 0;
            const int MAX_LOOP = 5000;

            while (open.Count > 0)
            {
                if (++loopCount > MAX_LOOP)
                {
                    Debug.LogError("[D*] Infinite loop detected!");
                    break;
                }

                Node current = open.OrderBy(n => n.f).First();
                open.Remove(current);
                closed.Add(current.pos);

                if (Vector3.Distance(current.pos, goalWorld) <= reachThreshold)
                {
                    lastDStarPath = ReconstructPath(current, goalWorld);
                    dstarNodes = ConvertPathToNodes(lastDStarPath);
                    Debug.Log("[D*] 探索完了");
                    break;
                }

                foreach (var nextPos in ExpandNeighbors(current.pos))
                {
                    if (closed.Contains(nextPos)) continue;
                    if (IsObstacleBetween(current.pos, nextPos)) continue;

                    float cost = Vector3.Distance(current.pos, nextPos);
                    float tentativeG = current.g + cost;

                    if (!nodes.TryGetValue(nextPos, out Node next))
                    {
                        next = new Node { pos = nextPos, g = Mathf.Infinity };
                        nodes[nextPos] = next;
                    }

                    if (tentativeG < next.g)
                    {
                        next.parent = current;
                        next.g = tentativeG;
                        next.f = tentativeG + Heuristic(next.pos, goalWorld);
                        if (!open.Contains(next)) open.Add(next);
                    }
                }
            }
        }
        sw.Stop();

        CSVLogger.Log(
            type: "Performance",
            name: "AIManager",
            state: "Pathfinding",
            action: "Executed",
            target: "DStar",
            targetPos: Vector3.zero,
            currentPos: Vector3.zero,
            nextPos: Vector3.zero,
            aiType: "DStar",
            cpuMs: (float)sw.Elapsed.TotalMilliseconds
        );

        return lastDStarPath;
    }

    //============================================================
    // NavMesh
    //============================================================
    private Vector3[] GetPathNavMesh(Vector3 startWorld, Vector3 goalWorld)
    {
        Stopwatch sw = Stopwatch.StartNew();
        Vector3[] result;
        using (markerNavMesh.Auto())
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(startWorld, goalWorld, NavMesh.AllAreas, path))
                result = path.corners;
            else
                result = new Vector3[0];
        }
        sw.Stop();

        CSVLogger.Log(
            type: "Performance",
            name: "AIManager",
            state: "Pathfinding",
            action: "Executed",
            target: "NavMesh",
            targetPos: Vector3.zero,
            currentPos: Vector3.zero,
            nextPos: Vector3.zero,
            aiType: "NavMesh",
            cpuMs: (float)sw.Elapsed.TotalMilliseconds
        );

        return result;
    }

    //============================================================
    // 動的障害物通知
    //============================================================
    public void OnDynamicObstacleChanged(Vector3 position, float radius, bool isBlocked)
    {
        position.y = 0f;

        if (isBlocked)
        {
            if (obstacles.Any(o => Vector3.Distance(o.pos, position) < 0.1f))
                return;
            obstacles.Add(new Obstacle { pos = position, radius = radius });
        }
        else
        {
            obstacles.RemoveAll(o => Vector3.Distance(o.pos, position) < 0.1f);
        }

        Debug.Log($"[AIManager] 動的障害物更新: pos={position}, radius={radius:F2}, blocked={isBlocked}");

        foreach (var agent in FindObjectsOfType<AgentController>())
        {
            var data = agent.GetData();
            if (data == null || data.aiType != AIType.DStar) continue;
            if (Vector3.Distance(agent.transform.position, position) > radius * 5f)
                continue;

            Vector3 start = agent.transform.position;
            Vector3 goal = agent.GetGoalPosition();
            var newPath = GetPathDStar(start, goal);
            if (newPath == null || newPath.Length <= 1) continue;

            agent.SetPath(newPath);
        }
    }

    //============================================================
    // 共通関数
    //============================================================
    private bool IsObstacleBetween(Vector3 a, Vector3 b)
    {
        foreach (var o in obstacles)
        {
            Vector3 closest = ClosestPointOnSegment(o.pos, a, b);
            float distance = Vector3.Distance(o.pos, closest);
            if (distance <= o.radius)
                return true;
        }
        return false;
    }

    private Vector3 ClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / ab.sqrMagnitude);
        return a + ab * t;
    }

    private IEnumerable<Vector3> ExpandNeighbors(Vector3 pos)
    {
        float angleStep = 360f / directionSamples;
        for (int i = 0; i < directionSamples; i++)
        {
            float rad = angleStep * i * Mathf.Deg2Rad;
            float nx = pos.x + Mathf.Cos(rad) * stepLength;
            float nz = pos.z + Mathf.Sin(rad) * stepLength;

            nx = Mathf.Round(nx / gridSize) * gridSize;
            nz = Mathf.Round(nz / gridSize) * gridSize;

            Vector3 neighbor = new Vector3(nx, pos.y, nz);
            if (IsObstacleBetween(pos, neighbor)) continue;
            yield return neighbor;
        }
    }

    private float Heuristic(Vector3 a, Vector3 b) => Vector3.Distance(a, b);

    private Vector3[] ReconstructPath(Node endNode, Vector3 goal)
    {
        var path = new List<Vector3>();
        var current = endNode;
        HashSet<Node> visited = new HashSet<Node>();

        while (current != null)
        {
            if (!visited.Add(current)) break;
            path.Insert(0, current.pos);
            current = current.parent;
        }

        path.Add(goal);
        return path.ToArray();
    }

    private List<Node> ConvertPathToNodes(Vector3[] path)
    {
        return path.Select(p => new Node { pos = p }).ToList();
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.cyan;
        if (lastDStarPath != null && lastDStarPath.Length > 1)
            for (int i = 0; i < lastDStarPath.Length - 1; i++)
                Gizmos.DrawLine(lastDStarPath[i], lastDStarPath[i + 1]);
    }

    public bool HasObstacleBetween(Vector3 a, Vector3 b) => IsObstacleBetween(a, b);
}


//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.AI;
//using Unity.Profiling;

//public class AIManager : MonoBehaviour
//{
//    public static AIManager Instance { get; private set; }

//    // --- Profilerマーカーを追加 ---
//    private static readonly ProfilerMarker markerAStar = new ProfilerMarker("AStar_Pathfind");
//    private static readonly ProfilerMarker markerDStar = new ProfilerMarker("DStar_Pathfind");
//    private static readonly ProfilerMarker markerNavMesh = new ProfilerMarker("NavMesh_Pathfind");

//    [System.Serializable]
//    public class Obstacle
//    {
//        public Vector3 pos;
//        public float radius;
//    }

//    [Header("探索設定")]
//    [SerializeField] private float gridSize = 1f;
//    [SerializeField] private float reachThreshold = 0.5f;

//    [Header("360°サンプリング設定")]
//    [SerializeField, Range(8, 72)] private int directionSamples = 100; // 8,16,32など自由
//    [SerializeField] private float stepLength = 1f;                    // 1ステップの長さ（＝gridSizeでOK）
//    //[SerializeField, Range(1, 10)] private int subSteps = 2;          // 細分化数（例:10で0.1刻み）

//    [Header("デバッグ表示")]
//    [SerializeField] private bool showGizmos = true;

//    private List<Obstacle> obstacles = new List<Obstacle>();
//    private List<Node> dstarNodes = new List<Node>();
//    private Vector3[] lastAStarPath;
//    private Vector3[] lastDStarPath;
//    private bool initialObstacleRegistered = false;

//    private void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);
//    }

//    //============================================================
//    // ノード定義
//    //============================================================
//    private class Node
//    {
//        public Vector3 pos;
//        public float g;
//        public float f;
//        public Node parent;
//    }

//    //============================================================
//    // 経路探索の選択
//    //============================================================
//    public Vector3[] GetPath(Vector3 startWorld, Vector3 goalWorld, AIType type)
//    {
//        switch (type)
//        {
//            case AIType.AStar: return GetPathAStar(startWorld, goalWorld);
//            case AIType.DStar: return GetPathDStar(startWorld, goalWorld);
//            case AIType.NavMesh: return GetPathNavMesh(startWorld, goalWorld);
//            default: return new Vector3[0];
//        }
//    }

//    //============================================================
//    // A*
//    //============================================================
//    private Vector3[] GetPathAStar(Vector3 startWorld, Vector3 goalWorld)
//    {
//        var sw = System.Diagnostics.Stopwatch.StartNew();

//        using (markerAStar.Auto()) // ここでCPU計測開始
//        {
//            Debug.Log("[A*] 新規経路探索を実行");
//            var open = new List<Node>();
//            var closed = new HashSet<Vector3>();
//            var nodes = new Dictionary<Vector3, Node>();

//            Node start = new Node { pos = startWorld, g = 0, f = Heuristic(startWorld, goalWorld) };
//            nodes[startWorld] = start;
//            open.Add(start);

//            int loopCount = 0;
//            const int MAX_LOOP = 5000;

//            while (open.Count > 0)
//            {
//                if (++loopCount > MAX_LOOP)
//                {
//                    Debug.LogError("[A*] Infinite loop detected!");
//                    break;
//                }

//                Node current = open.OrderBy(n => n.f).First();
//                open.Remove(current);
//                closed.Add(current.pos);

//                if (Vector3.Distance(current.pos, goalWorld) <= reachThreshold)
//                {
//                    lastAStarPath = ReconstructPath(current, goalWorld);
//                    return lastAStarPath;
//                }

//                foreach (var nextPos in ExpandNeighbors(current.pos))
//                {
//                    if (closed.Contains(nextPos)) continue;
//                    if (IsObstacleBetween(current.pos, nextPos)) continue;

//                    float tentativeG = current.g + Vector3.Distance(current.pos, nextPos);
//                    if (!nodes.TryGetValue(nextPos, out Node next))
//                    {
//                        next = new Node { pos = nextPos, g = Mathf.Infinity };
//                        nodes[nextPos] = next;
//                    }

//                    if (tentativeG < next.g)
//                    {
//                        next.parent = current;
//                        next.g = tentativeG;
//                        next.f = tentativeG + Heuristic(next.pos, goalWorld);
//                        if (!open.Contains(next)) open.Add(next);
//                    }
//                }
//            }

//            return new Vector3[0];
//        }// ここでCPU計測終了

//        sw.Stop();
//        CSVLogger.LogPathPerformance("AStar", (float)sw.Elapsed.TotalMilliseconds);
//        return lastAStarPath;
//    }

//    //============================================================
//    // D*
//    //============================================================
//    private Vector3[] GetPathDStar(Vector3 startWorld, Vector3 goalWorld)
//    {
//        var sw = System.Diagnostics.Stopwatch.StartNew();

//        using (markerAStar.Auto()) // ここでCPU計測開始
//        {
//            Debug.Log("[D*] 新規経路探索を実行");

//            var open = new List<Node>();
//            var closed = new HashSet<Vector3>();
//            var nodes = new Dictionary<Vector3, Node>();

//            Node start = new Node { pos = startWorld, g = 0, f = Heuristic(startWorld, goalWorld) };
//            nodes[startWorld] = start;
//            open.Add(start);

//            int loopCount = 0;
//            const int MAX_LOOP = 5000;

//            while (open.Count > 0)
//            {
//                if (++loopCount > MAX_LOOP)
//                {
//                    Debug.LogError("[D*] Infinite loop detected!");
//                    break;
//                }

//                Node current = open.OrderBy(n => n.f).First();
//                open.Remove(current);
//                closed.Add(current.pos);

//                if (Vector3.Distance(current.pos, goalWorld) <= reachThreshold)
//                {
//                    lastDStarPath = ReconstructPath(current, goalWorld);
//                    dstarNodes = ConvertPathToNodes(lastDStarPath);
//                    Debug.Log("[D*] 探索完了");
//                    return lastDStarPath;
//                }

//                foreach (var nextPos in ExpandNeighbors(current.pos))
//                {
//                    if (closed.Contains(nextPos)) continue;
//                    if (IsObstacleBetween(current.pos, nextPos)) continue;

//                    float cost = Vector3.Distance(current.pos, nextPos);
//                    float tentativeG = current.g + cost;

//                    if (!nodes.TryGetValue(nextPos, out Node next))
//                    {
//                        next = new Node { pos = nextPos, g = Mathf.Infinity };
//                        nodes[nextPos] = next;
//                    }

//                    if (tentativeG < next.g)
//                    {
//                        next.parent = current;
//                        next.g = tentativeG;
//                        next.f = tentativeG + Heuristic(next.pos, goalWorld);
//                        if (!open.Contains(next)) open.Add(next);
//                    }
//                }
//            }

//            return new Vector3[0];
//        }// ここでCPU計測終了

//        sw.Stop();
//        CSVLogger.LogPathPerformance("DStar", (float)sw.Elapsed.TotalMilliseconds);
//        return lastDStarPath;
//    }

//    //============================================================
//    // NavMesh
//    //============================================================
//    private Vector3[] GetPathNavMesh(Vector3 startWorld, Vector3 goalWorld)
//    {
//        var sw = System.Diagnostics.Stopwatch.StartNew();

//        using (markerNavMesh.Auto()) // ここでCPU計測開始
//        {
//            NavMeshPath path = new NavMeshPath();
//            if (NavMesh.CalculatePath(startWorld, goalWorld, NavMesh.AllAreas, path))
//                return path.corners;
//            return new Vector3[0];
//        }// ここでCPU計測終了

//        sw.Stop();
//        CSVLogger.LogPathPerformance("NavMesh", (float)sw.Elapsed.TotalMilliseconds);
//    }

//    //============================================================
//    // 動的障害物通知（修正版）
//    //============================================================
//    public void OnDynamicObstacleChanged(Vector3 position, float radius, bool isBlocked)
//    {
//        position.y = 0f;

//        // 重複登録防止
//        if (isBlocked)
//        {
//            if (obstacles.Any(o => Vector3.Distance(o.pos, position) < 0.1f))
//                return;
//            obstacles.Add(new Obstacle { pos = position, radius = radius });
//        }
//        else
//        {
//            obstacles.RemoveAll(o => Vector3.Distance(o.pos, position) < 0.1f);
//        }

//        Debug.Log($"[AIManager] 動的障害物更新: pos={position}, radius={radius:F2}, blocked={isBlocked}");

//        foreach (var agent in FindObjectsOfType<AgentController>())
//        {
//            var data = agent.GetData();
//            if (data == null || data.aiType != AIType.DStar) continue;

//            // ★範囲拡大で暴走防止
//            if (Vector3.Distance(agent.transform.position, position) > radius * 5f)
//                continue;

//            Vector3 start = agent.transform.position;
//            Vector3 goal = agent.GetGoalPosition();

//            var newPath = GetPathDStar(start, goal);
//            if (newPath == null || newPath.Length <= 1) continue;

//            agent.SetPath(newPath);
//        }
//    }

//    //============================================================
//    // 共通関数
//    //============================================================
//    private bool IsObstacleBetween(Vector3 a, Vector3 b)
//    {
//        foreach (var o in obstacles)
//        {
//            Vector3 closest = ClosestPointOnSegment(o.pos, a, b);
//            float distance = Vector3.Distance(o.pos, closest);
//            if (distance <= o.radius)
//                return true;
//        }
//        return false;
//    }

//    private Vector3 ClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
//    {
//        Vector3 ab = b - a;
//        float t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / ab.sqrMagnitude);
//        return a + ab * t;
//    }

//    //private IEnumerable<Vector3> ExpandNeighbors(Vector3 pos)
//    //{
//    //    for (int dx = -1; dx <= 1; dx++)
//    //        for (int dz = -1; dz <= 1; dz++)
//    //        {
//    //            if (dx == 0 && dz == 0) continue;
//    //            yield return pos + new Vector3(dx * gridSize, 0, dz * gridSize);
//    //        }
//    //}
//    private IEnumerable<Vector3> ExpandNeighbors(Vector3 pos)
//    {
//        // 360°を directionSamples で分割
//        float angleStep = 360f / directionSamples;

//        for (int i = 0; i < directionSamples; i++)
//        {
//            float rad = angleStep * i * Mathf.Deg2Rad;
//            float nx = pos.x + Mathf.Cos(rad) * stepLength;
//            float nz = pos.z + Mathf.Sin(rad) * stepLength;

//            // 無限ループ防止：整数グリッド単位にスナップ
//            nx = Mathf.Round(nx / gridSize) * gridSize;
//            nz = Mathf.Round(nz / gridSize) * gridSize;

//            Vector3 neighbor = new Vector3(nx, pos.y, nz);

//            if (IsObstacleBetween(pos, neighbor))
//                continue;

//            yield return neighbor;
//        }
//    }
//    //private IEnumerable<Vector3> ExpandNeighbors(Vector3 pos)
//    //{
//    //    float angleStep = 360f / directionSamples;
//    //    float subStepLength = stepLength / subSteps; // 例: 1.0/10 = 0.1

//    //    for (int i = 0; i < directionSamples; i++)
//    //    {
//    //        float rad = angleStep * i * Mathf.Deg2Rad;

//    //        // 細かくステップを刻む
//    //        for (int s = 1; s <= subSteps; s++)
//    //        {
//    //            float nx = pos.x + Mathf.Cos(rad) * (subStepLength * s);
//    //            float nz = pos.z + Mathf.Sin(rad) * (subStepLength * s);

//    //            // グリッド丸めしてループ防止（0.01単位など）
//    //            nx = Mathf.Round(nx * 100f) / 100f;
//    //            nz = Mathf.Round(nz * 100f) / 100f;

//    //            Vector3 neighbor = new Vector3(nx, pos.y, nz);

//    //            // 障害物チェック
//    //            if (!IsObstacleBetween(pos, neighbor))
//    //                yield return neighbor;
//    //        }
//    //    }
//    //}

//    private float Heuristic(Vector3 a, Vector3 b) => Vector3.Distance(a, b);

//    private Vector3[] ReconstructPath(Node endNode, Vector3 goal)
//    {
//        var path = new List<Vector3>();
//        var current = endNode;
//        HashSet<Node> visited = new HashSet<Node>();

//        while (current != null)
//        {
//            if (!visited.Add(current)) break;
//            path.Insert(0, current.pos);
//            current = current.parent;
//        }

//        path.Add(goal);
//        return path.ToArray();
//    }

//    private List<Node> ConvertPathToNodes(Vector3[] path)
//    {
//        return path.Select(p => new Node { pos = p }).ToList();
//    }

//    private void OnDrawGizmos()
//    {
//        if (!showGizmos) return;

//        Gizmos.color = Color.cyan;
//        if (lastDStarPath != null && lastDStarPath.Length > 1)
//            for (int i = 0; i < lastDStarPath.Length - 1; i++)
//                Gizmos.DrawLine(lastDStarPath[i], lastDStarPath[i + 1]);
//    }

//    public bool HasObstacleBetween(Vector3 a, Vector3 b) => IsObstacleBetween(a, b);
//}