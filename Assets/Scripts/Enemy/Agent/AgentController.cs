using UnityEngine;
using PathIntelligence;

public class AgentController : MonoBehaviour
{
    [SerializeField] private AgentData data;
    private AgentData runtimeData;

    public Vector3[] currentPath;
    public int pathIndex = 0;
    public bool needReplan = false;

    private float lastReplanTime = -999f;
    private const float REPLAN_COOLDOWN = 0.5f;

    private AIManager aiManager;
    private float defaultMoveSpeed;
    private AgentBaseState currentState;

    public bool reachedGoal = false;
    private Transform goal;

    //==============================================================
    // 初期化
    //==============================================================
    private void Start()
    {
        runtimeData = Instantiate(data);
        defaultMoveSpeed = runtimeData.moveSpeed;
        aiManager = AIManager.Instance;

        // --- ゴールを自動検出 ---
        goal = GameObject.FindWithTag("Goal")?.transform;
        if (goal != null)
            Debug.Log($"[{name}] Goal自動検出: {goal.name}");

        // --- 初期状態をIdleに設定 ---
        ChangeState(new IdleState());
    }

    //==============================================================
    // 毎フレーム更新
    //==============================================================
    private void Update()
    {
        if (currentState != null)
            currentState.Execute(this);
    }

    //==============================================================
    // 状態遷移管理
    //==============================================================
    public void ChangeState(AgentBaseState newState)
    {
        if (currentState != null)
            currentState.Exit(this);

        currentState = newState;

        if (currentState != null)
            currentState.Enter(this);
    }

    //==============================================================
    // 経路移動処理
    //==============================================================
    public void MoveTo(Vector3 destination)
    {
        Vector3 current = transform.position;
        Vector3 next = Vector3.MoveTowards(current, destination, runtimeData.moveSpeed * Time.deltaTime);

        // --- 通行不能チェック ---
        if (AIManager.Instance != null && AIManager.Instance.HasObstacleBetween(current, next))
        {
            if (runtimeData.aiType == AIType.DStar)
            {
                // ★ D*専用再探索（0.5秒クールダウン）
                if (Time.time - lastReplanTime > REPLAN_COOLDOWN)
                {
                    lastReplanTime = Time.time;
                    Debug.Log($"[{name}] D*再探索開始 (cooldown OK)");
                    RecalculatePath();
                }
            }
            return;
        }

        transform.position = next;
    }

    //==============================================================
    // 経路再設定
    //==============================================================
    public void SetPath(Vector3[] path)
    {
        Debug.Log($"[SetPath:IN] {name} curPos={transform.position} newLen={path.Length}");

        if (path == null || path.Length == 0) return;

        // --- 現在位置に最も近いノードを探索し、そこから再開 ---
        float minDist = float.MaxValue;
        int nearest = 0;
        for (int i = 0; i < path.Length; i++)
        {
            float d = Vector3.Distance(transform.position, path[i]);
            if (d < minDist)
            {
                minDist = d;
                nearest = i;
            }
        }

        pathIndex = nearest;

        Debug.Log($"[SetPath:NEAREST] {name} startIndex={nearest} first={path[0]} last={path[path.Length - 1]}");

        currentPath = path;
        Debug.Log($"[{name}] 経路再設定完了: {path.Length}ノード (startIndex={pathIndex})");

        Debug.Log($"[SetPath:OUT] {name} pathIndex={pathIndex} len={currentPath?.Length}");
    }

    //==============================================================
    // 経路再探索
    //==============================================================
    public void RecalculatePath()
    {
        if (aiManager == null) aiManager = AIManager.Instance;
        if (aiManager == null) return;

        Vector3 goalPos = GetGoalPosition();
        var newPath = aiManager.GetPath(transform.position, goalPos, runtimeData.aiType);
        if (newPath != null && newPath.Length > 1)
            SetPath(newPath);
    }

    //==============================================================
    // ゴール管理
    //==============================================================
    public void SetGoal(Transform newGoal)
    {
        goal = newGoal;
    }

    public Vector3 GetGoalPosition()
    {
        if (goal != null)
            return goal.position;
        return Vector3.zero;
    }

    //==============================================================
    // 攻撃処理（AttackStateから呼ばれる）
    //==============================================================
    public void Attack(AgentController targetAgent)
    {
        if (targetAgent == null) return;

        var targetHP = targetAgent.GetComponent<AgentHealth>();
        if (targetHP != null)
        {
            targetHP.TakeDamage(runtimeData.attackPower, name);
            Debug.Log($"[{name}] が [{targetAgent.name}] に {runtimeData.attackPower} ダメージを与えた");
        }
        else
        {
            Debug.LogWarning($"[{targetAgent.name}] に AgentHealth が見つかりません");
        }
    }

