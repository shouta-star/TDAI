using UnityEngine;
using PathIntelligence;

public class AgentController : MonoBehaviour
{
    [SerializeField] private AgentData data;      // 元データ（共通アセット）
    private AgentData runtimeData;                // 実行時専用のコピー（安全に変更可能）

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

    private float defaultMoveSpeed; // 速度の初期値を保持

    private void Start()
    {
        // ScriptableObjectを個体ごとにコピーして使用（安全）
        runtimeData = Instantiate(data);
        defaultMoveSpeed = runtimeData.moveSpeed; // 初期速度を記録
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

    public void MoveTo(Vector3 destination)
    {
        // 攻撃中(speed==0)なら停止
        if (runtimeData.moveSpeed <= 0f)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            runtimeData.moveSpeed * Time.deltaTime
        );
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
            return transform.position; // 自分の位置を返しておく
        }
        return goal.position;
    }

    public void SetPath(Vector3[] path)
    {
        currentPath = path;
        pathIndex = 0;
    }

    // ===== Getter / Setter =====
    public AgentData GetData() => runtimeData;

    // 攻撃時に速度を停止／復帰させるための関数群
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

//    private void Start()
//    {
//        // ScriptableObjectを個体ごとにコピーして使用（安全）
//        runtimeData = Instantiate(data);
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
//    //public Vector3 GetGoalPosition() => goal.position;
//    public Vector3 GetGoalPosition()
//    {
//        if (goal == null)
//        {
//            Debug.LogWarning($"[{name}] goal が未設定です。");
//            return transform.position; // 自分の位置を返しておく
//        }
//        return goal.position;
//    }
//    public void SetPath(Vector3[] path) { currentPath = path; pathIndex = 0; }

//    // ===== Getter / Setter =====
//    public AgentData GetData() => runtimeData;

//    // 攻撃中などに速度を変える用
//    public void SetMoveSpeed(float value) => runtimeData.moveSpeed = value;
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

////    private void Start()
////    {
////        // ScriptableObjectを個体ごとにコピーして使用（安全）
////        runtimeData = Instantiate(data);
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
////    public Vector3 GetGoalPosition() => goal.position;
////    public void SetPath(Vector3[] path) { currentPath = path; pathIndex = 0; }

////    // ===== Getter / Setter =====
////    public AgentData GetData() => runtimeData;

////    // 攻撃中などに速度を変える用
////    public void SetMoveSpeed(float value) => runtimeData.moveSpeed = value;
////}


//////using UnityEngine;
//////using PathIntelligence;

//////public class AgentController : MonoBehaviour
//////{
//////    [SerializeField] private AgentData data;
//////    private AgentBaseState currentState;

//////    private IdleState idleState = new IdleState();
//////    private SearchState searchState = new SearchState();
//////    private MoveState moveState = new MoveState();
//////    private AvoidState avoidState = new AvoidState();
//////    private GoalState goalState = new GoalState();

//////    private Transform goal;
//////    [HideInInspector] public Vector3[] currentPath;
//////    [HideInInspector] public int pathIndex = 0;
//////    [HideInInspector] public bool reachedGoal = false;

//////    // MapManager削除に伴い再探索判定を簡略化
//////    [HideInInspector] public bool needReplan = false;

//////    //private float originalMoveSpeed; // 退避用
//////    //private float currentMoveSpeed;  // 実際の速度

//////    private void Awake()
//////    {
//////        // MapManagerは廃止
//////    }

//////    private void Start()
//////    {
//////        ChangeState(idleState);

//////        //originalMoveSpeed = data.moveSpeed;
//////        //currentMoveSpeed = originalMoveSpeed;
//////        //ChangeState(idleState);
//////    }

//////    private void Update()
//////    {
//////        if (currentState != null)
//////            currentState.Execute(this);
//////    }

//////    public void ChangeState(AgentBaseState newState)
//////    {
//////        if (currentState != null) currentState.Exit(this);
//////        currentState = newState;
//////        currentState.Enter(this);
//////    }

//////    public void MoveTo(Vector3 destination)
//////    {
//////        transform.position = Vector3.MoveTowards(transform.position, destination, data.moveSpeed * Time.deltaTime);

