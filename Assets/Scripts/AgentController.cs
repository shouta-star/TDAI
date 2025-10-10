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
}