    public void Attack()
    {
        Debug.Log($"[{name}] 攻撃実行（汎用呼び出し）");
    }

    //==============================================================
    // 次の座標（MoveStateやログ出力用）
    //==============================================================
    public Vector3 GetNextPosition()
    {
        if (currentPath == null || pathIndex >= currentPath.Length)
            return transform.position;
        return currentPath[pathIndex];
    }

    //==============================================================
    // 各種情報取得
    //==============================================================
    public AgentData GetData() => runtimeData;
}


//using UnityEngine;

//public class AgentController : MonoBehaviour
//{
//    [SerializeField] private AgentData data;
//    private AgentData runtimeData;

//    public Vector3[] currentPath;
//    public int pathIndex = 0;
//    public bool needReplan = false;

//    private float lastReplanTime = -999f;
//    private const float REPLAN_COOLDOWN = 0.5f;

//    private AIManager aiManager;
//    private float defaultMoveSpeed;

//    private void Start()
//    {
//        runtimeData = Instantiate(data);
//        defaultMoveSpeed = runtimeData.moveSpeed;
//        aiManager = AIManager.Instance;
//    }

//    private void Update()
//    {
//        // 状態マシン内で自動実行
//    }

//    public void MoveTo(Vector3 destination)
//    {
//        Vector3 current = transform.position;
//        Vector3 next = Vector3.MoveTowards(current, destination, runtimeData.moveSpeed * Time.deltaTime);

//        // --- 通行不能チェック ---
//        if (AIManager.Instance != null && AIManager.Instance.HasObstacleBetween(current, next))
//        {
//            if (runtimeData.aiType == AIType.DStar)
//            {
//                if (Time.time - lastReplanTime > REPLAN_COOLDOWN)
//                {
//                    lastReplanTime = Time.time;
//                    Debug.Log($"[{name}] D*再探索開始 (cooldown OK)");
//                    RecalculatePath();
//                }
//            }
//            return;
//        }

//        transform.position = next;
//    }

//    public void SetPath(Vector3[] path)
//    {
//        if (path == null || path.Length == 0) return;

//        // 近いノードを検索して pathIndex を補正
//        float minDist = float.MaxValue;
//        int nearest = 0;
//        for (int i = 0; i < path.Length; i++)
//        {
//            float d = Vector3.Distance(transform.position, path[i]);
//            if (d < minDist)
//            {
//                minDist = d;
//                nearest = i;
//            }
//        }

//        pathIndex = nearest;
//        currentPath = path;
//        Debug.Log($"[{name}] 経路再設定完了: {path.Length}ノード (startIndex={pathIndex})");
//    }

//    public void RecalculatePath()
//    {
//        if (aiManager == null) aiManager = AIManager.Instance;
//        if (aiManager == null) return;

//        Vector3 goal = GameObject.FindWithTag("Goal")?.transform.position ?? Vector3.zero;
//        var newPath = aiManager.GetPath(transform.position, goal, runtimeData.aiType);
//        if (newPath != null && newPath.Length > 1) SetPath(newPath);
//    }

//    public AgentData GetData() => runtimeData;
//}


////using UnityEngine;
////using System.Collections;
////using PathIntelligence;

////[ExecuteAlways]
////public class AgentController : MonoBehaviour
////{
////    [SerializeField] private AgentData data;
////    private AgentData runtimeData;

////    private AgentBaseState currentState;

////    private IdleState idleState = new IdleState();
////    private SearchState searchState = new SearchState();
////    private MoveState moveState = new MoveState();
////    private AvoidState avoidState = new AvoidState();
////    private GoalState goalState = new GoalState();

////    private Transform goal;

////    [HideInInspector] public Vector3[] currentPath;
////    [HideInInspector] public int pathIndex = 0;
////    [HideInInspector] public bool needReplan = false;
////    [HideInInspector] public bool reachedGoal = false;

////    private float defaultMoveSpeed;
////    private AIManager aiManager;

////    private Vector3 nextPosition;
////    public Vector3 GetNextPosition() => nextPosition;

////    private Vector3[] initialAStarPath;

////    private static Vector3[] lastAStarPath;
////    private static Vector3[] lastDStarPath;

////    private void Start()
////    {
////        // Goal検索（GameManagerが設定する前のフォールバック）
////        if (goal == null)
////        {
////            GameObject goalObj = GameObject.FindWithTag("Goal");
////            if (goalObj == null)
////                goalObj = GameObject.Find("Goal");

////            if (goalObj != null)
////            {
////                goal = goalObj.transform;
////                Debug.Log($"[{name}] Goal自動検出: {goal.name}");
////            }
////            else
////            {
////                Debug.LogWarning($"[{name}] Goalオブジェクトが見つかりません。");
////            }
////        }

