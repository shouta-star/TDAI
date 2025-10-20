using UnityEngine;
using System.Collections;
using PathIntelligence;

[ExecuteAlways]
public class AgentController : MonoBehaviour
{
    [SerializeField] private AgentData data;
    private AgentData runtimeData;

    private AgentBaseState currentState;

    private IdleState idleState = new IdleState();
    private SearchState searchState = new SearchState();
    private MoveState moveState = new MoveState();
    private AvoidState avoidState = new AvoidState();
    private GoalState goalState = new GoalState();

    private Transform goal;

    [HideInInspector] public Vector3[] currentPath;
    [HideInInspector] public int pathIndex = 0;
    [HideInInspector] public bool needReplan = false;
    [HideInInspector] public bool reachedGoal = false;

    private float defaultMoveSpeed;
    private AIManager aiManager;

    private Vector3 nextPosition;
    public Vector3 GetNextPosition() => nextPosition;

    private Vector3[] initialAStarPath;

    private static Vector3[] lastAStarPath;
    private static Vector3[] lastDStarPath;

    private void Start()
    {
        // Goal検索（GameManagerが設定する前のフォールバック）
        if (goal == null)
        {
            GameObject goalObj = GameObject.FindWithTag("Goal");
            if (goalObj == null)
                goalObj = GameObject.Find("Goal");

            if (goalObj != null)
            {
                goal = goalObj.transform;
                Debug.Log($"[{name}] Goal自動検出: {goal.name}");
            }
            else
            {
                Debug.LogWarning($"[{name}] Goalオブジェクトが見つかりません。");
            }
        }

        nextPosition = transform.position;

        // データの初期化
        if (data == null)
        {
            Debug.LogError($"[{name}] AgentDataが未設定です！");
            return;
        }

        runtimeData = Instantiate(data);
        defaultMoveSpeed = runtimeData.moveSpeed;

        // AIManagerを取得
        aiManager = FindObjectOfType<AIManager>();
        if (aiManager == null)
        {
            Debug.LogError("[AgentController] AIManagerがシーンに存在しません。");
            return;
        }

        // IdleStateから開始（GameManagerが経路を設定する）
        ChangeState(idleState);

        Debug.Log($"[{name}] 初期化完了 - AI: {runtimeData.aiType}, Speed: {runtimeData.moveSpeed}");
    }

    private void Update()
    {
        currentState?.Execute(this);
    }

    public void ChangeState(AgentBaseState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }

    public void MoveTo(Vector3 destination)
    {
        if (runtimeData.moveSpeed <= 0f)
            return;

        Vector3 current = transform.position;
        Vector3 next = Vector3.MoveTowards(
            current,
            destination,
            runtimeData.moveSpeed * Time.deltaTime
        );

        // 障害物チェック
        if (AIManager.Instance != null && AIManager.Instance.HasObstacleBetween(current, next))
        {
            Debug.Log($"[{name}] 進行方向に障害物あり → 経路再計算開始 ({runtimeData.aiType})");
            return;
        }

        nextPosition = next;
        transform.position = next;
    }

    public void Attack(AgentController target)
    {
        if (target == null) return;

        var health = target.GetComponent<AgentHealth>();
        if (health != null)
        {
            health.TakeDamage(runtimeData.attackPower, gameObject.name);
            Debug.Log($"[{name}] が [{target.name}] に {runtimeData.attackPower} ダメージを与えた！");
        }
    }

    public void SetGoal(Transform goalTransform)
    {
        goal = goalTransform;
        //Debug.Log($"[{name}] Goal設定: {goal.name}");
        Debug.Log($"[Agent:{name}] Goal set → {goal.position}");
    }

    public Vector3 GetGoalPosition()
    {
        if (goal == null)
        {
            Debug.LogWarning($"[{name}] goalが未設定です。");
            return Vector3.positiveInfinity;
        }
        return goal.position;
    }

    public void SetPath(Vector3[] path)
    {
        if (path == null || path.Length == 0)
        {
            Debug.LogWarning($"[{name}] 空の経路が設定されました。");
            return;
        }

        currentPath = path;
        pathIndex = 0;

        // A*の場合、初回経路を保存
        var d = GetData();
        if (d != null && d.aiType == AIType.AStar && initialAStarPath == null && path.Length >= 2)
        {
            initialAStarPath = (Vector3[])path.Clone();
        }

        Debug.Log($"[{name}] 経路設定完了: {path.Length}ノード");
    }

