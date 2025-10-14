using UnityEngine;

public class AllyMoveState : AllyBaseState
{
    public override void Enter(AllyController ally)
    {
        Debug.Log($"[{ally.name}] MoveState: “G‚ğ’ÇÕŠJn");
    }

    public override void Execute(AllyController ally)
    {
        if (ally.targetEnemy == null)
        {
            ally.ChangeState(new AllyIdleState());
            return;
        }

        Vector3 enemyPos = ally.targetEnemy.transform.position;

        // ˆê’è‹——£ˆÈ“à‚È‚çUŒ‚
        float dist = Vector3.Distance(ally.transform.position, enemyPos);
        if (dist <= ally.GetData().attackRange)
        {
            ally.ChangeState(new AllyAttackState());
            return;
        }

        // Œo˜H‚ª–³‚¯‚ê‚ÎÄ’Tõ
        if (ally.currentPath == null || ally.currentPath.Length == 0)
        {
            ally.ChangeState(new AllySearchState());
            return;
        }

        // Œo˜H‚É‰ˆ‚Á‚ÄˆÚ“®
        Vector3 target = ally.currentPath[ally.pathIndex];
        ally.transform.position = Vector3.MoveTowards(
            ally.transform.position,
            target,
            ally.GetData().moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(ally.transform.position, target) < 0.05f)
        {
            ally.pathIndex++;
            if (ally.pathIndex >= ally.currentPath.Length)
            {
                ally.ChangeState(new AllySearchState()); // “G‚ª“®‚¢‚½‰Â”\«‚ ‚èÄ’Tõ
            }
        }

        HeatmapManager.Instance.RecordPosition(ally.transform.position);
    }

    public override void Exit(AllyController ally)
    {
        Debug.Log($"[{ally.name}] MoveState: I—¹");
    }
}