////        nextPosition = transform.position;

////        // データの初期化
////        if (data == null)
////        {
////            Debug.LogError($"[{name}] AgentDataが未設定です！");
////            return;
////        }

////        runtimeData = Instantiate(data);
////        defaultMoveSpeed = runtimeData.moveSpeed;

////        // AIManagerを取得
////        aiManager = FindObjectOfType<AIManager>();
////        if (aiManager == null)
////        {
////            Debug.LogError("[AgentController] AIManagerがシーンに存在しません。");
////            return;
////        }

////        // IdleStateから開始（GameManagerが経路を設定する）
////        ChangeState(idleState);

////        Debug.Log($"[{name}] 初期化完了 - AI: {runtimeData.aiType}, Speed: {runtimeData.moveSpeed}");
////    }

////    private void Update()
////    {
////        currentState?.Execute(this);
////    }

////    public void ChangeState(AgentBaseState newState)
////    {
////        currentState?.Exit(this);
////        currentState = newState;
////        currentState.Enter(this);
////    }

////    public void MoveTo(Vector3 destination)
////    {
////        if (runtimeData.moveSpeed <= 0f)
////            return;

////        Vector3 current = transform.position;
////        Vector3 next = Vector3.MoveTowards(
////            current,
////            destination,
////            runtimeData.moveSpeed * Time.deltaTime
////        );

////        // 障害物チェック
////        if (AIManager.Instance != null && AIManager.Instance.HasObstacleBetween(current, next))
////        {
////            Debug.Log($"[{name}] 進行方向に障害物あり → 経路再計算開始 ({runtimeData.aiType})");
////            return;
////        }

////        nextPosition = next;
////        transform.position = next;
////    }

////    public void Attack(AgentController target)
////    {
////        if (target == null) return;

////        var health = target.GetComponent<AgentHealth>();
////        if (health != null)
////        {
////            health.TakeDamage(runtimeData.attackPower, gameObject.name);
////            Debug.Log($"[{name}] が [{target.name}] に {runtimeData.attackPower} ダメージを与えた！");
////        }
////    }

////    public void SetGoal(Transform goalTransform)
////    {
////        goal = goalTransform;
////        //Debug.Log($"[{name}] Goal設定: {goal.name}");
////        Debug.Log($"[Agent:{name}] Goal set → {goal.position}");
////    }

////    public Vector3 GetGoalPosition()
////    {
////        if (goal == null)
////        {
////            Debug.LogWarning($"[{name}] goalが未設定です。");
////            return Vector3.positiveInfinity;
////        }
////        return goal.position;
////    }

////    //public void SetPath(Vector3[] path)
////    //{
////    //    if (path == null || path.Length == 0)
////    //    {
////    //        Debug.LogWarning($"[{name}] 空の経路が設定されました。");
////    //        return;
////    //    }

////    //    currentPath = path;
////    //    pathIndex = 0;

////    //    // A*の場合、初回経路を保存
////    //    var d = GetData();
////    //    if (d != null && d.aiType == AIType.AStar && initialAStarPath == null && path.Length >= 2)
////    //    {
////    //        initialAStarPath = (Vector3[])path.Clone();
////    //    }

////    //    Debug.Log($"[{name}] 経路設定完了: {path.Length}ノード");
////    //}
////    public void SetPath(Vector3[] path)
////    {
////        if (path == null || path.Length == 0)
////        {
////            Debug.LogWarning($"[{name}] 空の経路が設定されました。");
////            return;
////        }

////        currentPath = path;

////        // --- ★ 現在位置に最も近いノードを探して補正 ---
////        float minDist = float.MaxValue;
////        int nearestIndex = 0;
////        Vector3 currentPos = transform.position;

////        for (int i = 0; i < path.Length; i++)
////        {
////            float dist = Vector3.Distance(currentPos, path[i]);
////            if (dist < minDist)
////            {
////                minDist = dist;
////                nearestIndex = i;
////            }
////        }

////        pathIndex = nearestIndex;
////        Debug.Log($"[{name}] 経路設定完了: {path.Length}ノード (開始index={pathIndex}, dist={minDist:F3})");

////        var d = GetData();
////        if (d != null && d.aiType == AIType.AStar && initialAStarPath == null && path.Length >= 2)
////        {
////            initialAStarPath = (Vector3[])path.Clone();
////        }
////    }


////    public void OnDynamicMapChanged(Vector3 changedPos, bool isBlocked)
////    {
////        if (runtimeData.aiType != AIType.DStar) return;
////        if (currentPath == null || currentPath.Length == 0) return;

////        foreach (var p in currentPath)
////        {
////            if (Vector3.Distance(p, changedPos) < 1.0f)
////            {
////                Debug.Log($"[D*:{name}] 動的障害物に反応 → 経路再計算開始");
////                RecalculatePath();
////                return;
////            }
////        }
////    }