    public void OnDynamicMapChanged(Vector3 changedPos, bool isBlocked)
    {
        if (runtimeData.aiType != AIType.DStar) return;
        if (currentPath == null || currentPath.Length == 0) return;

        foreach (var p in currentPath)
        {
            if (Vector3.Distance(p, changedPos) < 1.0f)
            {
                Debug.Log($"[D*:{name}] 動的障害物に反応 → 経路再計算開始");
                RecalculatePath();
                return;
            }
        }
    }

    public void RecalculatePath()
    {
        if (aiManager == null)
        {
            aiManager = FindObjectOfType<AIManager>();
            if (aiManager == null)
            {
                Debug.LogError("[AgentController] AIManagerが見つからないため経路再計算できません。");
                return;
            }
        }

        Vector3 start = transform.position;
        Vector3 goalPos = GetGoalPosition();

        if (goalPos == Vector3.positiveInfinity)
        {
            Debug.LogError($"[{name}] Goal未設定のため経路再計算できません。");
            return;
        }

        var path = aiManager.GetPath(start, goalPos, runtimeData.aiType);
        if (path == null || path.Length == 0)
        {
            Debug.LogWarning($"[AgentController:{name}] 経路が取得できませんでした。");
            return;
        }

        SetPath(path);
        needReplan = false;
        reachedGoal = false;
        Debug.Log($"[AgentController:{name}] RecalculatePath → {runtimeData.aiType}, nodes={path.Length}");
    }

    public AgentData GetData() => runtimeData;

    public void StopMovement()
    {
        runtimeData.moveSpeed = 0f;
        Debug.Log($"[{name}] StopMovement: speed → 0");
    }

    public void RestoreDefaultSpeed()
    {
        runtimeData.moveSpeed = defaultMoveSpeed;
        Debug.Log($"[{name}] RestoreDefaultSpeed: speed → {runtimeData.moveSpeed}");
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            var d = GetData();

            // A*は初回経路を常に表示
            if (d != null && d.aiType == AIType.AStar && initialAStarPath != null && initialAStarPath.Length >= 2)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < initialAStarPath.Length - 1; i++)
                    Gizmos.DrawLine(initialAStarPath[i], initialAStarPath[i + 1]);
            }

            // 現在の経路表示
            if (currentPath != null && currentPath.Length >= 2)
            {
                if (d != null && d.aiType == AIType.DStar)
                    Gizmos.color = Color.cyan;
                else
                    Gizmos.color = Color.yellow;

                for (int i = 0; i < currentPath.Length - 1; i++)
                    Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
            return;
        }

        // 停止中は最後の経路を表示
        if (lastAStarPath != null && lastAStarPath.Length >= 2)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < lastAStarPath.Length - 1; i++)
                Gizmos.DrawLine(lastAStarPath[i], lastAStarPath[i + 1]);
        }
        if (lastDStarPath != null && lastDStarPath.Length >= 2)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < lastDStarPath.Length - 1; i++)
                Gizmos.DrawLine(lastDStarPath[i], lastDStarPath[i + 1]);
        }
    }

    public bool HasPath()
    {
        return currentPath != null && currentPath.Length > 0;
    }
}

//using UnityEngine;
//using System.Collections;
//using PathIntelligence;

//[ExecuteAlways]
//public class AgentController : MonoBehaviour
//{
//    [SerializeField] private AgentData data;
//    private AgentData runtimeData;

//    private AgentBaseState currentState;

//    private IdleState idleState = new IdleState();
//    private SearchState searchState = new SearchState();
//    private MoveState moveState = new MoveState();
//    private AvoidState avoidState = new AvoidState();
//    private GoalState goalState = new GoalState();

//    private Transform goal;

//    [HideInInspector] public Vector3[] currentPath;
//    [HideInInspector] public int pathIndex = 0;
//    [HideInInspector] public bool needReplan = false;
//    [HideInInspector] public bool reachedGoal = false;

//    private float defaultMoveSpeed;
//    private AIManager aiManager;

//    private Vector3 nextPosition;
//    public Vector3 GetNextPosition() => nextPosition;

//    // AgentController 内のフィールド群に追加
//    private Vector3[] initialAStarPath;  // ← A*で最初に得た経路のスナップショット

//    // 経路のバックアップ（停止中表示用）
//    private static Vector3[] lastAStarPath;
//    private static Vector3[] lastDStarPath;

//    private void Start()
//    {
//        //return;

//        // すでに他スクリプトから設定済みでなければ、自動検索
//        if (goal == null)
//        {
//            GameObject goalObj = GameObject.FindWithTag("Goal");
//            if (goalObj == null)
//                goalObj = GameObject.Find("Goal");