//////        //transform.position = Vector3.MoveTowards(
//////        //transform.position,
//////        //destination,
//////        //currentMoveSpeed * Time.deltaTime // ← data.moveSpeed ではなく currentMoveSpeed
//////        //);
//////    }

//////    public void Attack(AgentController target)
//////    {
//////        if (target == null) return;

//////        var health = target.GetComponent<AgentHealth>();
//////        if (health != null)
//////        {
//////            health.TakeDamage(data.attackPower, gameObject.name);
//////            Debug.Log($"[{name}] が [{target.name}] に {data.attackPower} ダメージを与えた！");
//////        }
//////    }

//////    public void SetGoal(Transform goalTransform)
//////    {
//////        goal = goalTransform;
//////    }

//////    public Vector3 GetGoalPosition() => goal.position;

//////    public void SetPath(Vector3[] path)
//////    {
//////        currentPath = path;
//////        pathIndex = 0;
//////    }

//////    //public void SetMoveSpeed(float value)
//////    //{
//////    //    currentMoveSpeed = value;
//////    //}

//////    //public void ResetMoveSpeed()
//////    //{
//////    //    currentMoveSpeed = originalMoveSpeed;
//////    //}


//////    public AgentData GetData() => data;

//////    // MapManager依存のOnDestroy / OnTileChanged削除済み
//////}


////////using UnityEngine;

////////public class AgentController : MonoBehaviour
////////{
////////    [SerializeField] private AgentData data;
////////    private AgentBaseState currentState;

////////    private IdleState idleState = new IdleState();
////////    private SearchState searchState = new SearchState();
////////    private MoveState moveState = new MoveState();
////////    private AvoidState avoidState = new AvoidState();
////////    private GoalState goalState = new GoalState();

////////    private Transform goal;
////////    [HideInInspector] public Vector3[] currentPath;
////////    [HideInInspector] public int pathIndex = 0;
////////    [HideInInspector] public bool reachedGoal = false;

////////    // ★追加：MapManager参照と再探索フラグ
////////    private MapManager map;
////////    [HideInInspector] public bool needReplan = false;

////////    private void Awake()
////////    {
////////        // ★非推奨APIの置換
////////        map = Object.FindFirstObjectByType<MapManager>();
////////    }

////////    private void Start()
////////    {
////////        ChangeState(idleState);

////////        //var map = FindObjectOfType<MapManager>();
////////        var map = Object.FindFirstObjectByType<MapManager>();
////////        map.TileChanged += OnTileChanged;  // バッチTilesChangedでもOK
////////    }

////////    private void Update()
////////    {
////////        if (currentState != null)
////////            currentState.Execute(this);
////////    }

////////    public void ChangeState(AgentBaseState newState)
////////    {
////////        if (currentState != null) currentState.Exit(this);
////////        currentState = newState;
////////        currentState.Enter(this);
////////    }

////////    public void MoveTo(Vector3 destination)
////////    {
////////        transform.position = Vector3.MoveTowards(transform.position, destination, data.moveSpeed * Time.deltaTime);
////////    }

////////    public void SetGoal(Transform goalTransform)
////////    {
////////        goal = goalTransform;
////////    }

////////    public Vector3 GetGoalPosition() => goal.position;

////////    public void SetPath(Vector3[] path)
////////    {
////////        currentPath = path;
////////        pathIndex = 0;
////////    }

////////    public AgentData GetData() => data;

////////    private void OnDestroy()
////////    {
////////        //var map = FindObjectOfType<MapManager>();
////////        var map = Object.FindFirstObjectByType<MapManager>();
////////        if (map != null) map.TileChanged -= OnTileChanged;
////////    }

////////    // ★現在の経路上のセルが変更されたら、再探索を要求
////////    private void OnTileChanged(MapManager.TileChangedArgs args)
////////    {
////////        if (currentPath == null || map == null) return;

////////        for (int i = pathIndex; i < currentPath.Length; i++)
////////        {
////////            if (map.WorldToGrid(currentPath[i], out int x, out int z))
////////            {
////////                if (x == args.x && z == args.z)
////////                {
////////                    needReplan = true;
////////                    break;
////////                }
////////            }
////////        }
////////    }
////////}
