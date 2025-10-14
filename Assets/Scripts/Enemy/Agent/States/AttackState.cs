using UnityEngine;

public class AttackState : AgentBaseState
{
    private float attackTimer;
    private float attackInterval;
    private float originalMoveSpeed; // 復元用

    public override void Enter(AgentController agent)
    {
        attackInterval = agent.GetData().attackInterval;

        // 経路を破棄（MoveStateの移動を断つ）
        agent.currentPath = null;
        agent.pathIndex = 0;
        agent.needReplan = false;

        // 速度を0へ上書き（個体ランタイムコピーなので安全）
        originalMoveSpeed = agent.GetData().moveSpeed;
        agent.GetData().moveSpeed = 0f;

        // （見た目の滑り対策が必要なら）位置を軽くスナップ
        // var p = agent.transform.position;
        // agent.transform.position = new Vector3(Mathf.Round(p.x*100f)/100f, p.y, Mathf.Round(p.z*100f)/100f);

        Debug.Log($"[{agent.name}] AttackState: Enter speed {originalMoveSpeed} -> {agent.GetData().moveSpeed}");
        attackTimer = 0f;
    }

    public override void Execute(AgentController agent)
    {
        // ターゲットが射程外に出たら移動へ戻す
        var enemy = FindNearestEnemy(agent);
        if (enemy == null)
        {
            agent.ChangeState(new SearchState());
            return;
        }

        float dist = Vector3.Distance(agent.transform.position, enemy.transform.position);
        if (dist > agent.GetData().attackRange)
        {
            // 復元して追跡へ
            agent.GetData().moveSpeed = originalMoveSpeed;
            agent.ChangeState(new MoveState());
            return;
        }

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            agent.Attack(enemy);
        }
    }

    public override void Exit(AgentController agent)
    {
        // 念のためここでも復元（Searchに行くケースなどの保険）
        agent.GetData().moveSpeed = originalMoveSpeed;
        Debug.Log($"[{agent.name}] AttackState: Exit speed -> {agent.GetData().moveSpeed}");
    }

    private AgentController FindNearestEnemy(AgentController self)
    {
        var all = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
        AgentController best = null;
        float bestDist = float.MaxValue;
        foreach (var a in all)
        {
            if (a == self) continue;
            float d = Vector3.Distance(self.transform.position, a.transform.position);
            if (d < bestDist) { bestDist = d; best = a; }
        }
        return best;
    }
}


//using UnityEngine;

//public class AttackState : AgentBaseState
//{
//    private float attackTimer = 0f;
//    private float attackInterval;
//    private float originalMoveSpeed;

//    public override void Enter(AgentController agent)
//    {
//        //attackInterval = agent.GetData().attackInterval;
//        //Debug.Log($"[{agent.name}] AttackState: 開始");

//        //// 経路を破棄して移動停止
//        //agent.currentPath = null;

//        //// 現在の移動速度を保存して 0 に変更
//        //originalMoveSpeed = agent.GetData().moveSpeed;
//        //agent.GetData().moveSpeed = 0f;

//        attackInterval = agent.GetData().attackInterval;
//        Debug.Log($"[{agent.name}] AttackState: 開始");

//        agent.currentPath = null;

//        Debug.Log($"[{agent.name}] AttackState: 開始 (Speed Before={agent.GetData().moveSpeed})");

//        originalMoveSpeed = agent.GetData().moveSpeed;
//        agent.GetData().moveSpeed = 0f;

//        //Debug.Log($"[AttackState/Enter] {agent.name} speed(before)={originalMoveSpeed}, after={agent.GetData().moveSpeed}");
//        Debug.Log($"[{agent.name}] AttackState: MoveSpeed Set To {agent.GetData().moveSpeed}");
//    }

//    public override void Execute(AgentController agent)
//    {
//        attackTimer -= Time.deltaTime;

//        AgentController target = FindNearestEnemy(agent);
//        if (target == null)
//        {
//            agent.ChangeState(new SearchState());
//            return;
//        }

//        float dist = Vector3.Distance(agent.transform.position, target.transform.position);
//        if (dist > agent.GetData().attackRange)
//        {
//            // 攻撃範囲を外れたら速度を戻して移動再開
//            agent.GetData().moveSpeed = originalMoveSpeed;
//            agent.ChangeState(new MoveState());
//            return;
//        }

//        if (attackTimer <= 0f)
//        {
//            attackTimer = attackInterval;
//            agent.Attack(target);
//        }
//    }

//    private AgentController FindNearestEnemy(AgentController self)
//    {
//        AgentController[] all = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
//        AgentController nearest = null;
//        float minDist = float.MaxValue;

