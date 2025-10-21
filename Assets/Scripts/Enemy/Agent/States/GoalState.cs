using UnityEngine;

public class GoalState : AgentBaseState
{
    public override void Enter(AgentController agent)
    {
        Debug.Log($"[Goal:Enter] {agent.name} self={agent.transform.position} goal={agent.GetGoalPosition()} dist={Vector3.Distance(agent.transform.position, agent.GetGoalPosition()):F3}");

        LogAgentStatus(agent, "GoalReached");
        //Debug.Log($"[{agent.name}] GoalState: ƒS[ƒ‹“’B");
        agent.reachedGoal = true;
        GameManager.Instance.ReportAgentGoal(agent);
    }

    public override void Execute(AgentController agent) { }
    public override void Exit(AgentController agent) { }
}
