using UnityEngine;

public class MoveState : AgentBaseState
{
    public override void Enter(AgentController agent)
    {
        Debug.Log($"[{agent.name}] MoveState: 開始");
    }

    public override void Execute(AgentController agent)
    {
        // ★ここで再探索要求に対応
        if (agent.needReplan)
        {
            agent.needReplan = false;
            agent.ChangeState(new SearchState());
            return;
        }

        if (agent.currentPath == null || agent.currentPath.Length == 0)
        {
            agent.ChangeState(new SearchState());
            return;
        }

        Vector3 target = agent.currentPath[agent.pathIndex];
        agent.MoveTo(target);

        if (Vector3.Distance(agent.transform.position, target) < 0.1f)
        {
            agent.pathIndex++;
            if (agent.pathIndex >= agent.currentPath.Length)
            {
                agent.ChangeState(new GoalState());
            }
        }

        HeatmapManager.Instance.RecordPosition(agent.transform.position);
    }

    public override void Exit(AgentController agent)
    {
        Debug.Log($"[{agent.name}] MoveState: 終了");
    }
}
