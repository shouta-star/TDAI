using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private AIManager aiManager;         // 経路探索管理のみ
    [SerializeField] private UIManager uiManager;         // UI制御
    [SerializeField] private HeatmapManager heatmapManager; // ヒートマップ出力（任意）

    private AgentController[] agents;   // シーン上に配置されたエージェントを取得
    private int totalAgents;
    private int completedAgents = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        var goal = GameObject.FindWithTag("Goal");
        foreach (var agent in FindObjectsOfType<AgentController>())
            agent.SetGoal(goal.transform);

        StartGame();
    }

    public void StartGame()
    {
        Debug.Log("[GameManager] ゲーム開始");

        // --- Scene上に配置されている全Agentを自動取得 ---
        agents = FindObjectsOfType<AgentController>();
        totalAgents = agents.Length;
        completedAgents = 0;

        Debug.Log($"[GameManager] シーン上のエージェント数: {totalAgents}");

        // --- 各マネージャ初期化 ---
        if (aiManager == null)
        {
            aiManager = FindObjectOfType<AIManager>();
            if (aiManager == null)
                Debug.LogWarning("[GameManager] AIManager がシーンに見つかりませんでした。");
        }

        if (uiManager != null)
            uiManager.InitializeUI();
        else
            Debug.LogWarning("[GameManager] UIManager が未設定です。");

        if (heatmapManager == null)
            Debug.LogWarning("[GameManager] HeatmapManager が未設定です。");
    }

    // --- エージェントがゴール到達を報告するためのメソッド ---
    public void ReportAgentGoal(AgentController agent)
    {
        completedAgents++;
        Debug.Log($"[GameManager] Agent {agent.name} がゴールしました ({completedAgents}/{totalAgents})");

        if (completedAgents >= totalAgents)
            EndGame();
    }

    private void EndGame()
    {
        Debug.Log("[GameManager] 全エージェント到達 → 結果処理開始");

        if (heatmapManager != null)
            heatmapManager.GenerateHeatmap();

        if (uiManager != null)
            uiManager.ShowResult();
    }
}


//using UnityEngine;

//public class GameManager : MonoBehaviour
//{
//    public static GameManager Instance;

//    [SerializeField] private AIManager aiManager;
//    [SerializeField] private UIManager uiManager;
//    [SerializeField] private HeatmapManager heatmapManager;

//    private int totalAgents;
//    private int completedAgents = 0;

//    private void Awake()
//    {
//        Instance = this;
//    }

//    private void Start()
//    {
//        StartGame();
//    }

//    public void StartGame()
//    {
//        Debug.Log("[GameManager] ゲーム開始");

//        aiManager.SpawnAgents();

//        totalAgents = aiManager.AgentCount;
//        completedAgents = 0;

//        uiManager.InitializeUI();
//    }

//    public void ReportAgentGoal(AgentController agent)
//    {
//        completedAgents++;
//        Debug.Log($"[GameManager] Agent {agent.name} がゴールしました ({completedAgents}/{totalAgents})");

//        if (completedAgents >= totalAgents)
//        {
//            EndGame();
//        }
//    }

//    private void EndGame()
//    {
//        Debug.Log("[GameManager] 全エージェント到達 → 結果処理開始");
//        heatmapManager.GenerateHeatmap();
//        uiManager.ShowResult();
//    }
//}
