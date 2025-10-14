using UnityEngine;
using System.Linq; // ← 近距離敵検索に使用

public class MoveState : AgentBaseState
{
    private float attackCooldown = 0f;

    public override void Enter(AgentController agent)
    {
        Debug.Log($"[{agent.name}] MoveState: 開始");
    }

    public override void Execute(AgentController agent)
    {
        // 攻撃クールタイム減算
        attackCooldown -= Time.deltaTime;

        // ==============================
        // ① 敵が近くにいれば攻撃する
        // ==============================
        var nearest = FindNearestEnemy(agent);
        if (nearest != null)
        {
            float dist = Vector3.Distance(agent.transform.position, nearest.transform.position);
            if (dist <= agent.GetData().attackRange && attackCooldown <= 0f)
            {
                agent.Attack(nearest);
                attackCooldown = agent.GetData().attackInterval;
                return; // 攻撃したら一旦停止
            }
        }

        // ==============================
        // ② 経路再探索チェック
        // ==============================
        if (agent.needReplan)
        {
            agent.needReplan = false;
            agent.ChangeState(new SearchState());
            return;
        }

        // 経路が無ければ探索へ戻る
        if (agent.currentPath == null || agent.currentPath.Length == 0)
        {
            agent.ChangeState(new SearchState());
            return;
        }

        // 現在の目標マス
        Vector3 target = agent.currentPath[agent.pathIndex];
        Vector3 current = agent.transform.position;

        target.y = current.y = 0f;

        agent.transform.position = Vector3.MoveTowards(
            current,
            target,
            agent.GetData().moveSpeed * Time.deltaTime
        );

        // 到達したら次のマスへ
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

        HeatmapManager.Instance.RecordPosition(agent.transform.position);
    }

    private AgentController FindNearestEnemy(AgentController self)
    {
        // 自分以外の全Agentから最も近いものを探す
        AgentController[] all = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
        AgentController nearest = null;
        float minDist = float.MaxValue;

        foreach (var other in all)
        {
            if (other == self) continue;
            float d = Vector3.Distance(self.transform.position, other.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = other;
            }
        }

        return nearest;
    }

    public override void Exit(AgentController agent)
    {
        Debug.Log($"[{agent.name}] MoveState: 終了");
    }
}


//using UnityEngine;

//public class MoveState : AgentBaseState
//{
//    public override void Enter(AgentController agent)
//    {
//        Debug.Log($"[{agent.name}] MoveState: 開始");
//    }

//    public override void Execute(AgentController agent)
//    {
//        // 再探索要求対応
//        if (agent.needReplan)
//        {
//            agent.needReplan = false;
//            agent.ChangeState(new SearchState());
//            return;
//        }

//        // 経路が無ければ探索へ戻る
//        if (agent.currentPath == null || agent.currentPath.Length == 0)
//        {
//            agent.ChangeState(new SearchState());
//            return;
//        }

//        // 現在の目標マス
//        Vector3 target = agent.currentPath[agent.pathIndex];
//        Vector3 current = agent.transform.position;

//        // ★Yを固定してPlane上を移動
//        target.y = current.y = 0f;

//        // ★マス中心への補間
//        agent.transform.position = Vector3.MoveTowards(
//            current,
//            target,
//            agent.GetData().moveSpeed * Time.deltaTime
//        );

//        // ★マス中心に到達したらスナップ＋次のマスへ
//        if (Vector3.Distance(agent.transform.position, target) < 0.01f)
//        {
//            agent.transform.position = target; // スナップ
//            agent.pathIndex++;

//            if (agent.pathIndex >= agent.currentPath.Length)
//            {
//                agent.ChangeState(new GoalState());
//                return;
//            }
//        }

//        HeatmapManager.Instance.RecordPosition(agent.transform.position);
//    }

//    public override void Exit(AgentController agent)
//    {
//        Debug.Log($"[{agent.name}] MoveState: 終了");
//    }
//}


////using UnityEngine;

////public class MoveState : AgentBaseState
////{
////    public override void Enter(AgentController agent)
////    {
////        Debug.Log($"[{agent.name}] MoveState: 開始");
////    }