////    public void RecalculatePath()
////    {
////        if (aiManager == null)
////        {
////            aiManager = FindObjectOfType<AIManager>();
////            if (aiManager == null)
////            {
////                Debug.LogError("[AgentController] AIManagerが見つからないため経路再計算できません。");
////                return;
////            }
////        }

////        Vector3 start = transform.position;
////        Vector3 goalPos = GetGoalPosition();

////        if (goalPos == Vector3.positiveInfinity)
////        {
////            Debug.LogError($"[{name}] Goal未設定のため経路再計算できません。");
////            return;
////        }

////        var path = aiManager.GetPath(start, goalPos, runtimeData.aiType);
////        if (path == null || path.Length == 0)
////        {
////            Debug.LogWarning($"[AgentController:{name}] 経路が取得できませんでした。");
////            return;
////        }

////        SetPath(path);
////        needReplan = false;
////        reachedGoal = false;
////        Debug.Log($"[AgentController:{name}] RecalculatePath → {runtimeData.aiType}, nodes={path.Length}");
////    }

////    public AgentData GetData() => runtimeData;

////    public void StopMovement()
////    {
////        runtimeData.moveSpeed = 0f;
////        Debug.Log($"[{name}] StopMovement: speed → 0");
////    }

////    public void RestoreDefaultSpeed()
////    {
////        runtimeData.moveSpeed = defaultMoveSpeed;
////        Debug.Log($"[{name}] RestoreDefaultSpeed: speed → {runtimeData.moveSpeed}");
////    }

////    private void OnDrawGizmos()
////    {
////        if (Application.isPlaying)
////        {
////            var d = GetData();

////            // A*は初回経路を常に表示
////            if (d != null && d.aiType == AIType.AStar && initialAStarPath != null && initialAStarPath.Length >= 2)
////            {
////                Gizmos.color = Color.yellow;
////                for (int i = 0; i < initialAStarPath.Length - 1; i++)
////                    Gizmos.DrawLine(initialAStarPath[i], initialAStarPath[i + 1]);
////            }

////            // 現在の経路表示
////            if (currentPath != null && currentPath.Length >= 2)
////            {
////                if (d != null && d.aiType == AIType.DStar)
////                    Gizmos.color = Color.cyan;
////                else
////                    Gizmos.color = Color.yellow;

////                for (int i = 0; i < currentPath.Length - 1; i++)
////                    Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
////            }
////            return;
////        }

////        // 停止中は最後の経路を表示
////        if (lastAStarPath != null && lastAStarPath.Length >= 2)
////        {
////            Gizmos.color = Color.yellow;
////            for (int i = 0; i < lastAStarPath.Length - 1; i++)
////                Gizmos.DrawLine(lastAStarPath[i], lastAStarPath[i + 1]);
////        }
////        if (lastDStarPath != null && lastDStarPath.Length >= 2)
////        {
////            Gizmos.color = Color.cyan;
////            for (int i = 0; i < lastDStarPath.Length - 1; i++)
////                Gizmos.DrawLine(lastDStarPath[i], lastDStarPath[i + 1]);
////        }
////    }

////    public bool HasPath()
////    {
////        return currentPath != null && currentPath.Length > 0;
////    }
////}

//////using UnityEngine;
//////using System.Collections;
//////using PathIntelligence;

//////[ExecuteAlways]
//////public class AgentController : MonoBehaviour
//////{
//////    [SerializeField] private AgentData data;
//////    private AgentData runtimeData;

//////    private AgentBaseState currentState;

//////    private IdleState idleState = new IdleState();
//////    private SearchState searchState = new SearchState();
//////    private MoveState moveState = new MoveState();
//////    private AvoidState avoidState = new AvoidState();
//////    private GoalState goalState = new GoalState();

//////    private Transform goal;

//////    [HideInInspector] public Vector3[] currentPath;
//////    [HideInInspector] public int pathIndex = 0;
//////    [HideInInspector] public bool needReplan = false;
//////    [HideInInspector] public bool reachedGoal = false;

//////    private float defaultMoveSpeed;
//////    private AIManager aiManager;

//////    private Vector3 nextPosition;
//////    public Vector3 GetNextPosition() => nextPosition;

//////    // AgentController 内のフィールド群に追加
//////    private Vector3[] initialAStarPath;  // ← A*で最初に得た経路のスナップショット

//////    // 経路のバックアップ（停止中表示用）
//////    private static Vector3[] lastAStarPath;
//////    private static Vector3[] lastDStarPath;

//////    private void Start()
//////    {
//////        //return;

