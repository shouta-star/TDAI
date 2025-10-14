using UnityEngine;

public class AgentController : MonoBehaviour
{
    [SerializeField] private AgentData data;
    private AgentBaseState currentState;

    private IdleState idleState = new IdleState();
    private SearchState searchState = new SearchState();
    private MoveState moveState = new MoveState();
    private AvoidState avoidState = new AvoidState();
    private GoalState goalState = new GoalState();

    private Transform goal;
    [HideInInspector] public Vector3[] currentPath;
    [HideInInspector] public int pathIndex = 0;
    [HideInInspector] public bool reachedGoal = false;

    // MapManager削除に伴い再探索判定を簡略化
    [HideInInspector] public bool needReplan = false;

    private void Awake()
    {
        // MapManagerは廃止
    }

    private void Start()
    {
        ChangeState(idleState);
    }

    private void Update()
    {
        if (currentState != null)
            currentState.Execute(this);
    }

    public void ChangeState(AgentBaseState newState)
    {
        if (currentState != null) currentState.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }

    public void MoveTo(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, data.moveSpeed * Time.deltaTime);
    }

    public void SetGoal(Transform goalTransform)
    {
        goal = goalTransform;
    }

    public Vector3 GetGoalPosition() => goal.position;

    public void SetPath(Vector3[] path)
    {
        currentPath = path;
        pathIndex = 0;
    }

    public AgentData GetData() => data;

    // MapManager依存のOnDestroy / OnTileChanged削除済み
}


//using UnityEngine;

//public class AgentController : MonoBehaviour
//{
//    [SerializeField] private AgentData data;
//    private AgentBaseState currentState;

//    private IdleState idleState = new IdleState();
//    private SearchState searchState = new SearchState();
//    private MoveState moveState = new MoveState();
//    private AvoidState avoidState = new AvoidState();
//    private GoalState goalState = new GoalState();

//    private Transform goal;
//    [HideInInspector] public Vector3[] currentPath;
//    [HideInInspector] public int pathIndex = 0;
//    [HideInInspector] public bool reachedGoal = false;

//    // ★追加：MapManager参照と再探索フラグ
//    private MapManager map;
//    [HideInInspector] public bool needReplan = false;

//    private void Awake()
//    {
//        // ★非推奨APIの置換
//        map = Object.FindFirstObjectByType<MapManager>();
//    }

//    private void Start()
//    {
//        ChangeState(idleState);

//        //var map = FindObjectOfType<MapManager>();
//        var map = Object.FindFirstObjectByType<MapManager>();
//        map.TileChanged += OnTileChanged;  // バッチTilesChangedでもOK
//    }

//    private void Update()
//    {
//        if (currentState != null)
//            currentState.Execute(this);
//    }

//    public void ChangeState(AgentBaseState newState)
//    {
//        if (currentState != null) currentState.Exit(this);
//        currentState = newState;
//        currentState.Enter(this);
//    }

//    public void MoveTo(Vector3 destination)
//    {
//        transform.position = Vector3.MoveTowards(transform.position, destination, data.moveSpeed * Time.deltaTime);
//    }

//    public void SetGoal(Transform goalTransform)
//    {
//        goal = goalTransform;
//    }

//    public Vector3 GetGoalPosition() => goal.position;

//    public void SetPath(Vector3[] path)
//    {
//        currentPath = path;
//        pathIndex = 0;
//    }

//    public AgentData GetData() => data;

//    private void OnDestroy()
//    {
//        //var map = FindObjectOfType<MapManager>();
//        var map = Object.FindFirstObjectByType<MapManager>();
//        if (map != null) map.TileChanged -= OnTileChanged;
//    }

//    // ★現在の経路上のセルが変更されたら、再探索を要求
//    private void OnTileChanged(MapManager.TileChangedArgs args)
//    {
//        if (currentPath == null || map == null) return;

//        for (int i = pathIndex; i < currentPath.Length; i++)
//        {
//            if (map.WorldToGrid(currentPath[i], out int x, out int z))
//            {
//                if (x == args.x && z == args.z)
//                {
//                    needReplan = true;
//                    break;
//                }
//            }
//        }
//    }
//}
