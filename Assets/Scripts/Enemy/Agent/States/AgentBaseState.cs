using UnityEngine;

public abstract class AgentBaseState
{
    public abstract void Enter(AgentController agent);
    public abstract void Execute(AgentController agent);
    public abstract void Exit(AgentController agent);

    // ============================
    // 共通ログ出力関数
    // ============================
    protected void LogAgentStatus(AgentController agent, string action)
    {
        string stateName = GetType().Name;
        string targetName = "None";
        Vector3 targetPos = Vector3.zero;

        // --- 現在追っている敵や味方を参照 ---
        MonoBehaviour nearest = FindNearestEnemy(agent);
        if (nearest != null)
        {
            targetName = nearest.name;
            targetPos = ((Component)nearest).transform.position;
        }
        else if (agent.GetGoalPosition() != Vector3.zero)
        {
            targetName = "Goal";
            targetPos = agent.GetGoalPosition();
        }

        Vector3 self = agent.transform.position;

        Debug.Log(
            $"[AgentLog] {agent.name} | State={stateName} | Action={action} | " +
            $"Target={targetName} | TargetPos=({targetPos.x:F2},{targetPos.y:F2},{targetPos.z:F2}) | " +
            $"SelfPos=({self.x:F2},{self.y:F2},{self.z:F2})"
        );
    }

    // ============================
    // 共通ターゲット検索（Agent + Ally）
    // ============================
    protected MonoBehaviour FindNearestEnemy(AgentController self)
    {
        float bestDist = float.MaxValue;
        MonoBehaviour nearest = null;

        // --- Allyを敵として検索 ---
        AllyController[] allies = Object.FindObjectsByType<AllyController>(FindObjectsSortMode.None);
        foreach (var ally in allies)
        {
            float d = Vector3.Distance(self.transform.position, ally.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                nearest = ally;
            }
        }

        // --- 他のAgent（自分以外）も対象に ---
        AgentController[] agents = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
        foreach (var agent in agents)
        {
            if (agent == self) continue;
            float d = Vector3.Distance(self.transform.position, agent.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                nearest = agent;
            }
        }

        return nearest;
    }
}
