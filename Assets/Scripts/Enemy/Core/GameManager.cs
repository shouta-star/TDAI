using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<Transform> allGoals = new List<Transform>();
    private HashSet<Transform> reachedGoals = new HashSet<Transform>();

    public bool IsGameEnded { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f); // 少し待機してからGoal登録

        // --- Goal登録 ---
        var goalObjects = GameObject.FindGameObjectsWithTag("Goal");
        foreach (var g in goalObjects)
        {
            allGoals.Add(g.transform);
        }

        for (int i = 0; i < allGoals.Count; i++)
        {
            var t = allGoals[i];
            Debug.Log($"[GM/Goal] #{i} name='{t.name}' pos={t.position}");
        }

        Debug.Log($"[GameManager] Goal数: {allGoals.Count}");

        yield return new WaitForSeconds(0.5f);
        StartGame();
    }

    public void StartGame()
    {
        Debug.Log("[GameManager] ゲーム開始");

        var agents = FindObjectsOfType<AgentController>();
        Debug.Log($"[GameManager] エージェント数: {agents.Length}");

        foreach (var agent in agents)
        {
            agent.InitializeGoals(allGoals);
            agent.SelectNextGoal();
            agent.RecalculatePath();
        }

        Debug.Log("[GameManager] すべてのエージェントを初期化完了");
    }

    public List<Transform> GetUnreachedGoals()
    {
        List<Transform> result = new List<Transform>();
        foreach (var g in allGoals)
        {
            if (!reachedGoals.Contains(g))
                result.Add(g);
        }
        return result;
    }

    public void ReportGoalReached(Transform goal, AgentController agent)
    {
        if (goal == null)
        {
            Debug.LogWarning($"[GameManager] {agent.name} のゴールが未設定のまま報告されました。処理をスキップします。");
            return;
        }

        if (reachedGoals.Contains(goal)) return;

        reachedGoals.Add(goal);
        Debug.Log($"[GameManager] {agent.name} が {goal.name} に到達 pos={goal.position} ({reachedGoals.Count}/{allGoals.Count})");

        if (reachedGoals.Count >= allGoals.Count)
        {
            EndGame();
        }
    }

    public void DumpGoals(string tag = "GM")
    {
        for (int i = 0; i < allGoals.Count; i++)
        {
            var t = allGoals[i];
            bool reached = reachedGoals.Contains(t);
            Debug.Log($"[{tag}/GoalList] #{i} name='{t.name}' pos={t.position} reached={reached}");
        }
    }

    public void EndGame()
    {
        if (IsGameEnded) return;
        IsGameEnded = true;
        Debug.Log("[GameManager] 全Goal到達 → ゲームクリア！");

        // HeatmapManagerが存在するならヒートマップ生成
        //var heatmap = FindObjectOfType<HeatmapManager>();
        //if (heatmap != null)
        //{
        //    heatmap.GenerateHeatmap();
        //    //Debug.Log($"[HeatmapManager] 通過点数: {heatmap.PointCount}");
        //}
    }
}


//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class GameManager : MonoBehaviour
//{
//    public static GameManager Instance { get; private set; }

//    private List<Transform> allGoals = new List<Transform>();
//    private HashSet<Transform> reachedGoals = new HashSet<Transform>();

//    private void Awake()
//    {
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);
//    }

//    //private IEnumerator Start()
//    //{
//    //    // --- AIManager初期化を待機 ---
//    //    while (AIManager.Instance == null)
//    //        yield return null;

//    //    // --- 少し待ってから開始 ---
//    //    yield return new WaitForSeconds(0.5f);
//    //    Debug.Log("[GameManager] 初期化完了 → 経路探索を開始");

//    //    // --- Goal登録 ---
//    //    foreach (var g in GameObject.FindGameObjectsWithTag("Goal"))
//    //        allGoals.Add(g.transform);

//    //    Debug.Log($"[GameManager] Goal数: {allGoals.Count}");

