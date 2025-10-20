using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private bool autoStart = true;

    private int agentsReached = 0;
    private int totalAgents = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private IEnumerator Start()
    {
        // AIManagerの初期化を待つ
        AIManager aiManager = null;
        while (aiManager == null)
        {
            aiManager = FindObjectOfType<AIManager>();
            yield return null;
        }

        // 壁登録完了を待つ（少し時間を置く）
        yield return new WaitForSeconds(0.5f);
        Debug.Log("[GameManager] 壁登録完了を検知 → 経路探索を開始");

        StartGame();
    }

    //private void StartGame()
    //{
    //    Debug.Log("[GameManager] ゲーム開始");

    //    var aiManager = FindObjectOfType<AIManager>();
    //    if (aiManager == null)
    //    {
    //        Debug.LogError("[GameManager] AIManagerが見つかりません！");
    //        return;
    //    }

    //    var agents = FindObjectsOfType<AgentController>();
    //    totalAgents = agents.Length;

    //    foreach (var agent in agents)
    //    {
    //        var data = agent.GetData();
    //        if (data == null) continue;

    //        var path = aiManager.GetPath(agent.transform.position, agent.GetGoalPosition(), data.aiType);
    //        agent.SetPath(path);
    //    }

    //    Debug.Log($"[GameManager] {totalAgents}体のエージェントを初期化完了");
    //}
    //private void StartGame()
    //{
    //    Debug.Log("[GameManager] ゲーム開始");

    //    var aiManager = FindObjectOfType<AIManager>();
    //    if (aiManager == null)
    //    {
    //        Debug.LogError("[GameManager] AIManagerが見つかりません！");
    //        return;
    //    }

    //    // --- Goalオブジェクトを取得 ---
    //    var goalObj = GameObject.FindWithTag("Goal");
    //    if (goalObj == null)
    //    {
    //        goalObj = GameObject.Find("Goal");
    //        if (goalObj == null)
    //        {
    //            Debug.LogError("[GameManager] Goalオブジェクトが見つかりません。");
    //            return;
    //        }
    //    }

    //    // --- Agentをすべて取得 ---
    //    var agents = FindObjectsOfType<AgentController>();
    //    totalAgents = agents.Length;

    //    // --- 各AgentにGoalを設定して経路探索 ---
    //    foreach (var agent in agents)
    //    {
    //        // Goalを明示的に設定
    //        agent.SetGoal(goalObj.transform);

    //        var data = agent.GetData();
    //        if (data == null)
    //        {
    //            Debug.LogWarning($"[GameManager] {agent.name} にAgentDataがありません。");
    //            continue;
    //        }

    //        // 経路計算
    //        var path = aiManager.GetPath(agent.transform.position, agent.GetGoalPosition(), data.aiType);
    //        if (path == null || path.Length == 0)
    //        {
    //            Debug.LogWarning($"[GameManager] {agent.name} の経路が見つかりません。");
    //            continue;
    //        }

    //        // 経路を適用
    //        agent.SetPath(path);
    //        Debug.Log($"[GameManager] {agent.name} 経路設定完了 (pathCount={path.Length})");
    //    }

    //    Debug.Log($"[GameManager] {totalAgents}体のエージェントを初期化完了");
    //}
    private void StartGame()
    {
        Debug.Log("[GameManager] ゲーム開始");

        var aiManager = FindObjectOfType<AIManager>();
        if (aiManager == null)
        {
            Debug.LogError("[GameManager] AIManagerが見つかりません！");
            return;
        }

        var goalObj = GameObject.FindWithTag("Goal");
        if (goalObj == null)
        {
            goalObj = GameObject.Find("Goal");
            if (goalObj == null)
            {
                Debug.LogError("[GameManager] Goalオブジェクトが見つかりません。");
                return;
            }
        }

        var agents = FindObjectsOfType<AgentController>();
        totalAgents = agents.Length; // ★ここでtotalAgentsを設定

        Debug.Log($"[GameManager] Total agents found: {totalAgents}"); // ★確認ログ追加

        foreach (var agent in agents)
        {
            agent.SetGoal(goalObj.transform);

            var data = agent.GetData();
            if (data == null)
            {
                Debug.LogWarning($"[GameManager] {agent.name} にAgentDataがありません。");
                continue;
            }

            var path = aiManager.GetPath(agent.transform.position, agent.GetGoalPosition(), data.aiType);
            if (path == null || path.Length == 0)
            {
                Debug.LogWarning($"[GameManager] {agent.name} の経路が見つかりません。");
                continue;
            }

            agent.SetPath(path);
            Debug.Log($"[GameManager] {agent.name} 経路設定完了 (pathCount={path.Length})");
        }

        Debug.Log($"[GameManager] {totalAgents}体のエージェントを初期化完了");
    }


    public void ReportAgentGoal(AgentController agent)
    {
        agentsReached++;
        Debug.Log($"[GameManager] Agent {agent.name} がゴールしました ({agentsReached}/{totalAgents})");

        if (agentsReached >= totalAgents)
            EndGame();
    }

    private void EndGame()
    {
        Debug.Log("[GameManager] 全エージェント到達 → 結果処理開始");

        var heatmap = FindObjectOfType<HeatmapManager>();
        if (heatmap != null)
        {
            heatmap.GenerateHeatmap();
        }

        // 今後必要ならここに結果集計・再試行処理を追加
    }
}


//using System.Collections;
//using UnityEngine;

//public class GameManager : MonoBehaviour
//{
//    public static GameManager Instance { get; private set; }

//    [SerializeField] private bool autoStart = true;

//    private int agentsReached = 0;
//    private int totalAgents = 0;

//    private void Awake()
//    {
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);
//    }

//    private IEnumerator Start()
//    {
//        // AIManagerの初期化を待つ
//        while (AIManager.Instance == null)
//            yield return null;

//        // 壁データが登録されるまで待機（方法②）
//        while (!AIManager.Instance.IsObstacleDataReady())
//            yield return null;

//        Debug.Log("[GameManager] 壁登録完了を検知 → 経路探索を開始");

//        // 初回の経路探索を安全に実行
//        StartGame();
//    }

//    private void StartGame()
//    {
//        Debug.Log("[GameManager] ゲーム開始");

//        var aiManager = AIManager.Instance;
//        if (aiManager == null)
//        {
//            Debug.LogError("[GameManager] AIManagerが見つかりません！");
//            return;
//        }

//        var agents = FindObjectsOfType<AgentController>();
//        totalAgents = agents.Length;

//        foreach (var agent in agents)
//        {
//            var data = agent.GetData();
//            if (data == null) continue;

//            var path = aiManager.GetPath(agent.transform.position, agent.GetGoalPosition(), data.aiType);
//            agent.SetPath(path);
//        }

//        Debug.Log($"[GameManager] {totalAgents}体のエージェントを初期化完了");
//    }

//    public void ReportAgentGoal(AgentController agent)
//    {
//        agentsReached++;
//        Debug.Log($"[GameManager] Agent {agent.name} がゴールしました ({agentsReached}/{totalAgents})");

//        if (agentsReached >= totalAgents)
//            EndGame();
//    }

//    private void EndGame()
//    {
//        Debug.Log("[GameManager] 全エージェント到達 → 結果処理開始");

//        var heatmap = FindObjectOfType<HeatmapManager>();
//        if (heatmap != null)
//        {
//            // 通過点数ログ呼び出しは削除（B案）
//            heatmap.GenerateHeatmap();
//        }

//        // 必要に応じて他の終了処理を追加
//    }
//}
