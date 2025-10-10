using UnityEngine;

public class IdleState : AgentBaseState
{
    public override void Enter(AgentController agent)
    {
        Debug.Log($"[{agent.name}] IdleState: äJén");
    }

    public override void Execute(AgentController agent)
    {
        agent.ChangeState(new SearchState());
    }

    public override void Exit(AgentController agent)
    {
        Debug.Log($"[{agent.name}] IdleState: èIóπ");
    }
}
