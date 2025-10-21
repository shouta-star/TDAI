using UnityEngine;

public class IdleState : AgentBaseState
{
    public override void Enter(AgentController agent)
    {
        //LogAgentStatus(agent, "Idling");

        //Debug.Log($"[{agent.name}] IdleState: äJén");
    }

    public override void Execute(AgentController agent)
    {
        LogAgentStatus(agent, "Idling");
        agent.ChangeState(new SearchState());
    }

    public override void Exit(AgentController agent)
    {
        //Debug.Log($"[{agent.name}] IdleState: èIóπ");
    }
}