//        foreach (var other in all)
//        {
//            if (other == self) continue;
//            float d = Vector3.Distance(self.transform.position, other.transform.position);
//            if (d < minDist)
//            {
//                minDist = d;
//                nearest = other;
//            }
//        }
//        return nearest;
//    }

//    public override void Exit(AgentController agent)
//    {
//        // 攻撃終了時に元の速度へ戻す
//        agent.GetData().moveSpeed = originalMoveSpeed;
//        Debug.Log($"[{agent.name}] AttackState: 終了");
//    }
//}


////using UnityEngine;

////public class AttackState : AgentBaseState
////{
////    private float attackTimer = 0f;
////    private float attackInterval;
////    private float originalMoveSpeed; // 元の速度を記録

////    public override void Enter(AgentController agent)
////    {
////        attackInterval = agent.GetData().attackInterval;
////        Debug.Log($"[{agent.name}] AttackState: 開始");

////        // 経路を破棄して移動停止
////        agent.currentPath = null;

////        // ★ 現在の速度を保存して 0 に設定
////        originalMoveSpeed = agent.GetData().moveSpeed;
////        agent.SetMoveSpeed(0f);
////    }

////    public override void Execute(AgentController agent)
////    {
////        attackTimer -= Time.deltaTime;

////        AgentController target = FindNearestEnemy(agent);
////        if (target == null)
////        {
////            agent.ChangeState(new SearchState());
////            return;
////        }

////        float dist = Vector3.Distance(agent.transform.position, target.transform.position);
////        if (dist > agent.GetData().attackRange)
////        {
////            // 攻撃範囲を外れたら速度を戻して移動へ
////            agent.SetMoveSpeed(originalMoveSpeed);
////            agent.ChangeState(new MoveState());
////            return;
////        }

////        if (attackTimer <= 0f)
////        {
////            attackTimer = attackInterval;
////            agent.Attack(target);
////        }
////    }

////    private AgentController FindNearestEnemy(AgentController self)
////    {
////        AgentController[] all = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
////        AgentController nearest = null;
////        float minDist = float.MaxValue;

////        foreach (var other in all)
////        {
////            if (other == self) continue;
////            float d = Vector3.Distance(self.transform.position, other.transform.position);
////            if (d < minDist)
////            {
////                minDist = d;
////                nearest = other;
////            }
////        }
////        return nearest;
////    }

////    public override void Exit(AgentController agent)
////    {
////        // ★ AttackState 終了時に速度を戻す
////        agent.SetMoveSpeed(originalMoveSpeed);
////        Debug.Log($"[{agent.name}] AttackState: 終了");
////    }
////}


//////using UnityEngine;

//////public class AttackState : AgentBaseState
//////{
//////    private float attackTimer = 0f;
//////    private float attackInterval;

//////    public override void Enter(AgentController agent)
//////    {
//////        attackInterval = agent.GetData().attackInterval;
//////        Debug.Log($"[{agent.name}] AttackState: 開始");

//////        // 経路を破棄して移動停止
//////        agent.currentPath = null;

//////        //attackInterval = agent.GetData().attackInterval;
//////        //Debug.Log($"[{agent.name}] AttackState: 開始");

//////        //// 経路を破棄して移動停止
//////        //agent.currentPath = null;

//////        //// ★ moveSpeed を 0 に設定
//////        //agent.SetMoveSpeed(0f);
//////    }

//////    public override void Execute(AgentController agent)
//////    {
//////        attackTimer -= Time.deltaTime;

//////        AgentController target = FindNearestEnemy(agent);
//////        if (target == null)
//////        {
//////            agent.ChangeState(new SearchState());
//////            return;
//////        }

//////        float dist = Vector3.Distance(agent.transform.position, target.transform.position);
//////        if (dist > agent.GetData().attackRange)
//////        {
//////            agent.ChangeState(new MoveState());
//////            return;
//////        }

//////        if (attackTimer <= 0f)
//////        {
//////            attackTimer = attackInterval;
//////            agent.Attack(target);
//////        }
//////    }

//////    private AgentController FindNearestEnemy(AgentController self)
//////    {
//////        // �������F�������ł��߂�����Agent��G�Ƃ݂Ȃ�
//////        AgentController[] all = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
//////        AgentController nearest = null;
//////        float minDist = float.MaxValue;

//////        foreach (var other in all)
//////        {
//////            if (other == self) continue;
//////            float d = Vector3.Distance(self.transform.position, other.transform.position);
//////            if (d < minDist)
//////            {
//////                minDist = d;
//////                nearest = other;
//////            }
//////        }
//////        return nearest;
//////    }

//////    public override void Exit(AgentController agent)

//////    {
//////        // ★ AttackState 終了時に元の速度へ戻す
//////        //agent.ResetMoveSpeed();

//////        Debug.Log($"[{agent.name}] AttackState: 終了");
//////    }
//////}