//            if (goalObj != null)
//                goal = goalObj.transform;
//            else
//                Debug.LogWarning($"[{name}] Goal オブジェクトが見つかりません。");
//        }

//        nextPosition = transform.position;

//        runtimeData = Instantiate(data);
//        defaultMoveSpeed = runtimeData.moveSpeed;

//        aiManager = FindObjectOfType<AIManager>();
//        if (aiManager == null)
//            Debug.LogError("[AgentController] AIManager がシーンに存在しません。");

//        ChangeState(idleState);
//    }

//    private void Update()
//    {
//        currentState?.Execute(this);
//    }

//    public void ChangeState(AgentBaseState newState)
//    {
//        currentState?.Exit(this);
//        currentState = newState;
//        currentState.Enter(this);
//    }

//    //public void MoveTo(Vector3 destination)
//    //{
//    //    if (runtimeData.moveSpeed <= 0f)
//    //        return;

//    //    transform.position = Vector3.MoveTowards(
//    //        transform.position,
//    //        destination,
//    //        runtimeData.moveSpeed * Time.deltaTime
//    //    );
//    //}
//    public void MoveTo(Vector3 destination)
//    {
//        if (runtimeData.moveSpeed <= 0f)
//            return;

//        Vector3 current = transform.position;
//        Vector3 next = Vector3.MoveTowards(
//            current,
//            destination,
//            runtimeData.moveSpeed * Time.deltaTime
//        );

//        //// ★ 移動前に障害物判定
//        //if (AIManager.Instance != null && AIManager.Instance.HasObstacleBetween(current, next))
//        //{
//        //    Debug.Log($"[{name}] 進行方向に障害物あり → {(runtimeData.aiType == AIType.DStar ? "即リルート" : "停止")}");

//        //    if (runtimeData.aiType == AIType.DStar)
//        //    {
//        //        RecalculatePath(); // D*なら即リルート
//        //    }
//        //    else
//        //    {
//        //        StopMovement();    // A*なら停止（次の再探索待ち）
//        //    }
//        //    return;
//        //}
//        // ★ 移動前に障害物判定
//        if (AIManager.Instance != null && AIManager.Instance.HasObstacleBetween(current, next))
//        {
//            Debug.Log($"[{name}] 進行方向に障害物あり → 経路再計算開始 ({runtimeData.aiType})");
//            //RecalculatePath(); // A*・D*どちらも再計算
//            return;
//        }

//        // ★ここで保存
//        nextPosition = next;

//        transform.position = next;
//    }


//    public void Attack(AgentController target)
//    {
//        if (target == null) return;

//        var health = target.GetComponent<AgentHealth>();
//        if (health != null)
//        {
//            health.TakeDamage(runtimeData.attackPower, gameObject.name);
//            Debug.Log($"[{name}] が [{target.name}] に {runtimeData.attackPower} ダメージを与えた！");
//        }
//    }

//    public void SetGoal(Transform goalTransform) => goal = goalTransform;

//    public Vector3 GetGoalPosition()
//    {
//        if (goal == null)
//        {
//            Debug.LogWarning($"[{name}] goal が未設定です。");
//            //return transform.position;
//            return Vector3.positiveInfinity; // ← 無効座標を返す
//        }
//        return goal.position;
//    }

//    public void SetPath(Vector3[] path)
//    {
//        //currentPath = path;
//        //pathIndex = 0;

//        currentPath = path;
//        pathIndex = 0;

//        // A* のとき、初回だけ保存（以後は上書きしない）
//        var d = GetData();
//        if (d != null && d.aiType == AIType.AStar && initialAStarPath == null && path != null && path.Length >= 2)
//        {
//            initialAStarPath = (Vector3[])path.Clone();
//        }
//    }

//    // ===========================================================
//    //  MovingWall 通知 → D*用の即時リルート処理
//    // ===========================================================
//    public void OnDynamicMapChanged(Vector3 changedPos, bool isBlocked)
//    {
//        if (runtimeData.aiType != AIType.DStar) return;
//        if (currentPath == null || currentPath.Length == 0) return;

//        foreach (var p in currentPath)
//        {
//            if (Vector3.Distance(p, changedPos) < 1.0f)
//            {
//                Debug.Log($"[D*:{name}] 動的障害物に反応 → 経路再計算開始");
//                RecalculatePath();
//                return;
//            }
//        }
//    }

