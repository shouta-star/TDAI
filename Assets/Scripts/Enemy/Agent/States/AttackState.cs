using UnityEngine;

public class AttackState : AgentBaseState
{
    private float attackTimer = 0f;
    private float attackInterval;

    public override void Enter(AgentController agent)
    {
        attackInterval = agent.GetData().attackInterval;
        Debug.Log($"[{agent.name}] AttackState: 開始");
    }

    public override void Execute(AgentController agent)
    {
        attackTimer -= Time.deltaTime;

        AgentController target = FindNearestEnemy(agent);
        if (target == null)
        {
            agent.ChangeState(new SearchState());
            return;
        }

        float dist = Vector3.Distance(agent.transform.position, target.transform.position);
        if (dist > agent.GetData().attackRange)
        {
            agent.ChangeState(new MoveState());
            return;
        }

        if (attackTimer <= 0f)
        {
            attackTimer = attackInterval;
            agent.Attack(target);
        }
    }

    private AgentController FindNearestEnemy(AgentController self)
    {
        // 仮実装：距離が最も近い他のAgentを敵とみなす
        AgentController[] all = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
        AgentController nearest = null;
        float minDist = float.MaxValue;

        foreach (var other in all)
        {
            if (other == self) continue;
            float d = Vector3.Distance(self.transform.position, other.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = other;
            }
        }
        return nearest;
    }

    public override void Exit(AgentController agent)
    {
        Debug.Log($"[{agent.name}] AttackState: 終了");
    }
}
