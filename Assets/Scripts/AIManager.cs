using UnityEngine;
using System.Collections.Generic;

public class AIManager : MonoBehaviour
{
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform goalPoint;
    [SerializeField] private int spawnCount = 1;

    private List<AgentController> agents = new List<AgentController>();
    private MapManager mapManager;

    public int AgentCount => agents.Count;

    private void Awake()
    {
        mapManager = FindObjectOfType<MapManager>();
    }

    public void SpawnAgents()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject agentObj = Instantiate(agentPrefab, spawnPoint.position, Quaternion.identity);
            AgentController agent = agentObj.GetComponent<AgentController>();
            agent.name = $"Agent_{i}";
            agent.SetGoal(goalPoint);
            agents.Add(agent);
        }

        Debug.Log($"[AIManager] エージェントを {spawnCount} 体生成しました");
    }

    // ======== A*アルゴリズム本体 =========
    public Vector3[] GetPath(Vector3 start, Vector3 goal)
    {
        Node startNode = mapManager.GetClosestNode(start);
        Node goalNode = mapManager.GetClosestNode(goal);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                   (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == goalNode)
            {
                return RetracePath(startNode, goalNode);
            }

            foreach (Node neighbor in mapManager.GetNeighbors(currentNode))
            {
                if (closedSet.Contains(neighbor)) continue;

                float newCost = currentNode.gCost + Vector3.Distance(currentNode.worldPosition, neighbor.worldPosition);
                if (newCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost = Vector3.Distance(neighbor.worldPosition, goalNode.worldPosition);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        Debug.LogWarning("[AIManager] 経路が見つかりませんでした");
        return new Vector3[] { goal }; // フォールバック
    }

    private Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.worldPosition);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path.ToArray();
    }
}


//using UnityEngine;
//using System.Collections.Generic;

//public class AIManager : MonoBehaviour
//{
//    [SerializeField] private GameObject agentPrefab;
//    [SerializeField] private Transform spawnPoint;
//    [SerializeField] private Transform goalPoint;
//    [SerializeField] private int spawnCount = 1;

//    private List<AgentController> agents = new List<AgentController>();

//    public int AgentCount => agents.Count;

//    public void SpawnAgents()
//    {
//        for (int i = 0; i < spawnCount; i++)
//        {
//            GameObject agentObj = Instantiate(agentPrefab, spawnPoint.position, Quaternion.identity);
//            AgentController agent = agentObj.GetComponent<AgentController>();
//            agent.name = $"Agent_{i}";
//            agent.SetGoal(goalPoint);
//            agents.Add(agent);
//        }

//        Debug.Log($"[AIManager] エージェントを {spawnCount} 体生成しました");
//    }

//    // 仮の経路探索：スタートからゴールまで直線
//    public Vector3[] GetPath(Vector3 start, Vector3 goal)
//    {
//        return new Vector3[] { goal };
//    }
//}
