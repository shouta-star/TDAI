using UnityEngine;

public class AvoidState : AgentBaseState
{
    public override void Enter(AgentController agent)
    {
        Debug.Log($"[{agent.name}] AvoidState: ‰ñ”ðŠJŽn");
        agent.ChangeState(new SearchState());
    }

    public override void Execute(AgentController agent) 
    {
        LogAgentStatus(agent, "Avoiding");
    }
    public override void Exit(AgentController agent) { }
}