//    //    StartGame();
//    //}
//    private IEnumerator Start()
//    {
//        // --- AIManager初期化を待機 ---
//        while (AIManager.Instance == null)
//            yield return null;

//        // --- Goal登録 ---
//        var goalObjects = GameObject.FindGameObjectsWithTag("Goal");
//        while (goalObjects.Length == 0)
//        {
//            Debug.LogWarning("[GameManager] GoalがまだSceneに存在しません。待機中...");
//            yield return null;
//            goalObjects = GameObject.FindGameObjectsWithTag("Goal");
//        }

//        foreach (var g in goalObjects)
//            allGoals.Add(g.transform);

//        Debug.Log($"[GameManager] Goal数: {allGoals.Count}");

//        yield return new WaitForSeconds(0.5f);
//        StartGame();
//    }


//    //============================================================
//    // ゲーム開始
//    //============================================================
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
//        Debug.Log($"[GameManager] エージェント数: {agents.Length}");

//        foreach (var agent in agents)
//        {
//            agent.InitializeGoals(allGoals); // Goalリストを渡す
//            agent.SelectNextGoal();          // 最寄りGoalを設定
//            agent.RecalculatePath();         // 経路探索開始
//        }

//        Debug.Log($"[GameManager] {agents.Length}体のエージェントを初期化完了");
//    }

//    //============================================================
//    // Goal到達報告
//    //============================================================
//    public void ReportGoalReached(Transform goal, AgentController agent)
//    {
//        if (goal == null)
//        {
//            Debug.LogWarning($"[GameManager] {agent.name} のゴールが未設定のまま報告されました。処理をスキップします。");
//            return;
//        }

//        if (reachedGoals.Contains(goal))
//            return;

//        reachedGoals.Add(goal);
//        Debug.Log($"[GameManager] {agent.name} が {goal.name} に到達 ({reachedGoals.Count}/{allGoals.Count})");

//        if (reachedGoals.Count >= allGoals.Count)
//        {
//            EndGame();
//        }
//    }

//    //============================================================
//    // 未到達Goalの取得
//    //============================================================
//    public List<Transform> GetUnreachedGoals()
//    {
//        List<Transform> list = new List<Transform>();
//        foreach (var g in allGoals)
//        {
//            if (!reachedGoals.Contains(g))
//                list.Add(g);
//        }
//        return list;
//    }

//    //============================================================
//    // ゲーム終了
//    //============================================================
//    private void EndGame()
//    {
//        Debug.Log("[GameManager] 全Goal到達 → ゲームクリア！");

//        var heatmap = FindObjectOfType<HeatmapManager>();
//        if (heatmap != null)
//        {
//            heatmap.GenerateHeatmap();
//        }
//    }
//}


////using System.Collections;
////using UnityEngine;

////public class GameManager : MonoBehaviour
////{
////    public static GameManager Instance { get; private set; }

////    [SerializeField] private bool autoStart = true;

////    private int agentsReached = 0;
////    private int totalAgents = 0;

////    private void Awake()
////    {
////        if (Instance == null)
////            Instance = this;
////        else
////            Destroy(gameObject);
////    }

////    private IEnumerator Start()
////    {
////        // AIManagerの初期化を待つ
////        AIManager aiManager = null;
////        while (aiManager == null)
////        {
////            aiManager = FindObjectOfType<AIManager>();
////            yield return null;
////        }

////        // 壁登録完了を待つ（少し時間を置く）
////        yield return new WaitForSeconds(0.5f);
////        Debug.Log("[GameManager] 壁登録完了を検知 → 経路探索を開始");

////        StartGame();
////    }

////    //private void StartGame()
////    //{
////    //    Debug.Log("[GameManager] ゲーム開始");

////    //    var aiManager = FindObjectOfType<AIManager>();
////    //    if (aiManager == null)
////    //    {
////    //        Debug.LogError("[GameManager] AIManagerが見つかりません！");
////    //        return;
////    //    }

