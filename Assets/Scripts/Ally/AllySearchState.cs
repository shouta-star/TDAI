using UnityEngine;

public class AllySearchState : AllyBaseState
{
    public override void Enter(AllyController ally)
    {
        //LogAllyStatus(ally, "Searching");

        // AllyData から索敵半径を取得
        float radius = ally.GetData().searchRadius;
        AgentController nearest = FindNearestAgentInRange(ally, radius);

        if (nearest != null)
        {
            // 敵が索敵範囲内に見つかった → 追跡開始
            ally.targetEnemy = nearest;
            ally.ChangeState(new AllyMoveState());
        }
        else
        {
            // 敵がいない → 停止（Idle）
            ally.targetEnemy = null;
            ally.ChangeState(new AllyIdleState());
        }
    }

    public override void Execute(AllyController ally)
    {
        // 継続処理は不要（必要なら周期再索敵に変更可）

        LogAllyStatus(ally, "Searching");
    }

    public override void Exit(AllyController ally)
    {
        //LogAllyStatus(ally, "Exit Searching");
    }

    // 索敵範囲内の最も近い Agent を取得（距離二乗で比較）
    private AgentController FindNearestAgentInRange(AllyController ally, float range)
    {
        float rangeSqr = range * range;
        AgentController nearest = null;
        float bestDistSqr = float.MaxValue;

        foreach (var agent in Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None))
        {
            if (agent == null) continue;

            Vector3 d = agent.transform.position - ally.transform.position;
            // XZ 平面だけで判定したい場合： d.y = 0f;
            float distSqr = d.sqrMagnitude; // 二乗距離

            if (distSqr <= rangeSqr && distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                nearest = agent;
            }
        }

        return nearest;
    }
}


//using UnityEngine;

//public class AllySearchState : AllyBaseState
//{
//    private const float SEARCH_RADIUS = 6f; // 索敵範囲（お好みで調整）

//    public override void Enter(AllyController ally)
//    {
//        LogAllyStatus(ally, "Searching");

//        // --- 索敵範囲内の最も近いAgentを探す ---
//        AgentController nearestAgent = FindNearestAgentInRange(ally, SEARCH_RADIUS);

//        if (nearestAgent != null)
//        {
//            // 敵が見つかった → ターゲット設定して MoveState へ
//            ally.targetEnemy = nearestAgent;
//            ally.ChangeState(new AllyMoveState());
//        }
//        else
//        {
//            // 敵がいない → その場で停止（IdleStateへ）
//            ally.targetEnemy = null;
//            ally.ChangeState(new AllyIdleState());
//        }
//    }

//    public override void Execute(AllyController ally)
//    {
//        // 特に継続処理は不要
//    }

//    public override void Exit(AllyController ally)
//    {
//        // 終了ログ（任意）
//        LogAllyStatus(ally, "Exit Searching");
//    }

//    // --- 索敵範囲内の最も近いAgentを返す ---
//    private AgentController FindNearestAgentInRange(AllyController ally, float range)
//    {
//        AgentController nearest = null;
//        float bestDist = float.MaxValue;

//        foreach (var agent in Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None))
//        {
//            float dist = Vector3.Distance(ally.transform.position, agent.transform.position);
//            if (dist < range && dist < bestDist)
//            {
//                bestDist = dist;
//                nearest = agent;
//            }
//        }

//        return nearest;
//    }
//}

////using UnityEngine;

////public class AllySearchState : AllyBaseState
////{
////    public override void Enter(AllyController ally)
////    {
////        if (ally.targetEnemy == null)
////        {
////            ally.ChangeState(new AllyIdleState());
////            return;
////        }

////        //Debug.Log($"[{ally.name}] AllySearchState: 経路探索開始");

////        // Ally専用の経路探索マネージャーを使用
////        AllyAIManager ai = Object.FindFirstObjectByType<AllyAIManager>();
////        if (ai == null)
////        {
////            //Debug.LogError("[AllySearchState] AllyAIManager がシーンに存在しません！");
////            ally.ChangeState(new AllyIdleState());
////            return;
////        }

////        Vector3[] path = ai.GetPath(ally.transform.position, ally.targetEnemy.transform.position);
////        ally.SetPath(path);

////        ally.ChangeState(new AllyMoveState());
////    }

////    public override void Execute(AllyController ally) 
////    {
////        LogAllyStatus(ally, "Searching");
////    }

////    public override void Exit(AllyController ally)
////    {
////        //Debug.Log($"[{ally.name}] AllySearchState: 終了");
////    }
////}