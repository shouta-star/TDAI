using UnityEngine;

public class AllySearchState : AllyBaseState
{
    public override void Enter(AllyController ally)
    {
        if (ally.targetEnemy == null)
        {
            ally.ChangeState(new AllyIdleState());
            return;
        }

        Debug.Log($"[{ally.name}] SearchState: 経路探索開始");

        AIManager ai = Object.FindFirstObjectByType<AIManager>();
        Vector3[] path = ai.GetPath(ally.transform.position, ally.targetEnemy.transform.position);
        ally.SetPath(path);

        ally.ChangeState(new AllyMoveState());
    }

    public override void Execute(AllyController ally) { }

    public override void Exit(AllyController ally)
    {
        Debug.Log($"[{ally.name}] SearchState: 終了");
    }
}
