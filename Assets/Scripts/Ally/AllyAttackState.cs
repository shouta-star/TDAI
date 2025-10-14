using UnityEngine;

public class AllyAttackState : AllyBaseState
{
    private float attackTimer = 0f;

    public override void Enter(AllyController ally)
    {
        Debug.Log($"[{ally.name}] AttackState: UŒ‚ŠJŽn");
        attackTimer = 0f;
    }

    public override void Execute(AllyController ally)
    {
        if (ally.targetEnemy == null)
        {
            ally.ChangeState(new AllyIdleState());
            return;
        }

        float dist = Vector3.Distance(ally.transform.position, ally.targetEnemy.transform.position);
        if (dist > ally.GetData().attackRange)
        {
            ally.ChangeState(new AllySearchState());
            return;
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            ally.Attack(ally.targetEnemy);
            attackTimer = ally.GetData().attackInterval;
        }
    }

    public override void Exit(AllyController ally)
    {
        Debug.Log($"[{ally.name}] AttackState: I—¹");
    }
}