//////        // すでに他スクリプトから設定済みでなければ、自動検索
//////        if (goal == null)
//////        {
//////            GameObject goalObj = GameObject.FindWithTag("Goal");
//////            if (goalObj == null)
//////                goalObj = GameObject.Find("Goal");

//////            if (goalObj != null)
//////                goal = goalObj.transform;
//////            else
//////                Debug.LogWarning($"[{name}] Goal オブジェクトが見つかりません。");
//////        }

//////        nextPosition = transform.position;

//////        runtimeData = Instantiate(data);
//////        defaultMoveSpeed = runtimeData.moveSpeed;

//////        aiManager = FindObjectOfType<AIManager>();
//////        if (aiManager == null)
//////            Debug.LogError("[AgentController] AIManager がシーンに存在しません。");

//////        ChangeState(idleState);
//////    }

//////    private void Update()
//////    {
//////        currentState?.Execute(this);
//////    }

//////    public void ChangeState(AgentBaseState newState)
//////    {
//////        currentState?.Exit(this);
//////        currentState = newState;
//////        currentState.Enter(this);
//////    }

//////    //public void MoveTo(Vector3 destination)
//////    //{
//////    //    if (runtimeData.moveSpeed <= 0f)
//////    //        return;

//////    //    transform.position = Vector3.MoveTowards(
//////    //        transform.position,
//////    //        destination,
//////    //        runtimeData.moveSpeed * Time.deltaTime
//////    //    );
//////    //}
//////    public void MoveTo(Vector3 destination)
//////    {
//////        if (runtimeData.moveSpeed <= 0f)
//////            return;

//////        Vector3 current = transform.position;
//////        Vector3 next = Vector3.MoveTowards(
//////            current,
//////            destination,
//////            runtimeData.moveSpeed * Time.deltaTime
//////        );

//////        //// ★ 移動前に障害物判定
//////        //if (AIManager.Instance != null && AIManager.Instance.HasObstacleBetween(current, next))
//////        //{
//////        //    Debug.Log($"[{name}] 進行方向に障害物あり → {(runtimeData.aiType == AIType.DStar ? "即リルート" : "停止")}");

//////        //    if (runtimeData.aiType == AIType.DStar)
//////        //    {
//////        //        RecalculatePath(); // D*なら即リルート
//////        //    }
//////        //    else
//////        //    {
//////        //        StopMovement();    // A*なら停止（次の再探索待ち）
//////        //    }
//////        //    return;
//////        //}
//////        // ★ 移動前に障害物判定
//////        if (AIManager.Instance != null && AIManager.Instance.HasObstacleBetween(current, next))
//////        {
//////            Debug.Log($"[{name}] 進行方向に障害物あり → 経路再計算開始 ({runtimeData.aiType})");
//////            //RecalculatePath(); // A*・D*どちらも再計算
//////            return;
//////        }

//////        // ★ここで保存
//////        nextPosition = next;

//////        transform.position = next;
//////    }


//////    public void Attack(AgentController target)
//////    {
//////        if (target == null) return;

//////        var health = target.GetComponent<AgentHealth>();
//////        if (health != null)
//////        {
//////            health.TakeDamage(runtimeData.attackPower, gameObject.name);
//////            Debug.Log($"[{name}] が [{target.name}] に {runtimeData.attackPower} ダメージを与えた！");
//////        }
//////    }

//////    public void SetGoal(Transform goalTransform) => goal = goalTransform;

//////    public Vector3 GetGoalPosition()
//////    {
//////        if (goal == null)
//////        {
//////            Debug.LogWarning($"[{name}] goal が未設定です。");
//////            //return transform.position;
//////            return Vector3.positiveInfinity; // ← 無効座標を返す
//////        }
//////        return goal.position;
//////    }

//////    public void SetPath(Vector3[] path)
//////    {
//////        //currentPath = path;
//////        //pathIndex = 0;

//////        currentPath = path;
//////        pathIndex = 0;

//////        // A* のとき、初回だけ保存（以後は上書きしない）
//////        var d = GetData();
//////        if (d != null && d.aiType == AIType.AStar && initialAStarPath == null && path != null && path.Length >= 2)
//////        {
//////            initialAStarPath = (Vector3[])path.Clone();
//////        }
//////    }

//////    // ===========================================================
//////    //  MovingWall 通知 → D*用の即時リルート処理
//////    // ===========================================================
//////    public void OnDynamicMapChanged(Vector3 changedPos, bool isBlocked)
//////    {
//////        if (runtimeData.aiType != AIType.DStar) return;
//////        if (currentPath == null || currentPath.Length == 0) return;

//////        foreach (var p in currentPath)
//////        {
//////            if (Vector3.Distance(p, changedPos) < 1.0f)
//////            {
//////                Debug.Log($"[D*:{name}] 動的障害物に反応 → 経路再計算開始");
//////                RecalculatePath();
//////                return;
//////            }
//////        }
//////    }

