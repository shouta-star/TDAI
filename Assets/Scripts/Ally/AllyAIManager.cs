using System.Collections.Generic;
using UnityEngine;

public class AllyAIManager : MonoBehaviour
{
    [SerializeField] private LayerMask obstacleMask;   // 障害物レイヤー
    [SerializeField] private float stepSize = 1.0f;    // 1ステップの長さ
    [SerializeField] private int maxNodes = 1000;      // 安全上限
    [SerializeField] private float reachThreshold = 1f;// ゴール到達距離
    [SerializeField] private float allyRadius = 0.25f; // 味方の当たり判定半径
    [SerializeField, Range(8, 180)] private int directionResolution = 72; // 探索方向数

    private class Node
    {
        public Vector3 pos;
        public Node parent;
        public float g; // 開始からのコスト
        public float f; // g + h
    }

    /// <summary>
    /// 味方のA*経路探索（敵とは独立管理）
    /// </summary>
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

            // --- ゴール到達 ---
            if (Vector3.Distance(current.pos, goalWorld) <= reachThreshold)
                return ReconstructPath(current, goalWorld);

            // --- 近傍展開 ---
            foreach (var next in Expand360(current, goalWorld))
            {
                if (closed.Contains(next.pos)) continue;

                // --- 障害物チェック ---
                Vector3 dir = (next.pos - current.pos);
                float dist = dir.magnitude;
                if (dist > 0f)
                {
                    dir /= dist;
                    if (Physics.SphereCast(current.pos, allyRadius, dir, out _, dist, obstacleMask))
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

        Debug.LogWarning("[AllyA*] 経路が見つかりませんでした。フォールバック経路を返します。");
        return new[] { goalWorld };
    }

    // --------------------------------------
    // 補助関数群
    // --------------------------------------
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
