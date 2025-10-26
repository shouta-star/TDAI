using UnityEngine;

public class GoalState : AgentBaseState
{
    //public override void Enter(AgentController agent)
    //{
    //    Debug.Log($"[Goal:Enter] {agent.name} self={agent.transform.position} goal={agent.GetGoalPosition()} dist={Vector3.Distance(agent.transform.position, agent.GetGoalPosition()):F3}");

    //    LogAgentStatus(agent, "Goal");
    //    //Debug.Log($"[{agent.name}] GoalState: ゴール到達");
    //    agent.reachedGoal = true;
    //    //GameManager.Instance.ReportAgentGoal(agent);
    //    GameManager.Instance.ReportGoalReached(agent.GetCurrentGoalTransform(), agent);
    //}
    public override void Enter(AgentController agent)
    {
        var goal = agent.EnsureCurrentGoal();
        if (goal == null)
        {
            Debug.LogWarning($"[GoalState] {agent.name} のゴールが未設定のままです。");
            return;
        }

        Debug.Log($"[GoalState] {agent.name} が {goal.name} に到達しました。");
        GameManager.Instance.ReportGoalReached(goal, agent);

        // 次のゴールがあるなら再設定
        agent.SelectNextGoal();
        if (agent.GetCurrentGoalTransform() != null)
        {
            agent.RecalculatePath();
            agent.ChangeState(new MoveState());
        }
        else
        {
            agent.ChangeState(new IdleState());
            Debug.Log($"[GoalState] {agent.name} すべてのGoal到達 → 停止");
        }
    }


    public override void Execute(AgentController agent) { }
    public override void Exit(AgentController agent) { }
}
