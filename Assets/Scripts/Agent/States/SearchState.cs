using UnityEngine;

public class SearchState : AgentBaseState
{
    public override void Enter(AgentController agent)
    {
        Debug.Log($"[{agent.name}] SearchState: 経路探索開始");

        //AIManager aiManager = GameObject.FindObjectOfType<AIManager>();
        AIManager aiManager = Object.FindFirstObjectByType<AIManager>();

        Vector3 start = agent.transform.position;
        Vector3 goal = agent.GetGoalPosition();

        Vector3[] path = aiManager.GetPath(start, goal);
        agent.SetPath(path);

        agent.ChangeState(new MoveState());
    }

    public override void Execute(AgentController agent) { }

    public override void Exit(AgentController agent)
    {
        Debug.Log($"[{agent.name}] SearchState: 終了");
    }
}