//////    /// <summary>
//////    /// 現在位置から目標への経路を再計算
//////    /// </summary>
//////    public void RecalculatePath()
//////    {
//////        if (aiManager == null)
//////        {
//////            aiManager = FindObjectOfType<AIManager>();
//////            if (aiManager == null)
//////            {
//////                Debug.LogError("[AgentController] AIManager が見つからないため経路再計算できません。");
//////                return;
//////            }
//////        }

//////        Vector3 start = transform.position;
//////        Vector3 goalPos = GetGoalPosition();
//////        var path = aiManager.GetPath(start, goalPos, runtimeData.aiType);
//////        if (path == null || path.Length == 0)
//////        {
//////            Debug.LogWarning($"[AgentController:{name}] 経路が取得できませんでした。");
//////            return;
//////        }
//////        //var path = aiManager.GetPath(start, goalPos, runtimeData.aiType);
//////        //if (path == null || path.Length <= 1)
//////        //{
//////        //    Debug.LogWarning($"[AgentController:{name}] 経路が短すぎるため再試行");
//////        //    StartCoroutine(RetryRepath()); // 0.5秒後などに再探索
//////        //    return;
//////        //}

//////        SetPath(path);
//////        needReplan = false;
//////        reachedGoal = false;
//////        Debug.Log($"[AgentController:{name}] RecalculatePath → {runtimeData.aiType}, nodes={path.Length}");
//////    }

//////    //private IEnumerator RetryRepath()
//////    //{
//////    //    yield return new WaitForSeconds(0.5f);
//////    //    RecalculatePath();
//////    //}

//////    public AgentData GetData() => runtimeData;

//////    public void StopMovement()
//////    {
//////        runtimeData.moveSpeed = 0f;
//////        Debug.Log($"[{name}] StopMovement: speed → 0");
//////    }

//////    public void RestoreDefaultSpeed()
//////    {
//////        runtimeData.moveSpeed = defaultMoveSpeed;
//////        Debug.Log($"[{name}] RestoreDefaultSpeed: speed → {runtimeData.moveSpeed}");
//////    }

//////    //private void OnDrawGizmos()
//////    //{
//////    //    // --- 再生中 ---
//////    //    if (Application.isPlaying)
//////    //    {
//////    //        if (currentPath != null && currentPath.Length >= 2)
//////    //        {
//////    //            var data = GetData();
//////    //            if (data != null)
//////    //            {
//////    //                Gizmos.color = (data.aiType == AIType.DStar) ? Color.cyan : Color.yellow;

//////    //                for (int i = 0; i < currentPath.Length - 1; i++)
//////    //                    Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);

//////    //                // ★停止後も残すために保存
//////    //                if (data.aiType == AIType.DStar)
//////    //                    lastDStarPath = (Vector3[])currentPath.Clone();
//////    //                else
//////    //                    lastAStarPath = (Vector3[])currentPath.Clone();
//////    //            }
//////    //        }
//////    //    }
//////    //    // --- 停止中（最後の経路を表示） ---
//////    //    else
//////    //    {
//////    //        if (lastAStarPath != null && lastAStarPath.Length >= 2)
//////    //        {
//////    //            Gizmos.color = Color.yellow;
//////    //            for (int i = 0; i < lastAStarPath.Length - 1; i++)
//////    //                Gizmos.DrawLine(lastAStarPath[i], lastAStarPath[i + 1]);
//////    //        }

//////    //        if (lastDStarPath != null && lastDStarPath.Length >= 2)
//////    //        {
//////    //            Gizmos.color = Color.cyan;
//////    //            for (int i = 0; i < lastDStarPath.Length - 1; i++)
//////    //                Gizmos.DrawLine(lastDStarPath[i], lastDStarPath[i + 1]);
//////    //        }
//////    //    }
//////    //}
//////    private void OnDrawGizmos()
//////    {
//////        // --- 実行中（Play中） ---
//////        if (Application.isPlaying)
//////        {
//////            var d = GetData();

//////            // ★ A* は「初回経路」を常に表示
//////            if (d != null && d.aiType == AIType.AStar && initialAStarPath != null && initialAStarPath.Length >= 2)
//////            {
//////                Gizmos.color = Color.yellow;
//////                for (int i = 0; i < initialAStarPath.Length - 1; i++)
//////                    Gizmos.DrawLine(initialAStarPath[i], initialAStarPath[i + 1]);
//////            }

//////            // 既存の「現在の経路」描画（D*やNavMeshの可視化用にそのまま残す）
//////            if (currentPath != null && currentPath.Length >= 2)
//////            {
//////                if (d != null && d.aiType == AIType.DStar) Gizmos.color = Color.cyan;
//////                else Gizmos.color = Color.yellow;

