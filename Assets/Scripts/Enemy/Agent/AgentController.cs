using UnityEngine;
using PathIntelligence;

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

    private void Start()
    {
        runtimeData = Instantiate(data);
        defaultMoveSpeed = runtimeData.moveSpeed;

        aiManager = FindObjectOfType<AIManager>();
        if (aiManager == null)
            Debug.LogError("[AgentController] AIManager がシーンに存在しません。");

        ChangeState(idleState);
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

    //public void MoveTo(Vector3 destination)
    //{
    //    if (runtimeData.moveSpeed <= 0f)
    //        return;

    //    transform.position = Vector3.MoveTowards(
    //        transform.position,
    //        destination,
    //        runtimeData.moveSpeed * Time.deltaTime
    //    );
    //}
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

        // ★ 移動前に障害物判定
        if (AIManager.Instance != null && AIManager.Instance.HasObstacleBetween(current, next))
        {
            Debug.Log($"[{name}] 進行方向に障害物あり → {(runtimeData.aiType == AIType.DStar ? "即リルート" : "停止")}");

            if (runtimeData.aiType == AIType.DStar)
            {
                RecalculatePath(); // D*なら即リルート
            }
            else
            {
                StopMovement();    // A*なら停止（次の再探索待ち）
            }
            return;
        }

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

    public void SetGoal(Transform goalTransform) => goal = goalTransform;

    public Vector3 GetGoalPosition()
    {
        if (goal == null)
        {
            Debug.LogWarning($"[{name}] goal が未設定です。");
            return transform.position;
        }
        return goal.position;
    }

    public void SetPath(Vector3[] path)
    {
        currentPath = path;
        pathIndex = 0;
    }

    // ===========================================================
    //  MovingWall 通知 → D*用の即時リルート処理
    // ===========================================================
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

    /// <summary>
    /// 現在位置から目標への経路を再計算
    /// </summary>
    public void RecalculatePath()
    {
        if (aiManager == null)
        {
            aiManager = FindObjectOfType<AIManager>();
            if (aiManager == null)
            {
                Debug.LogError("[AgentController] AIManager が見つからないため経路再計算できません。");
                return;
            }
        }

        Vector3 start = transform.position;
        Vector3 goalPos = GetGoalPosition();
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
}


//using UnityEngine;
//using PathIntelligence;

//public class AgentController : MonoBehaviour
//{
//    [SerializeField] private AgentData data;      // 元データ（共通アセット）
//    private AgentData runtimeData;                // 実行時専用のコピー（安全に変更可能）

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

//    private float defaultMoveSpeed; // 速度の初期値を保持
//    private AIManager aiManager;    // 経路探索の窓口

//    private void Start()
//    {
//        // ScriptableObjectを個体ごとにコピーして使用（安全）
//        runtimeData = Instantiate(data);
//        defaultMoveSpeed = runtimeData.moveSpeed; // 初期速度を記録

//        // AIManager をキャッシュ
//        aiManager = FindObjectOfType<AIManager>();
//        if (aiManager == null)
//        {
//            Debug.LogError("[AgentController] AIManager がシーンに存在しません。");
//        }

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

//    public void MoveTo(Vector3 destination)
//    {
//        // 攻撃中(speed==0)なら停止
//        if (runtimeData.moveSpeed <= 0f)
//            return;

//        transform.position = Vector3.MoveTowards(
//            transform.position,
//            destination,
//            runtimeData.moveSpeed * Time.deltaTime
//        );
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
//            return transform.position; // 自分の位置を返しておく
//        }
//        return goal.position;
//    }

//    public void SetPath(Vector3[] path)
//    {
//        currentPath = path;
//        pathIndex = 0;
//    }

//    /// <summary>
//    /// AgentData.aiType に基づいて A* / D* を切り替え、現在地→目標への経路を再計算します。
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
//        var path = aiManager.GetPath(start, goalPos, runtimeData.aiType); // ★ aiType を渡す
//        if (path == null || path.Length == 0)
//        {
//            Debug.LogWarning($"[AgentController:{name}] 経路が取得できませんでした。");
//            return;
//        }

//        SetPath(path);
//        needReplan = false;
//        reachedGoal = false;
//        Debug.Log($"[AgentController:{name}] RecalculatePath → {runtimeData.aiType}, nodes={path.Length}");
//    }

//    // ===== Getter / Setter =====
//    public AgentData GetData() => runtimeData;

//    // 攻撃時に速度を停止／復帰させるための関数群
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

////    private void Start()
////    {
////        // ScriptableObjectを個体ごとにコピーして使用（安全）
////        runtimeData = Instantiate(data);
////        defaultMoveSpeed = runtimeData.moveSpeed; // 初期速度を記録
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