////    //    var agents = FindObjectsOfType<AgentController>();
////    //    totalAgents = agents.Length;

////    //    foreach (var agent in agents)
////    //    {
////    //        var data = agent.GetData();
////    //        if (data == null) continue;

////    //        var path = aiManager.GetPath(agent.transform.position, agent.GetGoalPosition(), data.aiType);
////    //        agent.SetPath(path);
////    //    }

////    //    Debug.Log($"[GameManager] {totalAgents}体のエージェントを初期化完了");
////    //}
////    //private void StartGame()
////    //{
////    //    Debug.Log("[GameManager] ゲーム開始");

////    //    var aiManager = FindObjectOfType<AIManager>();
////    //    if (aiManager == null)
////    //    {
////    //        Debug.LogError("[GameManager] AIManagerが見つかりません！");
////    //        return;
////    //    }

////    //    // --- Goalオブジェクトを取得 ---
////    //    var goalObj = GameObject.FindWithTag("Goal");
////    //    if (goalObj == null)
////    //    {
////    //        goalObj = GameObject.Find("Goal");
////    //        if (goalObj == null)
////    //        {
////    //            Debug.LogError("[GameManager] Goalオブジェクトが見つかりません。");
////    //            return;
////    //        }
////    //    }

////    //    // --- Agentをすべて取得 ---
////    //    var agents = FindObjectsOfType<AgentController>();
////    //    totalAgents = agents.Length;

////    //    // --- 各AgentにGoalを設定して経路探索 ---
////    //    foreach (var agent in agents)
////    //    {
////    //        // Goalを明示的に設定
////    //        agent.SetGoal(goalObj.transform);

////    //        var data = agent.GetData();
////    //        if (data == null)
////    //        {
////    //            Debug.LogWarning($"[GameManager] {agent.name} にAgentDataがありません。");
////    //            continue;
////    //        }

////    //        // 経路計算
////    //        var path = aiManager.GetPath(agent.transform.position, agent.GetGoalPosition(), data.aiType);
////    //        if (path == null || path.Length == 0)
////    //        {
////    //            Debug.LogWarning($"[GameManager] {agent.name} の経路が見つかりません。");
////    //            continue;
////    //        }

////    //        // 経路を適用
////    //        agent.SetPath(path);
////    //        Debug.Log($"[GameManager] {agent.name} 経路設定完了 (pathCount={path.Length})");
////    //    }

////    //    Debug.Log($"[GameManager] {totalAgents}体のエージェントを初期化完了");
////    //}
////    private void StartGame()
////    {
////        Debug.Log("[GameManager] ゲーム開始");

////        var aiManager = FindObjectOfType<AIManager>();
////        if (aiManager == null)
////        {
////            Debug.LogError("[GameManager] AIManagerが見つかりません！");
////            return;
////        }

////        var goalObj = GameObject.FindWithTag("Goal");
////        if (goalObj == null)
////        {
////            goalObj = GameObject.Find("Goal");
////            if (goalObj == null)
////            {
////                Debug.LogError("[GameManager] Goalオブジェクトが見つかりません。");
////                return;
////            }
////        }

////        var agents = FindObjectsOfType<AgentController>();
////        totalAgents = agents.Length; // ★ここでtotalAgentsを設定

////        Debug.Log($"[GameManager] Total agents found: {totalAgents}"); // ★確認ログ追加

////        foreach (var agent in agents)
////        {
////            agent.SetGoal(goalObj.transform);

////            var data = agent.GetData();
////            if (data == null)
////            {
////                Debug.LogWarning($"[GameManager] {agent.name} にAgentDataがありません。");
////                continue;
////            }

////            var path = aiManager.GetPath(agent.transform.position, agent.GetGoalPosition(), data.aiType);
////            if (path == null || path.Length == 0)
////            {
////                Debug.LogWarning($"[GameManager] {agent.name} の経路が見つかりません。");
////                continue;
////            }

