using UnityEngine;

public class GoalState : AgentBaseState
{
    public override void Enter(AgentController agent)
    {
        Debug.Log($"[{agent.name}] GoalState: ƒS[ƒ‹“’B");
        agent.reachedGoal = true;
        GameManager.Instance.ReportAgentGoal(agent);
    }

    public override void Execute(AgentController agent) { }
    public override void Exit(AgentController agent) { }
}
