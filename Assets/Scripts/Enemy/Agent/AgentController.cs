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