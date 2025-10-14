using UnityEngine;
using PathIntelligence; // AgentHealth参照用

public class AllyController : MonoBehaviour
{
    [SerializeField] private AllyData allydata;

    private AllyBaseState currentState;
    private AllyIdleState idleState = new AllyIdleState();
    private AllySearchState searchState = new AllySearchState();
    private AllyMoveState moveState = new AllyMoveState();
    private AllyAttackState attackState = new AllyAttackState();

    [HideInInspector] public AgentController targetEnemy;
    [HideInInspector] public Vector3[] currentPath;
    [HideInInspector] public int pathIndex = 0;
    [HideInInspector] public bool needReplan = false;

    private void Start()
    {
        ChangeState(idleState);
    }

    private void Update()
    {
        currentState?.Execute(this);
    }

    public void ChangeState(AllyBaseState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }

    public AllyData GetData() => allydata;

    public void SetPath(Vector3[] path)
    {
        currentPath = path;
        pathIndex = 0;
    }

    public void Attack(AgentController enemy)
    {
        var health = enemy.GetComponent<AgentHealth>();
        if (health != null)
        {
            health.TakeDamage(allydata.attackPower, gameObject.name);
            Debug.Log($"[{name}] が [{enemy.name}] に {allydata.attackPower} ダメージ！");
        }
    }
}