//////                for (int i = 0; i < currentPath.Length - 1; i++)
//////                    Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
//////            }
//////            return;
//////        }

//////        // --- 停止中（エディタ停止時は最後の経路を表示：既存処理を維持） ---
//////        if (lastAStarPath != null && lastAStarPath.Length >= 2)
//////        {
//////            Gizmos.color = Color.yellow;
//////            for (int i = 0; i < lastAStarPath.Length - 1; i++)
//////                Gizmos.DrawLine(lastAStarPath[i], lastAStarPath[i + 1]);
//////        }
//////        if (lastDStarPath != null && lastDStarPath.Length >= 2)
//////        {
//////            Gizmos.color = Color.cyan;
//////            for (int i = 0; i < lastDStarPath.Length - 1; i++)
//////                Gizmos.DrawLine(lastDStarPath[i], lastDStarPath[i + 1]);
//////        }
//////    }

//////    public bool HasPath()
//////    {
//////        return currentPath != null && currentPath.Length > 0;
//////    }
//////}


////////using UnityEngine;
////////using PathIntelligence;

////////public class AgentController : MonoBehaviour
////////{
////////    [SerializeField] private AgentData data;      // 元データ（共通アセット）
////////    private AgentData runtimeData;                // 実行時専用のコピー（安全に変更可能）

////////    private AgentBaseState currentState;

////////    private IdleState idleState = new IdleState();
////////    private SearchState searchState = new SearchState();
////////    private MoveState moveState = new MoveState();
////////    private AvoidState avoidState = new AvoidState();
////////    private GoalState goalState = new GoalState();

////////    private Transform goal;

////////    [HideInInspector] public Vector3[] currentPath;
////////    [HideInInspector] public int pathIndex = 0;
////////    [HideInInspector] public bool needReplan = false;
////////    [HideInInspector] public bool reachedGoal = false;

////////    private float defaultMoveSpeed; // 速度の初期値を保持
////////    private AIManager aiManager;    // 経路探索の窓口

////////    private void Start()
////////    {
////////        // ScriptableObjectを個体ごとにコピーして使用（安全）
////////        runtimeData = Instantiate(data);
////////        defaultMoveSpeed = runtimeData.moveSpeed; // 初期速度を記録

////////        // AIManager をキャッシュ
////////        aiManager = FindObjectOfType<AIManager>();
////////        if (aiManager == null)
////////        {
////////            Debug.LogError("[AgentController] AIManager がシーンに存在しません。");
////////        }

////////        ChangeState(idleState);
////////    }

////////    private void Update()
////////    {
////////        currentState?.Execute(this);
////////    }

////////    public void ChangeState(AgentBaseState newState)
////////    {
////////        currentState?.Exit(this);
////////        currentState = newState;
////////        currentState.Enter(this);
////////    }

////////    public void MoveTo(Vector3 destination)
////////    {
////////        // 攻撃中(speed==0)なら停止
////////        if (runtimeData.moveSpeed <= 0f)
////////            return;

////////        transform.position = Vector3.MoveTowards(
////////            transform.position,
////////            destination,
////////            runtimeData.moveSpeed * Time.deltaTime
////////        );
////////    }

////////    public void Attack(AgentController target)
////////    {
////////        if (target == null) return;

////////        var health = target.GetComponent<AgentHealth>();
////////        if (health != null)
////////        {
////////            health.TakeDamage(runtimeData.attackPower, gameObject.name);
////////            Debug.Log($"[{name}] が [{target.name}] に {runtimeData.attackPower} ダメージを与えた！");
////////        }
////////    }

////////    public void SetGoal(Transform goalTransform) => goal = goalTransform;

////////    public Vector3 GetGoalPosition()
////////    {
////////        if (goal == null)
////////        {
////////            Debug.LogWarning($"[{name}] goal が未設定です。");
////////            return transform.position; // 自分の位置を返しておく
////////        }
////////        return goal.position;
////////    }

////////    public void SetPath(Vector3[] path)
////////    {
////////        currentPath = path;
////////        pathIndex = 0;
////////    }

////////    /// <summary>
////////    /// AgentData.aiType に基づいて A* / D* を切り替え、現在地→目標への経路を再計算します。
////////    /// </summary>
////////    public void RecalculatePath()
////////    {
////////        if (aiManager == null)
////////        {
////////            aiManager = FindObjectOfType<AIManager>();
////////            if (aiManager == null)
////////            {
////////                Debug.LogError("[AgentController] AIManager が見つからないため経路再計算できません。");
////////                return;
////////            }
////////        }