////            agent.SetPath(path);
////            Debug.Log($"[GameManager] {agent.name} 経路設定完了 (pathCount={path.Length})");

////            // 経路を適用する直前 or 直後どちらでもOK（両方でも可）
////            Debug.Log($"[GM-Path] {agent.name} ai={data.aiType} start={agent.transform.position} goal={agent.GetGoalPosition()} " +
////                      $"len={(path == null ? -1 : path.Length)} first={(path != null && path.Length > 0 ? path[0].ToString() : "N/A")} " +
////                      $"last={(path != null && path.Length > 0 ? path[path.Length - 1].ToString() : "N/A")}");
////        }

////        Debug.Log($"[GameManager] {totalAgents}体のエージェントを初期化完了");
////    }


////    public void ReportAgentGoal(AgentController agent)
////    {
////        agentsReached++;
////        Debug.Log($"[GameManager] Agent {agent.name} がゴールしました ({agentsReached}/{totalAgents})");

////        if (agentsReached >= totalAgents)
////            EndGame();
////    }

////    private void EndGame()
////    {
////        Debug.Log("[GameManager] 全エージェント到達 → 結果処理開始");

////        var heatmap = FindObjectOfType<HeatmapManager>();
////        if (heatmap != null)
////        {
////            heatmap.GenerateHeatmap();
////        }

////        // 今後必要ならここに結果集計・再試行処理を追加
////    }
////}


//////using System.Collections;
//////using UnityEngine;

//////public class GameManager : MonoBehaviour
//////{
//////    public static GameManager Instance { get; private set; }

//////    [SerializeField] private bool autoStart = true;

//////    private int agentsReached = 0;
//////    private int totalAgents = 0;

//////    private void Awake()
//////    {
//////        if (Instance == null)
//////            Instance = this;
//////        else
//////            Destroy(gameObject);
//////    }

//////    private IEnumerator Start()
//////    {
//////        // AIManagerの初期化を待つ
//////        while (AIManager.Instance == null)
//////            yield return null;

//////        // 壁データが登録されるまで待機（方法②）
//////        while (!AIManager.Instance.IsObstacleDataReady())
//////            yield return null;

//////        Debug.Log("[GameManager] 壁登録完了を検知 → 経路探索を開始");

//////        // 初回の経路探索を安全に実行
//////        StartGame();
//////    }

//////    private void StartGame()
//////    {
//////        Debug.Log("[GameManager] ゲーム開始");

//////        var aiManager = AIManager.Instance;
//////        if (aiManager == null)
//////        {
//////            Debug.LogError("[GameManager] AIManagerが見つかりません！");
//////            return;
//////        }

//////        var agents = FindObjectsOfType<AgentController>();
//////        totalAgents = agents.Length;

//////        foreach (var agent in agents)
//////        {
//////            var data = agent.GetData();
//////            if (data == null) continue;

//////            var path = aiManager.GetPath(agent.transform.position, agent.GetGoalPosition(), data.aiType);
//////            agent.SetPath(path);
//////        }

//////        Debug.Log($"[GameManager] {totalAgents}体のエージェントを初期化完了");
//////    }

//////    public void ReportAgentGoal(AgentController agent)
//////    {
//////        agentsReached++;
//////        Debug.Log($"[GameManager] Agent {agent.name} がゴールしました ({agentsReached}/{totalAgents})");

//////        if (agentsReached >= totalAgents)
//////            EndGame();
//////    }

//////    private void EndGame()
//////    {
//////        Debug.Log("[GameManager] 全エージェント到達 → 結果処理開始");

//////        var heatmap = FindObjectOfType<HeatmapManager>();
//////        if (heatmap != null)
//////        {
//////            // 通過点数ログ呼び出しは削除（B案）
//////            heatmap.GenerateHeatmap();
//////        }

//////        // 必要に応じて他の終了処理を追加
//////    }
//////}
