using UnityEngine;

public class AllyIdleState : AllyBaseState
{
    public override void Enter(AllyController ally)
    {
        Debug.Log($"[{ally.name}] IdleState: ë“ã@íÜ");
    }

    public override void Execute(AllyController ally)
    {
        // ìGÇíTçı
        var enemy = FindNearestEnemy(ally);
        if (enemy != null)
        {
            ally.targetEnemy = enemy;
            ally.ChangeState(new AllySearchState());
        }
    }

    public override void Exit(AllyController ally) { }

    private AgentController FindNearestEnemy(AllyController self)
    {
        AgentController[] enemies = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
        AgentController nearest = null;
        float minDist = float.MaxValue;

        foreach (var e in enemies)
        {
            float d = Vector3.Distance(self.transform.position, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = e;
            }
        }
        return nearest;
    }
}