////    public override void Execute(AgentController agent)
////    {
////        // 再探索要求
////        if (agent.needReplan)
////        {
////            agent.needReplan = false;
////            agent.ChangeState(new SearchState());
////            return;
////        }

////        // 経路未設定なら探索へ
////        if (agent.currentPath == null || agent.currentPath.Length == 0)
////        {
////            agent.ChangeState(new SearchState());
////            return;
////        }

////        // 現在の目標マス
////        Vector3 target = agent.currentPath[agent.pathIndex];

////        // 時間的に補間して移動
////        agent.transform.position = Vector3.MoveTowards(
////            agent.transform.position,
////            target,
////            agent.GetData().moveSpeed * Time.deltaTime
////        );

////        // 近づいたら次のマスへ
////        if (Vector3.Distance(agent.transform.position, target) < 0.05f)
////        {
////            agent.pathIndex++;
////            if (agent.pathIndex >= agent.currentPath.Length)
////            {
////                agent.ChangeState(new GoalState());
////                return;
////            }
////        }

////        HeatmapManager.Instance.RecordPosition(agent.transform.position);
////    }

////    public override void Exit(AgentController agent)
////    {
////        Debug.Log($"[{agent.name}] MoveState: 終了");
////    }
////}

//////using UnityEngine;

//////public class MoveState : AgentBaseState
//////{
//////    public override void Enter(AgentController agent)
//////    {
//////        Debug.Log($"[{agent.name}] MoveState: 開始");
//////    }

//////    public override void Execute(AgentController agent)
//////    {
//////        if (agent.needReplan)
//////        {
//////            agent.needReplan = false;
//////            agent.ChangeState(new SearchState());
//////            return;
//////        }

//////        if (agent.currentPath == null || agent.currentPath.Length == 0)
//////        {
//////            agent.ChangeState(new SearchState());
//////            return;
//////        }

//////        // ★ 現在の目標マス
//////        Vector3 target = agent.currentPath[agent.pathIndex];

//////        // ★ 補間して移動（スナップではなく）
//////        agent.transform.position = Vector3.MoveTowards(
//////            agent.transform.position,
//////            target,
//////            agent.GetData().moveSpeed * Time.deltaTime
//////        );

//////        // ★ 1フレームでマス中心までスナップ移動
//////        agent.transform.position = target;

//////        // 次のマスへ進行
//////        agent.pathIndex++;
//////        if (agent.pathIndex >= agent.currentPath.Length)
//////        {
//////            agent.ChangeState(new GoalState());
//////            return;
//////        }

//////        HeatmapManager.Instance.RecordPosition(agent.transform.position);
//////    }

//////    public override void Exit(AgentController agent)
//////    {
//////        Debug.Log($"[{agent.name}] MoveState: 終了");
//////    }
//////}


////////using UnityEngine;

////////public class MoveState : AgentBaseState
////////{
////////    public override void Enter(AgentController agent)
////////    {
////////        Debug.Log($"[{agent.name}] MoveState: 開始");
////////    }

////////    public override void Execute(AgentController agent)
////////    {
////////        // ★ここで再探索要求に対応
////////        if (agent.needReplan)
////////        {
////////            agent.needReplan = false;
////////            agent.ChangeState(new SearchState());
////////            return;
////////        }

////////        if (agent.currentPath == null || agent.currentPath.Length == 0)
////////        {
////////            agent.ChangeState(new SearchState());
////////            return;
////////        }

////////        Vector3 target = agent.currentPath[agent.pathIndex];
////////        agent.MoveTo(target);

////////        if (Vector3.Distance(agent.transform.position, target) < 0.1f)
////////        {
////////            agent.pathIndex++;
////////            if (agent.pathIndex >= agent.currentPath.Length)
////////            {
////////                agent.ChangeState(new GoalState());
////////            }
////////        }

////////        HeatmapManager.Instance.RecordPosition(agent.transform.position);
////////    }

////////    public override void Exit(AgentController agent)
////////    {
////////        Debug.Log($"[{agent.name}] MoveState: 終了");
////////    }
////////}