//    /// <summary>
//    /// 現在位置から目標への経路を再計算
//    /// </summary>
//    public void RecalculatePath()
//    {
//        if (aiManager == null)
//        {
//            aiManager = FindObjectOfType<AIManager>();
//            if (aiManager == null)
//            {
//                Debug.LogError("[AgentController] AIManager が見つからないため経路再計算できません。");
//                return;
//            }
//        }

//        Vector3 start = transform.position;
//        Vector3 goalPos = GetGoalPosition();
//        var path = aiManager.GetPath(start, goalPos, runtimeData.aiType);
//        if (path == null || path.Length == 0)
//        {
//            Debug.LogWarning($"[AgentController:{name}] 経路が取得できませんでした。");
//            return;
//        }
//        //var path = aiManager.GetPath(start, goalPos, runtimeData.aiType);
//        //if (path == null || path.Length <= 1)
//        //{
//        //    Debug.LogWarning($"[AgentController:{name}] 経路が短すぎるため再試行");
//        //    StartCoroutine(RetryRepath()); // 0.5秒後などに再探索
//        //    return;
//        //}

//        SetPath(path);
//        needReplan = false;
//        reachedGoal = false;
//        Debug.Log($"[AgentController:{name}] RecalculatePath → {runtimeData.aiType}, nodes={path.Length}");
//    }

//    //private IEnumerator RetryRepath()
//    //{
//    //    yield return new WaitForSeconds(0.5f);
//    //    RecalculatePath();
//    //}

//    public AgentData GetData() => runtimeData;

//    public void StopMovement()
//    {
//        runtimeData.moveSpeed = 0f;
//        Debug.Log($"[{name}] StopMovement: speed → 0");
//    }

//    public void RestoreDefaultSpeed()
//    {
//        runtimeData.moveSpeed = defaultMoveSpeed;
//        Debug.Log($"[{name}] RestoreDefaultSpeed: speed → {runtimeData.moveSpeed}");
//    }

//    //private void OnDrawGizmos()
//    //{
//    //    // --- 再生中 ---
//    //    if (Application.isPlaying)
//    //    {
//    //        if (currentPath != null && currentPath.Length >= 2)
//    //        {
//    //            var data = GetData();
//    //            if (data != null)
//    //            {
//    //                Gizmos.color = (data.aiType == AIType.DStar) ? Color.cyan : Color.yellow;

//    //                for (int i = 0; i < currentPath.Length - 1; i++)
//    //                    Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);

//    //                // ★停止後も残すために保存
//    //                if (data.aiType == AIType.DStar)
//    //                    lastDStarPath = (Vector3[])currentPath.Clone();
//    //                else
//    //                    lastAStarPath = (Vector3[])currentPath.Clone();
//    //            }
//    //        }
//    //    }
//    //    // --- 停止中（最後の経路を表示） ---
//    //    else
//    //    {
//    //        if (lastAStarPath != null && lastAStarPath.Length >= 2)
//    //        {
//    //            Gizmos.color = Color.yellow;
//    //            for (int i = 0; i < lastAStarPath.Length - 1; i++)
//    //                Gizmos.DrawLine(lastAStarPath[i], lastAStarPath[i + 1]);
//    //        }

//    //        if (lastDStarPath != null && lastDStarPath.Length >= 2)
//    //        {
//    //            Gizmos.color = Color.cyan;
//    //            for (int i = 0; i < lastDStarPath.Length - 1; i++)
//    //                Gizmos.DrawLine(lastDStarPath[i], lastDStarPath[i + 1]);
//    //        }
//    //    }
//    //}
//    private void OnDrawGizmos()
//    {
//        // --- 実行中（Play中） ---
//        if (Application.isPlaying)
//        {
//            var d = GetData();

//            // ★ A* は「初回経路」を常に表示
//            if (d != null && d.aiType == AIType.AStar && initialAStarPath != null && initialAStarPath.Length >= 2)
//            {
//                Gizmos.color = Color.yellow;
//                for (int i = 0; i < initialAStarPath.Length - 1; i++)
//                    Gizmos.DrawLine(initialAStarPath[i], initialAStarPath[i + 1]);
//            }

//            // 既存の「現在の経路」描画（D*やNavMeshの可視化用にそのまま残す）
//            if (currentPath != null && currentPath.Length >= 2)
//            {
//                if (d != null && d.aiType == AIType.DStar) Gizmos.color = Color.cyan;
//                else Gizmos.color = Color.yellow;

//                for (int i = 0; i < currentPath.Length - 1; i++)
//                    Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
//            }
//            return;
//        }

