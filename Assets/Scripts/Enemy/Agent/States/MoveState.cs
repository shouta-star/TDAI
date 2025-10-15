using UnityEngine;

public class MoveState : AgentBaseState
{
    private float attackCooldown = 0f;

    public override void Enter(AgentController agent)
    {
        Debug.Log($"[{agent.name}] MoveState: ŠJn");
    }

    public override void Execute(AgentController agent)
    {
        attackCooldown -= Time.deltaTime;

        // ----------------------------
        // ‡@ UŒ‚”ÍˆÍƒ`ƒFƒbƒN
        // ----------------------------
        MonoBehaviour nearest = FindNearestEnemy(agent);
        if (nearest != null)
        {
            float dist = Vector3.Distance(agent.transform.position, ((Component)nearest).transform.position);

            // UŒ‚”ÍˆÍ“à‚È‚ç AttackState ‚Ö‘JˆÚ‚µ‚Ä’â~
            if (dist <= agent.GetData().attackRange && attackCooldown <= 0f)
            {
                // --- ˆÊ’u‚ğŒy‚­ƒXƒiƒbƒv‚µ‚ÄƒuƒŒ‚ğ–h~ ---
                agent.transform.position = new Vector3(
                    Mathf.Round(agent.transform.position.x * 100f) / 100f,
                    agent.transform.position.y,
                    Mathf.Round(agent.transform.position.z * 100f) / 100f
                );

                // --- Œo˜H”jŠü•ˆÚ“®’â~ ---
                agent.currentPath = null;
                agent.pathIndex = 0;

                // --- AttackState‚Ö‘JˆÚi‚±‚ÌƒtƒŒ[ƒ€‚Å‚Í“®‚©‚È‚¢j ---
                agent.ChangeState(new AttackState());
                return;
            }
        }

        // ----------------------------
        // ‡A Œo˜HÄ’Tõƒ`ƒFƒbƒN
        // ----------------------------
        if (agent.needReplan)
        {
            agent.needReplan = false;
            agent.ChangeState(new SearchState());
            return;
        }

        // ----------------------------
        // ‡B Œo˜H–¢İ’è‚È‚ç’Tõ‚Ö
        // ----------------------------
        if (agent.currentPath == null || agent.currentPath.Length == 0)
        {
            agent.ChangeState(new SearchState());
            return;
        }

        // ----------------------------
        // ‡C ˆÚ“®‘¬“x‚ª0iUŒ‚’†j‚È‚ç“®‚©‚È‚¢
        // ----------------------------
        if (agent.GetData().moveSpeed <= 0f)
            return;

        // ----------------------------
        // ‡D Œo˜H‚É‰ˆ‚Á‚ÄˆÚ“®
        // ----------------------------
        Vector3 target = agent.currentPath[agent.pathIndex];
        Vector3 current = agent.transform.position;
        target.y = current.y = 0f;

        agent.transform.position = Vector3.MoveTowards(
            current,
            target,
            agent.GetData().moveSpeed * Time.deltaTime
        );

        // Œo˜H“_‚É“’B‚µ‚½‚çŸ‚Ö
        if (Vector3.Distance(agent.transform.position, target) < 0.01f)
        {
            agent.transform.position = target;
            agent.pathIndex++;

            if (agent.pathIndex >= agent.currentPath.Length)
            {
                agent.ChangeState(new GoalState());
                return;
            }
        }

        // ----------------------------
        // ‡E Heatmap‹L˜^
        // ----------------------------
        HeatmapManager.Instance.RecordPosition(agent.transform.position);
    }

    public override void Exit(AgentController agent)
    {
        Debug.Log($"[{agent.name}] MoveState: I—¹");
    }

    // ----------------------------
    // ÅŠñ‚è‚Ì“G‚ğ’T‚·iAgent + Ally—¼‘Î‰j
    // ----------------------------
    private MonoBehaviour FindNearestEnemy(AgentController self)
    {
        float bestDist = float.MaxValue;
        MonoBehaviour nearest = null;

        // --- Allyi–¡•ûj‚ğ“G‚Æ‚µ‚ÄŒŸõ ---
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

        // --- ‘¼‚ÌAgenti©•ªˆÈŠOj‚àŠÜ‚ß‚é ---
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