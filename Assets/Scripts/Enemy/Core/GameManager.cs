using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private AIManager aiManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private HeatmapManager heatmapManager;

    private int totalAgents;
    private int completedAgents = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        Debug.Log("[GameManager] ゲーム開始");

        aiManager.SpawnAgents();

        totalAgents = aiManager.AgentCount;
        completedAgents = 0;

        uiManager.InitializeUI();
    }

    public void ReportAgentGoal(AgentController agent)
    {
        completedAgents++;
        Debug.Log($"[GameManager] Agent {agent.name} がゴールしました ({completedAgents}/{totalAgents})");

        if (completedAgents >= totalAgents)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        Debug.Log("[GameManager] 全エージェント到達 → 結果処理開始");
        heatmapManager.GenerateHeatmap();
        uiManager.ShowResult();
    }
}