//        // --- 停止中（エディタ停止時は最後の経路を表示：既存処理を維持） ---
//        if (lastAStarPath != null && lastAStarPath.Length >= 2)
//        {
//            Gizmos.color = Color.yellow;
//            for (int i = 0; i < lastAStarPath.Length - 1; i++)
//                Gizmos.DrawLine(lastAStarPath[i], lastAStarPath[i + 1]);
//        }
//        if (lastDStarPath != null && lastDStarPath.Length >= 2)
//        {
//            Gizmos.color = Color.cyan;
//            for (int i = 0; i < lastDStarPath.Length - 1; i++)
//                Gizmos.DrawLine(lastDStarPath[i], lastDStarPath[i + 1]);
//        }
//    }

//    public bool HasPath()
//    {
//        return currentPath != null && currentPath.Length > 0;
//    }
//}


////using UnityEngine;
////using PathIntelligence;

////public class AgentController : MonoBehaviour
////{
////    [SerializeField] private AgentData data;      // 元データ（共通アセット）
////    private AgentData runtimeData;                // 実行時専用のコピー（安全に変更可能）

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

////    private float defaultMoveSpeed; // 速度の初期値を保持
////    private AIManager aiManager;    // 経路探索の窓口

////    private void Start()
////    {
////        // ScriptableObjectを個体ごとにコピーして使用（安全）
////        runtimeData = Instantiate(data);
////        defaultMoveSpeed = runtimeData.moveSpeed; // 初期速度を記録

////        // AIManager をキャッシュ
////        aiManager = FindObjectOfType<AIManager>();
////        if (aiManager == null)
////        {
////            Debug.LogError("[AgentController] AIManager がシーンに存在しません。");
////        }

////        ChangeState(idleState);
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
////        // 攻撃中(speed==0)なら停止
////        if (runtimeData.moveSpeed <= 0f)
////            return;

////        transform.position = Vector3.MoveTowards(
////            transform.position,
////            destination,
////            runtimeData.moveSpeed * Time.deltaTime
////        );
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

////    public void SetGoal(Transform goalTransform) => goal = goalTransform;

////    public Vector3 GetGoalPosition()
////    {
////        if (goal == null)
////        {
////            Debug.LogWarning($"[{name}] goal が未設定です。");
////            return transform.position; // 自分の位置を返しておく
////        }
////        return goal.position;
////    }

////    public void SetPath(Vector3[] path)
////    {
////        currentPath = path;
////        pathIndex = 0;
////    }

////    /// <summary>
////    /// AgentData.aiType に基づいて A* / D* を切り替え、現在地→目標への経路を再計算します。
////    /// </summary>
////    public void RecalculatePath()
////    {
////        if (aiManager == null)
////        {
////            aiManager = FindObjectOfType<AIManager>();
////            if (aiManager == null)
////            {
////                Debug.LogError("[AgentController] AIManager が見つからないため経路再計算できません。");
////                return;
////            }
////        }

////        Vector3 start = transform.position;
////        Vector3 goalPos = GetGoalPosition();
////        var path = aiManager.GetPath(start, goalPos, runtimeData.aiType); // ★ aiType を渡す
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

////    // ===== Getter / Setter =====
////    public AgentData GetData() => runtimeData;

////    // 攻撃時に速度を停止／復帰させるための関数群
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
////}


//////using UnityEngine;
//////using PathIntelligence;

//////public class AgentController : MonoBehaviour
//////{
//////    [SerializeField] private AgentData data;      // 元データ（共通アセット）
//////    private AgentData runtimeData;                // 実行時専用のコピー（安全に変更可能）

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

//////    private float defaultMoveSpeed; // 速度の初期値を保持

//////    private void Start()
//////    {
//////        // ScriptableObjectを個体ごとにコピーして使用（安全）
//////        runtimeData = Instantiate(data);
//////        defaultMoveSpeed = runtimeData.moveSpeed; // 初期速度を記録
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

//////    public void MoveTo(Vector3 destination)
//////    {
//////        // 攻撃中(speed==0)なら停止
//////        if (runtimeData.moveSpeed <= 0f)
//////            return;

//////        transform.position = Vector3.MoveTowards(
//////            transform.position,
//////            destination,
//////            runtimeData.moveSpeed * Time.deltaTime
//////        );
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
//////            return transform.position; // 自分の位置を返しておく
//////        }
//////        return goal.position;
//////    }

//////    public void SetPath(Vector3[] path)
//////    {
//////        currentPath = path;
//////        pathIndex = 0;
//////    }

//////    // ===== Getter / Setter =====
//////    public AgentData GetData() => runtimeData;

//////    // 攻撃時に速度を停止／復帰させるための関数群
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
//////}