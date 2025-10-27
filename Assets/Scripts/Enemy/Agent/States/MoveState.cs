using UnityEngine;

public class MoveState : AgentBaseState
{
    private Vector3 lastPosition;
    private float stuckTimer = 0f;

    public override void Enter(AgentController agent)
    {
        lastPosition = agent.transform.position;

        // --- 経路の安全確認 ---
        if (agent.currentPath == null || agent.currentPath.Length < 2)
        {
            Debug.LogWarning($"[MoveState] {agent.name}: 経路が不完全 (len={agent.currentPath?.Length ?? 0}) → 再探索");
            agent.ChangeState(new SearchState());
            return;
        }

        LogAgentStatus(agent, "Moving");

        // デバッグ：パス末端とゴール距離を確認
        Vector3 goalPos = agent.GetGoalPosition();
        Vector3 pathEnd = agent.currentPath[agent.currentPath.Length - 1];
        Debug.Log($"[MoveState:Enter] {agent.name} pathEnd={pathEnd} goal={goalPos} dist={Vector3.Distance(pathEnd, goalPos):F3}");
    }

    public override void Execute(AgentController agent)
    {
        LogAgentStatus(agent, "Moving");

        // --- 経路存在チェック ---
        if (agent.currentPath == null || agent.currentPath.Length < 2)
        {
            Debug.LogWarning($"[MoveState] {agent.name}: 経路が短すぎるため再探索へ");
            agent.ChangeState(new SearchState());
            return;
        }

        // --- 終端チェックの前に距離優先で確認 ---
        Vector3 goalPos = agent.GetGoalPosition();
        float goalDist = Vector3.Distance(agent.transform.position, goalPos);
        Debug.Log($"[Move:EndCheck] {agent.name} goalDist={goalDist:F3} self={agent.transform.position} goal={goalPos}");

        // もし経路の最後に近い or 距離が近い場合でも、
        // まず距離で再探索 or ゴール判定を優先する
        if (goalDist > 0.5f && agent.pathIndex >= agent.currentPath.Length - 1)
        {
            Debug.LogWarning($"[MoveState] {agent.name}: Path末端だがGoal未到達 (距離 {goalDist:F2}) → 再探索");
            agent.ChangeState(new SearchState());
            return;
        }
        else if (goalDist <= 0.5f && agent.pathIndex >= agent.currentPath.Length - 1)
        {
            Debug.Log($"[MoveState] {agent.name}: Goal近距離 ({goalDist:F2}) → GoalStateへ");
            agent.ChangeState(new GoalState());
            return;
        }

        // --- 経路上の目標ノードに向かう ---
        if (agent.pathIndex < agent.currentPath.Length)
        {
            Vector3 target = agent.currentPath[agent.pathIndex];
            agent.MoveTo(target);

            float dist = Vector3.Distance(agent.transform.position, target);

            // ノード到達時に次へ
            if (dist < 0.05f)
            {
                agent.transform.position = target;
                agent.pathIndex++;
                Debug.Log($"[Move:WaypointReached] {agent.name} nextIdx={agent.pathIndex}/{agent.currentPath.Length}");
            }
        }

        // --- スタック検知 ---
        if (Vector3.Distance(agent.transform.position, lastPosition) < 0.001f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 1.0f)
            {
                Debug.LogWarning($"[MoveState] {agent.name} 移動停止検出 → 再探索");
                agent.ChangeState(new SearchState());
                stuckTimer = 0f;
                return;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = agent.transform.position;
    }

    public override void Exit(AgentController agent)
    {
        Debug.Log($"[MoveState] {agent.name}: Exit");
    }
}


////using UnityEngine;

////public class MoveState : AgentBaseState
////{
////    private Vector3 lastPosition;
////    private float stuckTimer = 0f;

////    public override void Enter(AgentController agent)
////    {
////        lastPosition = agent.transform.position;

////        // --- 経路の安全確認 ---
////        if (agent.currentPath == null || agent.currentPath.Length < 2)
////        {
////            Debug.LogWarning($"[MoveState] {agent.name}: 経路が不完全 (len={agent.currentPath?.Length ?? 0}) → 再探索");
////            agent.ChangeState(new SearchState());
////            return;
////        }

////        LogAgentStatus(agent, "Moving");
////        Debug.Log($"[MoveState] {agent.name}: Path length={agent.currentPath.Length}, StartIndex={agent.pathIndex}");
////    }

////    public override void Execute(AgentController agent)
////    {
////        LogAgentStatus(agent, "Moving");

////        // --- 経路が存在しない・短すぎる場合は再探索へ ---
////        if (agent.currentPath == null || agent.currentPath.Length < 2)
////        {
////            Debug.LogWarning($"[MoveState] {agent.name}: 経路が短すぎるため再探索へ");
////            agent.ChangeState(new SearchState());
////            return;
////        }

////        // --- 経路終端チェック ---
////        if (agent.pathIndex >= agent.currentPath.Length)
////        {
////            Vector3 goalPos = agent.GetGoalPosition();
////            float goalDist = Vector3.Distance(agent.transform.position, goalPos);

////            Debug.Log($"[Move:EndCheck] {agent.name} goalDist={goalDist:F3} self={agent.transform.position} goal={goalPos}");

////            // 改善：実際にゴール付近まで近づいた場合のみGoalStateへ
////            if (goalDist <= 0.3f)
////            {
////                Debug.Log($"[MoveState] {agent.name} 距離 {goalDist:F2} → GoalStateへ");
////                agent.ChangeState(new GoalState());
////                return;
////            }
////            else
////            {
////                Debug.LogWarning($"[MoveState] {agent.name}: 終端だがGoal未到達 (距離 {goalDist:F2}) → 再探索");
////                agent.ChangeState(new SearchState());
////                return;
////            }
////        }

////        // --- 現在の目標ノードへ移動 ---
////        Vector3 target = agent.currentPath[agent.pathIndex];
////        agent.MoveTo(target);

////        float dist = Vector3.Distance(agent.transform.position, target);

////        // --- ノード到達時 ---
////        if (dist < 0.05f)
////        {
////            agent.transform.position = target;
////            agent.pathIndex++;

////            Debug.Log($"[Move:WaypointReached] {agent.name} nextIdx={agent.pathIndex}/{agent.currentPath.Length}");

////            // --- 経路終端に到達した場合 ---
////            if (agent.pathIndex >= agent.currentPath.Length)
////            {
////                Vector3 goalPos = agent.GetGoalPosition();
////                float goalDist = Vector3.Distance(agent.transform.position, goalPos);
////                Debug.Log($"[Move:EndCheck] {agent.name} goalDist={goalDist:F3} self={agent.transform.position} goal={goalPos}");

////                if (goalDist <= 0.3f)
////                {
////                    Debug.Log($"[MoveState] {agent.name} ゴール近距離 ({goalDist:F2}) → GoalStateへ");
////                    agent.ChangeState(new GoalState());
////                    return;
////                }
////                else
////                {
////                    Debug.LogWarning($"[MoveState] {agent.name}: Goal未到達 (距離 {goalDist:F2}) → SearchStateへ");
////                    agent.ChangeState(new SearchState());
////                    return;
////                }
////            }
////        }

////        // --- スタック検知（動いていない場合） ---
////        if (Vector3.Distance(agent.transform.position, lastPosition) < 0.001f)
////        {
////            stuckTimer += Time.deltaTime;
////            if (stuckTimer > 1.0f)
////            {
////                Debug.LogWarning($"[MoveState] {agent.name} 移動停止検出 → SearchStateへ");
////                agent.ChangeState(new SearchState());
////                stuckTimer = 0f;
////                return;
////            }
////        }
////        else
////        {
////            stuckTimer = 0f;
////        }

////        lastPosition = agent.transform.position;
////    }

////    public override void Exit(AgentController agent)
////    {
////        Debug.Log($"[MoveState] {agent.name}: Exit");
////    }
////}


//using UnityEngine;

//public class MoveState : AgentBaseState
//{
//    private Vector3 lastPosition;
//    private float stuckTimer = 0f;

//    public override void Enter(AgentController agent)
//    {
//        lastPosition = agent.transform.position;

//        // --- 経路の安全確認 ---
//        if (agent.currentPath == null || agent.currentPath.Length < 2)
//        {
//            //Debug.LogWarning($"[MoveState] {agent.name}: 経路が不完全 (len={agent.currentPath?.Length ?? 0}) → 再探索");
//            agent.ChangeState(new SearchState());
//            return;
//        }

//        LogAgentStatus(agent, "Moving");
//        //Debug.Log($"[MoveState] {agent.name}: Path length={agent.currentPath.Length}, StartIndex={agent.pathIndex}");
//    }

//    public override void Execute(AgentController agent)
//    {
//        LogAgentStatus(agent, "Moving");

//        if (agent.currentPath == null || agent.currentPath.Length < 2)
//        {
//            //Debug.LogWarning($"[MoveState] {agent.name}: 経路が短すぎるため再探索へ");
//            agent.ChangeState(new SearchState());
//            return;
//        }

//        if (agent.pathIndex >= agent.currentPath.Length)
//        {
//            //Debug.LogWarning($"[Move:EarlyGoal?] {agent.name} idx={agent.pathIndex} >= len={agent.currentPath.Length}");
//            agent.ChangeState(new GoalState());
//            return;
//        }

//        Vector3 target = agent.currentPath[agent.pathIndex];
//        agent.MoveTo(target);

//        float dist = Vector3.Distance(agent.transform.position, target);

//        // --- 経路上の次ノードに近づいたとき ---
//        if (dist < 0.05f)
//        {
//            agent.transform.position = target;
//            agent.pathIndex++;

//            //Debug.Log($"[Move:WaypointReached] {agent.name} nextIdx={agent.pathIndex}/{agent.currentPath.Length}");

//            // --- 経路終端 ---
//            if (agent.pathIndex >= agent.currentPath.Length)
//            {
//                float goalDist = Vector3.Distance(agent.transform.position, agent.GetGoalPosition());
//                Debug.Log($"[Move:EndCheck] {agent.name} goalDist={goalDist:F3} self={agent.transform.position} goal={agent.GetGoalPosition()}");

//                if (goalDist < 0.5f)
//                {
//                    agent.ChangeState(new GoalState());
//                    return;
//                }
//                else
//                {
//                    //Debug.LogWarning($"[MoveState] {agent.name}: 終端だがGoal未到達 (距離 {goalDist:F2}) → 再探索");
//                    agent.ChangeState(new SearchState());
//                    return;
//                }
//            }
//        }

//        // --- 停止検知（動けていない場合は再探索へ） ---
//        if (Vector3.Distance(agent.transform.position, lastPosition) < 0.001f)
//        {
//            stuckTimer += Time.deltaTime;
//            if (stuckTimer > 1.0f)
//            {
//                //Debug.LogWarning($"[MoveState] {agent.name} 移動停止検出 → SearchStateへ");
//                agent.ChangeState(new SearchState());
//                stuckTimer = 0f;
//            }
//        }
//        else
//        {
//            stuckTimer = 0f;
//        }

//        lastPosition = agent.transform.position;
//    }

//    public override void Exit(AgentController agent)
//    {
//        //Debug.Log($"[MoveState] {agent.name}: Exit");
//    }
//}