using UnityEngine;
using System.Collections.Generic;

public class AIManager : MonoBehaviour
{
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform goalPoint;
    [SerializeField] private int spawnCount = 1;

    private List<AgentController> agents = new List<AgentController>();

    public int AgentCount => agents.Count;

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

    // 仮の経路探索：スタートからゴールまで直線
    public Vector3[] GetPath(Vector3 start, Vector3 goal)
    {
        return new Vector3[] { goal };
    }
}
