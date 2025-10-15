// Assets/Scripts/Ally/AllyMoveState.cs
using UnityEngine;

public class AllyMoveState : AllyBaseState
{
    public override void Enter(AllyController ally)
    {
        //LogAllyStatus(ally, "Enter Move");
    }

    public override void Execute(AllyController ally)
    {
        LogAllyStatus(ally, "Moving");

        // ターゲット喪失 → Idle
        if (ally.targetEnemy == null)
        {
            ally.ChangeState(new AllyIdleState());
            return;
        }

        // 距離計算（必要なら XZ 判定にする：toTarget.y = 0f;）
        Vector3 toTarget = ally.targetEnemy.transform.position - ally.transform.position;
        float distSqr = toTarget.sqrMagnitude;

        float searchR2 = ally.GetData().searchRadius * ally.GetData().searchRadius;
        float attackR2 = ally.GetData().attackRange * ally.GetData().attackRange;

        // 索敵範囲外に出たら停止（Idle）
        if (distSqr > searchR2)
        {
            ally.ChangeState(new AllyIdleState());
            return;
        }

        // 攻撃射程内なら攻撃へ
        if (distSqr <= attackR2)
        {
            ally.ChangeState(new AllyAttackState());
            return;
        }

        // 接近移動
        ally.transform.position = Vector3.MoveTowards(
            ally.transform.position,
            ally.targetEnemy.transform.position,
            ally.GetData().moveSpeed * Time.deltaTime
        );
    }

    public override void Exit(AllyController ally)
    {
        //LogAllyStatus(ally, "Exit Move");
    }
}


//using UnityEngine;

//public class AllyMoveState : AllyBaseState
//{
//    public override void Enter(AllyController ally)
//    {
//        LogAllyStatus(ally, "Enter Move");
//    }

//    public override void Execute(AllyController ally)
//    {
//        LogAllyStatus(ally, "Moving");

//        if (ally.targetEnemy == null)
//        {
//            // ターゲット喪失 → Idle に戻る
//            ally.ChangeState(new AllyIdleState());
//            return;
//        }

//        // ターゲットまでの距離計算
//        float dist = Vector3.Distance(ally.transform.position, ally.targetEnemy.transform.position);

//        // 索敵範囲外になったら停止（Idleへ）
//        if (dist > 6f)
//        {
//            ally.ChangeState(new AllyIdleState());
//            return;
//        }

//        // 攻撃範囲内なら AttackState へ
//        if (dist <= ally.GetData().attackRange)
//        {
//            ally.ChangeState(new AllyAttackState());
//            return;
//        }

//        // ターゲット方向へ移動
//        Vector3 targetPos = ally.targetEnemy.transform.position;
//        ally.transform.position = Vector3.MoveTowards(
//            ally.transform.position,
//            targetPos,
//            ally.GetData().moveSpeed * Time.deltaTime
//        );
//    }

//    public override void Exit(AllyController ally)
//    {
//        LogAllyStatus(ally, "Exit Move");
//    }
//}


////using UnityEngine;

////public class AllyMoveState : AllyBaseState
////{
////    public override void Enter(AllyController ally)
////    {
////        //Debug.Log($"[{ally.name}] MoveState: 敵を追跡開始");
////    }

////    public override void Execute(AllyController ally)
////    {
////        LogAllyStatus(ally, "Moving");

////        if (ally.targetEnemy == null)
////        {
////            ally.ChangeState(new AllyIdleState());
////            return;
////        }

////        Vector3 enemyPos = ally.targetEnemy.transform.position;

////        // 一定距離以内なら攻撃
////        float dist = Vector3.Distance(ally.transform.position, enemyPos);
////        if (dist <= ally.GetData().attackRange)
////        {
////            ally.ChangeState(new AllyAttackState());
////            return;
////        }

////        // 経路が無ければ再探索
////        if (ally.currentPath == null || ally.currentPath.Length == 0)
////        {
////            ally.ChangeState(new AllySearchState());
////            return;
////        }

////        // 経路に沿って移動
////        Vector3 target = ally.currentPath[ally.pathIndex];
////        ally.transform.position = Vector3.MoveTowards(
////            ally.transform.position,
////            target,
////            ally.GetData().moveSpeed * Time.deltaTime
////        );

////        if (Vector3.Distance(ally.transform.position, target) < 0.05f)
////        {
////            ally.pathIndex++;
////            if (ally.pathIndex >= ally.currentPath.Length)
////            {
////                ally.ChangeState(new AllySearchState()); // 敵が動いた可能性あり再探索
////            }
////        }

////        HeatmapManager.Instance.RecordPosition(ally.transform.position);
////    }

////    public override void Exit(AllyController ally)
////    {
////        //Debug.Log($"[{ally.name}] MoveState: 終了");
////    }
////}
