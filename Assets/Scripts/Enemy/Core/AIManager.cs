using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    [System.Serializable]
    public class Obstacle
    {
        public Vector3 pos;
        public float radius;
    }

    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float reachThreshold = 0.5f;
    [SerializeField] private bool showGizmos = true;

    private List<Obstacle> obstacles = new List<Obstacle>();
    private List<Node> dstarNodes = new List<Node>();
    private Vector3[] lastAStarPath;
    private Vector3[] lastDStarPath;

    private bool initialObstacleRegistered = false;

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
    // 経路探索ルート選択
    //============================================================
    public Vector3[] GetPath(Vector3 startWorld, Vector3 goalWorld, AIType type)
    {
        switch (type)
        {
            case AIType.AStar:
                return GetPathAStar(startWorld, goalWorld);
            case AIType.DStar:
                return GetPathDStar(startWorld, goalWorld);
            case AIType.NavMesh:
                return GetPathNavMesh(startWorld, goalWorld);
            default:
                return new Vector3[0];
        }
    }

    //============================================================
    // A* アルゴリズム（静的）
    //============================================================
    private Vector3[] GetPathAStar(Vector3 startWorld, Vector3 goalWorld)
    {
        Debug.Log("[A*] 新規経路探索を実行");

        var open = new List<Node>();
        var closed = new HashSet<Vector3>();
        var nodes = new Dictionary<Vector3, Node>();

        Node start = new Node { pos = startWorld, g = 0, f = Heuristic(startWorld, goalWorld) };
        nodes[startWorld] = start;
        open.Add(start);

        while (open.Count > 0)
        {
            Node current = open.OrderBy(n => n.f).First();
            open.Remove(current);
            closed.Add(current.pos);

            if (Vector3.Distance(current.pos, goalWorld) <= reachThreshold)
            {
                lastAStarPath = ReconstructPath(current, goalWorld);
                return lastAStarPath;
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
                    if (!open.Contains(next))
                        open.Add(next);
                }
            }
        }

        Debug.LogWarning("[A*] 経路が見つかりませんでした。");
        return new Vector3[0];
    }

    //============================================================
    // D* アルゴリズム（動的再探索）
    //============================================================
    private Vector3[] GetPathDStar(Vector3 startWorld, Vector3 goalWorld)
    {
        Debug.Log("[D*] 新規経路探索を実行");

        var open = new List<Node>();
        var closed = new HashSet<Vector3>();
        var nodes = new Dictionary<Vector3, Node>();

        Node start = new Node { pos = startWorld, g = 0, f = Heuristic(startWorld, goalWorld) };
        nodes[startWorld] = start;
        open.Add(start);

        while (open.Count > 0)
        {
            Node current = open.OrderBy(n => n.f).First();
            open.Remove(current);
            closed.Add(current.pos);

            if (Vector3.Distance(current.pos, goalWorld) <= reachThreshold)
            {
                lastDStarPath = ReconstructPath(current, goalWorld);
                dstarNodes = ConvertPathToNodes(lastDStarPath);
                Debug.Log("[D*] 初回探索完了（独立D*）");
                return lastDStarPath;
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
                    if (!open.Contains(next))
                        open.Add(next);
                }
            }
        }

        Debug.LogWarning("[D*] 経路が見つかりませんでした。");
        return new Vector3[0];
    }

    //============================================================
    // NavMesh 経路（参考）
    //============================================================
    private Vector3[] GetPathNavMesh(Vector3 startWorld, Vector3 goalWorld)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(startWorld, goalWorld, NavMesh.AllAreas, path))
            return path.corners;
        return new Vector3[0];
    }

    //============================================================
    // 動的障害物の登録／削除
    //============================================================
    //public void OnDynamicObstacleChanged(Vector3 position, float radius, bool isBlocked)
    //{
    //    position.y = 0f;

    //    if (isBlocked)
    //        obstacles.Add(new Obstacle { pos = position, radius = radius });
    //    else
    //        obstacles.RemoveAll(o => Vector3.Distance(o.pos, position) < 0.01f);

    //    if (!initialObstacleRegistered)
    //        initialObstacleRegistered = true;

    //    // D* エージェント → 即再探索
    //    // A* エージェント → 初回登録完了時のみ経路補正
    //    foreach (var agent in FindObjectsOfType<AgentController>())
    //    {
    //        var data = agent.GetData();
    //        if (data == null) continue;

    //        if (data.aiType == AIType.DStar)
    //        {
    //            var path = GetPathDStar(agent.transform.position, agent.GetGoalPosition());
    //            agent.SetPath(path);
    //        }
    //        else if (data.aiType == AIType.AStar && !agent.HasPath())
    //        {
    //            var path = GetPathAStar(agent.transform.position, agent.GetGoalPosition());
    //            agent.SetPath(path);
    //            Debug.Log($"[AIManager] A* 初期補正経路を適用 at {position}");
    //        }
    //    }
    //}
    public void OnDynamicObstacleChanged(Vector3 position, float radius, bool isBlocked)
    {
        position.y = 0f;

        if (isBlocked)
            obstacles.Add(new Obstacle { pos = position, radius = radius });
        else
            obstacles.RemoveAll(o => Vector3.Distance(o.pos, position) < 0.01f);

        if (!initialObstacleRegistered)
            initialObstacleRegistered = true;

        Debug.Log($"[AIManager] 動的障害物更新: pos={position}, radius={radius:F2}, blocked={isBlocked}");

        // --- D*エージェントは即時再探索 ---
        // --- A*エージェントは初回登録完了時のみ経路補正 ---
        foreach (var agent in FindObjectsOfType<AgentController>())
        {
            var data = agent.GetData();
            if (data == null) continue;

            // -------------------------
            // D* 再探索処理
            // -------------------------
            if (data.aiType == AIType.DStar)
            {
                Vector3 start = agent.transform.position;
                Vector3 goal = agent.GetGoalPosition();

                Debug.Log($"[D*] {agent.name} 再探索開始: start={start}, goal={goal}");

                var newPath = GetPathDStar(start, goal);

                // 無効な経路を上書きしないガード
                if (newPath == null || newPath.Length <= 1)
                {
                    Debug.LogWarning($"[D*] {agent.name} の再探索結果が無効 (len={newPath?.Length ?? 0}) → 経路上書きせず保持");
                    continue;
                }

                Debug.Log($"[D*] {agent.name} に新経路を適用: {newPath.Length} ノード");
                agent.SetPath(newPath);
            }

            // -------------------------
            // A* 初期補正処理（初回のみ）
            // -------------------------
            else if (data.aiType == AIType.AStar && !agent.HasPath())
            {
                Vector3 start = agent.transform.position;
                Vector3 goal = agent.GetGoalPosition();

                var newPath = GetPathAStar(start, goal);

                if (newPath != null && newPath.Length > 1)
                {
                    agent.SetPath(newPath);
                    Debug.Log($"[AIManager] A* 初期補正経路を適用: {agent.name} ({newPath.Length} ノード) at {position}");
                }
                else
                {
                    Debug.LogWarning($"[AIManager] {agent.name} のA*補正経路が無効 (len={newPath?.Length ?? 0}) → 上書きせず");
                }
            }
        }
    }


    public bool IsObstacleDataReady() => initialObstacleRegistered;

    //============================================================
    // 共通補助関数群
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
        float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    private IEnumerable<Vector3> ExpandNeighbors(Vector3 pos)
    {
        for (int dx = -1; dx <= 1; dx++)
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;
                yield return pos + new Vector3(dx * gridSize, 0, dz * gridSize);
            }
    }

    private float Heuristic(Vector3 a, Vector3 b) => Vector3.Distance(a, b);

    private Vector3[] ReconstructPath(Node endNode, Vector3 goal)
    {
        var path = new List<Vector3>();
        var current = endNode;
        while (current != null)
        {
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

    //============================================================
    // Gizmo描画
    //============================================================
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.yellow;
        if (lastAStarPath != null && lastAStarPath.Length > 1)
            for (int i = 0; i < lastAStarPath.Length - 1; i++)
                Gizmos.DrawLine(lastAStarPath[i], lastAStarPath[i + 1]);

        Gizmos.color = Color.cyan;
        if (lastDStarPath != null && lastDStarPath.Length > 1)
            for (int i = 0; i < lastDStarPath.Length - 1; i++)
                Gizmos.DrawLine(lastDStarPath[i], lastDStarPath[i + 1]);
    }

    // AgentController から参照される公開メソッド
    public bool HasObstacleBetween(Vector3 a, Vector3 b) => IsObstacleBetween(a, b);
}
