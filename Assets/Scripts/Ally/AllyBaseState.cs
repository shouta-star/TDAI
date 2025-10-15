using UnityEngine;

public abstract class AllyBaseState
{
    public abstract void Enter(AllyController ally);
    public abstract void Execute(AllyController ally);
    public abstract void Exit(AllyController ally);

    // ============================
    // 共通ログ出力関数
    // ============================
    protected void LogAllyStatus(AllyController ally, string action)
    {
        string stateName = GetType().Name;
        string targetName = "None";
        Vector3 targetPos = Vector3.zero;

        // --- 現在の攻撃ターゲットを探索（敵 = Agent） ---
        MonoBehaviour nearest = FindNearestEnemy(ally);
        if (nearest != null)
        {
            targetName = nearest.name;
            targetPos = ((Component)nearest).transform.position;
        }

        Vector3 self = ally.transform.position;

        Debug.Log(
            $"[AllyLog] {ally.name} | State={stateName} | Action={action} | " +
            $"Target={targetName} | TargetPos=({targetPos.x:F2},{targetPos.y:F2},{targetPos.z:F2}) | " +
            $"SelfPos=({self.x:F2},{self.y:F2},{self.z:F2})"
        );
    }

    // ============================
    // 共通ターゲット探索（敵 = Agent）
    // ============================
    protected MonoBehaviour FindNearestEnemy(AllyController self)
    {
        float bestDist = float.MaxValue;
        MonoBehaviour nearest = null;

        AgentController[] agents = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
        foreach (var agent in agents)
        {
            if (agent == null) continue;
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