////////        Vector3 start = transform.position;
////////        Vector3 goalPos = GetGoalPosition();
////////        var path = aiManager.GetPath(start, goalPos, runtimeData.aiType); // ★ aiType を渡す
////////        if (path == null || path.Length == 0)
////////        {
////////            Debug.LogWarning($"[AgentController:{name}] 経路が取得できませんでした。");
////////            return;
////////        }

////////        SetPath(path);
////////        needReplan = false;
////////        reachedGoal = false;
////////        Debug.Log($"[AgentController:{name}] RecalculatePath → {runtimeData.aiType}, nodes={path.Length}");
////////    }

////////    // ===== Getter / Setter =====
////////    public AgentData GetData() => runtimeData;

////////    // 攻撃時に速度を停止／復帰させるための関数群
////////    public void StopMovement()
////////    {
////////        runtimeData.moveSpeed = 0f;
////////        Debug.Log($"[{name}] StopMovement: speed → 0");
////////    }

////////    public void RestoreDefaultSpeed()
////////    {
////////        runtimeData.moveSpeed = defaultMoveSpeed;
////////        Debug.Log($"[{name}] RestoreDefaultSpeed: speed → {runtimeData.moveSpeed}");
////////    }
////////}


//////////using UnityEngine;
//////////using PathIntelligence;

//////////public class AgentController : MonoBehaviour
//////////{
//////////    [SerializeField] private AgentData data;      // 元データ（共通アセット）
//////////    private AgentData runtimeData;                // 実行時専用のコピー（安全に変更可能）

//////////    private AgentBaseState currentState;

//////////    private IdleState idleState = new IdleState();
//////////    private SearchState searchState = new SearchState();
//////////    private MoveState moveState = new MoveState();
//////////    private AvoidState avoidState = new AvoidState();
//////////    private GoalState goalState = new GoalState();

//////////    private Transform goal;
//////////    [HideInInspector] public Vector3[] currentPath;
//////////    [HideInInspector] public int pathIndex = 0;
//////////    [HideInInspector] public bool needReplan = false;
//////////    [HideInInspector] public bool reachedGoal = false;

//////////    private float defaultMoveSpeed; // 速度の初期値を保持

//////////    private void Start()
//////////    {
//////////        // ScriptableObjectを個体ごとにコピーして使用（安全）
//////////        runtimeData = Instantiate(data);
//////////        defaultMoveSpeed = runtimeData.moveSpeed; // 初期速度を記録
//////////        ChangeState(idleState);
//////////    }

//////////    private void Update()
//////////    {
//////////        currentState?.Execute(this);
//////////    }

//////////    public void ChangeState(AgentBaseState newState)
//////////    {
//////////        currentState?.Exit(this);
//////////        currentState = newState;
//////////        currentState.Enter(this);
//////////    }

//////////    public void MoveTo(Vector3 destination)
//////////    {
//////////        // 攻撃中(speed==0)なら停止
//////////        if (runtimeData.moveSpeed <= 0f)
//////////            return;

//////////        transform.position = Vector3.MoveTowards(
//////////            transform.position,
//////////            destination,
//////////            runtimeData.moveSpeed * Time.deltaTime
//////////        );
//////////    }

//////////    public void Attack(AgentController target)
//////////    {
//////////        if (target == null) return;

//////////        var health = target.GetComponent<AgentHealth>();
//////////        if (health != null)
//////////        {
//////////            health.TakeDamage(runtimeData.attackPower, gameObject.name);
//////////            Debug.Log($"[{name}] が [{target.name}] に {runtimeData.attackPower} ダメージを与えた！");
//////////        }
//////////    }

//////////    public void SetGoal(Transform goalTransform) => goal = goalTransform;

//////////    public Vector3 GetGoalPosition()
//////////    {
//////////        if (goal == null)
//////////        {
//////////            Debug.LogWarning($"[{name}] goal が未設定です。");
//////////            return transform.position; // 自分の位置を返しておく
//////////        }
//////////        return goal.position;
//////////    }

//////////    public void SetPath(Vector3[] path)
//////////    {
//////////        currentPath = path;
//////////        pathIndex = 0;
//////////    }

//////////    // ===== Getter / Setter =====
//////////    public AgentData GetData() => runtimeData;

//////////    // 攻撃時に速度を停止／復帰させるための関数群
//////////    public void StopMovement()
//////////    {
//////////        runtimeData.moveSpeed = 0f;
//////////        Debug.Log($"[{name}] StopMovement: speed → 0");
//////////    }

//////////    public void RestoreDefaultSpeed()
//////////    {
//////////        runtimeData.moveSpeed = defaultMoveSpeed;
//////////        Debug.Log($"[{name}] RestoreDefaultSpeed: speed → {runtimeData.moveSpeed}");
//////////    }
//////////}