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