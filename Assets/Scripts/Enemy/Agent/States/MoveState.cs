using UnityEngine;

public class MoveState : AgentBaseState
{
    private Vector3 lastPosition;
    private float stuckTimer = 0f;

    public override void Enter(AgentController agent)
    {
        LogAgentStatus(agent, "Moving");
        lastPosition = agent.transform.position;
    }

    public override void Execute(AgentController agent)
    {
        if (agent.currentPath == null || agent.currentPath.Length == 0)
        {
            agent.ChangeState(new SearchState());
            return;
        }

        if (agent.pathIndex >= agent.currentPath.Length)
        {
            agent.ChangeState(new GoalState());
            return;
        }

        Vector3 target = agent.currentPath[agent.pathIndex];
        agent.MoveTo(target);

        float dist = Vector3.Distance(agent.transform.position, target);

        //if (dist < 0.05f)
        //{
        //    agent.transform.position = target;
        //    agent.pathIndex++;
        //    if (agent.pathIndex >= agent.currentPath.Length)
        //    {
        //        agent.ChangeState(new GoalState());
        //        return;
        //    }
        //}
        // --- 経路上の次ノードに近づいたとき ---
        if (dist < 0.05f)
        {
            agent.transform.position = target;
            agent.pathIndex++;

            // 最終ノードに達していないなら次へ
            if (agent.pathIndex < agent.currentPath.Length)
                return;

            // --- 経路終端 → ゴール距離判定 ---
            float goalDist = Vector3.Distance(agent.transform.position, agent.GetGoalPosition());
            if (goalDist < 0.5f) // ← ゴール判定閾値を実距離に変更
            {
                agent.ChangeState(new GoalState());
                return;
            }
            else
            {
                Debug.LogWarning($"[MoveState] {agent.name}: 終端だがGoal未到達 (距離 {goalDist:F2})");
                agent.ChangeState(new SearchState()); // 経路外れ対策
                return;
            }
        }

        // --- 停止検知（動けていない場合は再探索へ） ---
        if (Vector3.Distance(agent.transform.position, lastPosition) < 0.001f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 1.0f)
            {
                Debug.LogWarning($"[MoveState] {agent.name} 移動停止検出 → SearchStateへ");
                agent.ChangeState(new SearchState());
                stuckTimer = 0f;
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


//using UnityEngine;

//public class MoveState : AgentBaseState
//{
//    private float attackCooldown = 0f;

//    public override void Enter(AgentController agent)
//    {
//        LogAgentStatus(agent, "Moving");

//        // 経路が無効な場合
//        if (agent.currentPath == null || agent.currentPath.Length == 0)
//        {
//            Debug.LogWarning($"[MoveState] {agent.name}: currentPath is null or empty → SearchStateへ");
//            agent.ChangeState(new SearchState());
//            return;
//        }

//        // 経路が短すぎる（＝再探索エラーやGoal重複など）
//        if (agent.currentPath.Length <= 1)
//        {
//            Debug.LogWarning($"[MoveState] {agent.name}: path length={agent.currentPath.Length} → GoalStateスキップ");
//            return;
//        }

//        // 初期ノードが自分の位置ならスキップ
//        float startDist = Vector3.Distance(agent.transform.position, agent.currentPath[0]);
//        if (startDist < 0.05f)
//        {
//            agent.pathIndex = 1;
//            Debug.Log($"[MoveState] {agent.name}: start node skipped (pathIndex=1, dist={startDist:F3})");
//        }
//        else
//        {
//            Debug.Log($"[MoveState] {agent.name}: startDist={startDist:F3}, starting from index 0");
//        }

//        Debug.Log($"[MoveState] {agent.name}: Path length={agent.currentPath.Length}, pathIndex={agent.pathIndex}");
//    }

//    public override void Execute(AgentController agent)
//    {
//        attackCooldown -= Time.deltaTime;

//        // 経路安全ガード
//        if (agent.currentPath == null || agent.currentPath.Length == 0)
//        {
//            Debug.LogWarning($"[MoveState] {agent.name}: No path, switching to SearchState");
//            agent.ChangeState(new SearchState());
//            return;
//        }

//        // 1ノードだけなら停止（再探索を待つ）
//        if (agent.currentPath.Length <= 1)
//        {
//            Debug.LogWarning($"[MoveState] {agent.name}: path length=1 → 移動停止 (再探索待機)");
//            return;
//        }

//        // index補正
//        if (agent.pathIndex < 0) agent.pathIndex = 0;
//        if (agent.pathIndex > agent.currentPath.Length - 1)
//        {
//            Debug.Log($"[MoveState] {agent.name}: index={agent.pathIndex} >= len={agent.currentPath.Length} → GoalStateへ");
//            agent.ChangeState(new GoalState());
//            return;
//        }

//        // 現在の目的地
//        Vector3 target = agent.currentPath[agent.pathIndex];
//        target.y = 0f;
//        Vector3 currentPos = agent.transform.position;
//        currentPos.y = 0f;

//        // 移動実行
//        agent.MoveTo(target);

//        float dist = Vector3.Distance(agent.transform.position, target);

//        if (Time.frameCount % 30 == 0)
//        {
//            Debug.Log($"[MoveState] {agent.name}: idx={agent.pathIndex}/{agent.currentPath.Length - 1}, dist={dist:F3}");
//        }

//        // 経路点に到達したら次へ
//        if (dist < 0.05f)
//        {
//            agent.transform.position = target;
//            agent.pathIndex++;

//            Debug.Log($"[MoveState] {agent.name}: reached waypoint, next index={agent.pathIndex}");

//            if (agent.pathIndex > agent.currentPath.Length - 1)
//            {
//                Debug.Log($"[MoveState] {agent.name}: 全waypoint完了 → GoalStateへ");
//                agent.ChangeState(new GoalState());
//                return;
//            }
//        }

//        // Heatmap / CSVログ（移動時のみ）
//        Vector3 afterMove = agent.transform.position;
//        afterMove.y = 0f;

//        if (Vector3.Distance(currentPos, afterMove) > 0.001f)
//        {
//            HeatmapManager.Instance?.RecordPosition(afterMove);
//            CSVLogger.Log(
//                "Agent",
//                agent.name,
//                "MoveState",
//                "Moving",
//                "Goal",
//                agent.GetGoalPosition(),
//                currentPos,
//                afterMove,
//                agent.GetData().aiType.ToString()
//            );
//        }
//    }

//    public override void Exit(AgentController agent)
//    {
//        Debug.Log($"[MoveState] {agent.name}: Exit");
//    }
//}


////using UnityEngine;

////public class MoveState : AgentBaseState
////{
////    private float attackCooldown = 0f;

////    public override void Enter(AgentController agent)
////    {
////        LogAgentStatus(agent, "Moving");

////        // 初期ノードが自分の位置ならスキップ
////        if (agent.currentPath != null && agent.currentPath.Length > 1)
////        {
////            float startDist = Vector3.Distance(agent.transform.position, agent.currentPath[0]);
////            if (startDist < 0.05f)
////            {
////                agent.pathIndex = 1;
////                Debug.Log($"[MoveState] {agent.name}: start node skipped (pathIndex=1, dist={startDist:F3})");
////            }
////            else
////            {
////                Debug.Log($"[MoveState] {agent.name}: startDist={startDist:F3}, starting from index 0");
////            }
////        }

////        // デバッグ: 経路情報を表示
////        if (agent.currentPath != null)
////        {
////            Debug.Log($"[MoveState] {agent.name}: Path length={agent.currentPath.Length}, pathIndex={agent.pathIndex}");
////        }
////    }

////    public override void Execute(AgentController agent)
////    {
////        attackCooldown -= Time.deltaTime;

////        // --- 経路がない場合は再探索へ ---
////        if (agent.currentPath == null || agent.currentPath.Length == 0)
////        {
////            Debug.LogWarning($"[MoveState] {agent.name}: No path, switching to SearchState");
////            agent.ChangeState(new SearchState());
////            return;
////        }

////        // --- index補正 ---
////        if (agent.pathIndex < 0)
////        {
////            agent.pathIndex = 0;
////            Debug.LogWarning($"[MoveState] {agent.name}: pathIndex was negative, reset to 0");
////        }

////        if (agent.pathIndex >= agent.currentPath.Length)
////        {
////            Debug.Log($"[MoveState] {agent.name}: Reached end of path (index={agent.pathIndex}, length={agent.currentPath.Length})");
////            //agent.ChangeState(new GoalState());
////            //return;
////        }

////        // --- 現在の目的地 ---
////        Vector3 target = agent.currentPath[agent.pathIndex];
////        target.y = 0f;

////        Vector3 currentPos = agent.transform.position;
////        currentPos.y = 0f;

////        // --- 到達チェック前にMove ---
////        agent.MoveTo(target);

////        // --- 到達判定 ---
////        float dist = Vector3.Distance(agent.transform.position, target);

////        // デバッグ: 移動状況を表示
////        if (Time.frameCount % 30 == 0) // 0.5秒ごとくらい
////        {
////            Debug.Log($"[MoveState] {agent.name}: index={agent.pathIndex}/{agent.currentPath.Length}, dist to target={dist:F3}");
////        }

////        if (dist < 0.05f)
////        {
////            // 正確に目的地に配置
////            Vector3 snapPos = target;
////            snapPos.y = agent.transform.position.y; // Y座標は維持
////            agent.transform.position = snapPos;

////            agent.pathIndex++;

////            Debug.Log($"[MoveState] {agent.name}: Reached waypoint {agent.pathIndex - 1}, moving to index {agent.pathIndex}");

////            // ゴールチェック
////            if (agent.pathIndex >= agent.currentPath.Length)
////            {
////                Debug.Log($"[MoveState] {agent.name}: All waypoints completed, switching to GoalState");
////                agent.ChangeState(new GoalState());
////                return;
////            }
////        }

////        // --- CSV/Heatmap記録 ---
////        Vector3 beforeMove = currentPos;
////        Vector3 afterMove = agent.transform.position;
////        afterMove.y = 0f;

////        // 実際に移動した場合のみ記録
////        if (Vector3.Distance(beforeMove, afterMove) > 0.001f)
////        {
////            // Heatmap記録
////            if (HeatmapManager.Instance != null)
////            {
////                HeatmapManager.Instance.RecordPosition(afterMove);
////            }

////            // CSV記録
////            CSVLogger.Log(
////                "Agent",
////                agent.name,
////                "MoveState",
////                "Moving",
////                "Goal",
////                agent.GetGoalPosition(),
////                beforeMove,
////                afterMove,
////                agent.GetData().aiType.ToString()
////            );
////        }
////    }

////    public override void Exit(AgentController agent)
////    {
////        Debug.Log($"[MoveState] {agent.name}: Exiting MoveState");
////    }
////}

//////using UnityEngine;

//////public class MoveState : AgentBaseState
//////{
//////    private float attackCooldown = 0f;

//////    public override void Enter(AgentController agent)
//////    {
//////        LogAgentStatus(agent, "Moving");

//////        // ★ 最初のノードが自分の位置ならスキップ
//////        if (agent.currentPath != null && agent.currentPath.Length > 1)
//////        {
//////            float startDist = Vector3.Distance(agent.transform.position, agent.currentPath[0]);
//////            if (startDist < 0.05f)
//////            {
//////                agent.pathIndex = 1;
//////                Debug.Log($"[MoveState] {agent.name}: start node skipped (pathIndex=1, dist={startDist:F3})");
//////            }
//////            else
//////            {
//////                Debug.Log($"[MoveState] {agent.name}: startDist={startDist:F3} (no skip)");
//////            }
//////        }
//////    }

//////    public override void Execute(AgentController agent)
//////    {
//////        attackCooldown -= Time.deltaTime;

//////        // --- 経路がない場合は再探索へ ---
//////        if (agent.currentPath == null || agent.currentPath.Length == 0)
//////        {
//////            agent.ChangeState(new SearchState());
//////            return;
//////        }

//////        // --- index補正 ---
//////        if (agent.pathIndex < 0) agent.pathIndex = 0;
//////        if (agent.pathIndex >= agent.currentPath.Length)
//////        {
//////            agent.ChangeState(new GoalState());
//////            return;
//////        }

//////        // --- 現在の目的地 ---
//////        Vector3 target = agent.currentPath[agent.pathIndex];
//////        target.y = 0f;

//////        // --- 到達チェック前にMove ---
//////        agent.MoveTo(target);

//////        // --- 到達判定 ---
//////        float dist = Vector3.Distance(agent.transform.position, target);
//////        if (dist < 0.05f)
//////        {
//////            agent.transform.position = target;
//////            agent.pathIndex++;

//////            if (agent.pathIndex >= agent.currentPath.Length)
//////            {
//////                agent.ChangeState(new GoalState());
//////                return;
//////            }
//////        }
//////    }

//////    public override void Exit(AgentController agent) { }
//////}


////////using UnityEngine;

////////public class MoveState : AgentBaseState
////////{
////////    private float attackCooldown = 0f;

////////    public override void Enter(AgentController agent)
////////    {
////////        LogAgentStatus(agent, "Moving");

////////        /// ★ 初期ノードが自分の位置とほぼ同じ場合はスキップ
////////        if (agent.currentPath != null && agent.currentPath.Length > 1)
////////        {
////////            if (Vector3.Distance(agent.transform.position, agent.currentPath[0]) < 0.1f)
////////            {
////////                agent.pathIndex = 1;
////////                Debug.Log($"[MoveState] {agent.name}: 最初のノードをスキップしました (pathIndex=1)");
////////            }
////////        }
////////    }

////////    public override void Execute(AgentController agent)
////////    {
////////        if (agent.pathIndex >= agent.currentPath.Length)
////////        {
////////            agent.ChangeState(new GoalState());
////////            return;
////////        }

////////        attackCooldown -= Time.deltaTime;

////////        // =========================================================
////////        // ① 経路の安全ガード
////////        // =========================================================
////////        if (agent.currentPath == null || agent.currentPath.Length == 0)
////////        {
////////            agent.ChangeState(new SearchState());
////////            return;
////////        }

////////        if (agent.pathIndex < 0) agent.pathIndex = 0;
////////        if (agent.pathIndex >= agent.currentPath.Length)
////////        {
////////            agent.ChangeState(new GoalState());
////////            return;
////////        }

////////        // =========================================================
////////        // ② 攻撃範囲チェック
////////        // =========================================================
////////        MonoBehaviour nearest = FindNearestEnemy(agent);
////////        if (nearest != null)
////////        {
////////            float dist = Vector3.Distance(agent.transform.position, ((Component)nearest).transform.position);
////////            if (dist <= agent.GetData().attackRange && attackCooldown <= 0f)
////////            {
////////                // スナップ補正
////////                agent.transform.position = new Vector3(
////////                    Mathf.Round(agent.transform.position.x * 100f) / 100f,
////////                    agent.transform.position.y,
////////                    Mathf.Round(agent.transform.position.z * 100f) / 100f
////////                );

////////                agent.currentPath = null;
////////                agent.pathIndex = 0;
////////                agent.ChangeState(new AttackState());
////////                return;
////////            }
////////        }

////////        // =========================================================
////////        // ③ 経路再探索要求
////////        // =========================================================
////////        if (agent.needReplan)
////////        {
////////            agent.needReplan = false;
////////            agent.ChangeState(new SearchState());
////////            return;
////////        }

////////        // =========================================================
////////        // ④ 移動速度が0なら停止
////////        // =========================================================
////////        if (agent.GetData().moveSpeed <= 0f)
////////            return;

////////        // =========================================================
////////        // ⑤ 経路に沿って移動
////////        // =========================================================
////////        Vector3 target = agent.currentPath[agent.pathIndex];
////////        target.y = 0f;
////////        Vector3 beforeMove = agent.transform.position;

////////        agent.MoveTo(target);

////////        // 経路点に到達したら次へ
////////        if (Vector3.Distance(agent.transform.position, target) < 0.01f)
////////        {
////////            agent.transform.position = target;
////////            agent.pathIndex++;

////////            // ゴールチェック
////////            if (agent.pathIndex >= agent.currentPath.Length)
////////            {
////////                agent.ChangeState(new GoalState());
////////                return;
////////            }
////////        }

////////        // =========================================================
////////        // ⑥ CSVログ出力（移動解析用）
////////        // =========================================================
////////        CSVLogger.Log(
////////            "Agent",
////////            agent.name,
////////            "MoveState",
////////            "Moving",
////////            "Goal",
////////            agent.GetGoalPosition(),
////////            beforeMove,
////////            agent.GetNextPosition(),
////////            agent.GetData().aiType.ToString()
////////        );
////////    }

////////    public override void Exit(AgentController agent) { }

////////    // =========================================================
////////    // 最寄りの敵（Ally / Agent）を探索
////////    // =========================================================
////////    private MonoBehaviour FindNearestEnemy(AgentController self)
////////    {
////////        float bestDist = float.MaxValue;
////////        MonoBehaviour nearest = null;

////////        AllyController[] allies = Object.FindObjectsByType<AllyController>(FindObjectsSortMode.None);
////////        foreach (var ally in allies)
////////        {
////////            float d = Vector3.Distance(self.transform.position, ally.transform.position);
////////            if (d < bestDist)
////////            {
////////                bestDist = d;
////////                nearest = ally;
////////            }
////////        }

////////        AgentController[] agents = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
////////        foreach (var agent in agents)
////////        {
////////            if (agent == self) continue;
////////            float d = Vector3.Distance(self.transform.position, agent.transform.position);
////////            if (d < bestDist)
////////            {
////////                bestDist = d;
////////                nearest = agent;
////////            }
////////        }

////////        return nearest;
////////    }
////////}


//////////using UnityEngine;

//////////public class MoveState : AgentBaseState
//////////{
//////////    private float attackCooldown = 0f;

//////////    public override void Enter(AgentController agent)
//////////    {
//////////        LogAgentStatus(agent, "Moving");
//////////    }

//////////    public override void Execute(AgentController agent)
//////////    {
//////////        if (agent.currentPath != null && agent.currentPath.Length > 1)
//////////        {
//////////            // 最初の経路点が自分の位置とほぼ同じならスキップ
//////////            if (Vector3.Distance(agent.transform.position, agent.currentPath[0]) < 0.05f)
//////////            {
//////////                agent.pathIndex = 1;
//////////            }
//////////        }

//////////        attackCooldown -= Time.deltaTime;

//////////        // =========================================================
//////////        // ① 経路の安全ガード（Null / 空 / 範囲外）
//////////        // =========================================================
//////////        if (agent.currentPath == null || agent.currentPath.Length == 0)
//////////        {
//////////            agent.ChangeState(new SearchState());
//////////            return;
//////////        }

//////////        if (agent.pathIndex < 0 || agent.pathIndex >= agent.currentPath.Length)
//////////        {
//////////            agent.ChangeState(new GoalState());
//////////            return;
//////////        }

//////////        // =========================================================
//////////        // ② 攻撃範囲チェック
//////////        // =========================================================
//////////        MonoBehaviour nearest = FindNearestEnemy(agent);
//////////        if (nearest != null)
//////////        {
//////////            float dist = Vector3.Distance(agent.transform.position, ((Component)nearest).transform.position);
//////////            if (dist <= agent.GetData().attackRange && attackCooldown <= 0f)
//////////            {
//////////                // スナップ補正
//////////                agent.transform.position = new Vector3(
//////////                    Mathf.Round(agent.transform.position.x * 100f) / 100f,
//////////                    agent.transform.position.y,
//////////                    Mathf.Round(agent.transform.position.z * 100f) / 100f
//////////                );

//////////                agent.currentPath = null;
//////////                agent.pathIndex = 0;
//////////                agent.ChangeState(new AttackState());
//////////                return;
//////////            }
//////////        }

//////////        // =========================================================
//////////        // ③ 経路再探索要求がある場合
//////////        // =========================================================
//////////        if (agent.needReplan)
//////////        {
//////////            agent.needReplan = false;
//////////            agent.ChangeState(new SearchState());
//////////            return;
//////////        }

//////////        // =========================================================
//////////        // ④ 速度0なら静止
//////////        // =========================================================
//////////        if (agent.GetData().moveSpeed <= 0f)
//////////            return;

//////////        // =========================================================
//////////        // ⑤ 経路に沿って移動（修正版）
//////////        // =========================================================
//////////        Vector3 target = agent.currentPath[agent.pathIndex];
//////////        target.y = 0f;
//////////        Vector3 beforeMove = agent.transform.position;

//////////        agent.MoveTo(target);

//////////        // 経路点に到達したら次へ
//////////        if (Vector3.Distance(agent.transform.position, target) < 0.01f)
//////////        {
//////////            agent.transform.position = target;
//////////            agent.pathIndex++;

//////////            // ゴールチェック
//////////            if (agent.pathIndex >= agent.currentPath.Length)
//////////            {
//////////                agent.ChangeState(new GoalState());
//////////                return;
//////////            }
//////////        }

//////////        // =========================================================
//////////        // ⑥ CSVログ出力（移動解析用）
//////////        // =========================================================
//////////        CSVLogger.Log(
//////////            "Agent",
//////////            agent.name,
//////////            "MoveState",
//////////            "Moving",
//////////            "Goal",
//////////            agent.GetGoalPosition(),
//////////            beforeMove,
//////////            agent.GetNextPosition(),
//////////            agent.GetData().aiType.ToString()
//////////        );
//////////    }

//////////    public override void Exit(AgentController agent) { }

//////////    // =========================================================
//////////    // 最寄りの敵（Ally / Agent）を探索
//////////    // =========================================================
//////////    private MonoBehaviour FindNearestEnemy(AgentController self)
//////////    {
//////////        float bestDist = float.MaxValue;
//////////        MonoBehaviour nearest = null;

//////////        AllyController[] allies = Object.FindObjectsByType<AllyController>(FindObjectsSortMode.None);
//////////        foreach (var ally in allies)
//////////        {
//////////            float d = Vector3.Distance(self.transform.position, ally.transform.position);
//////////            if (d < bestDist)
//////////            {
//////////                bestDist = d;
//////////                nearest = ally;
//////////            }
//////////        }

//////////        AgentController[] agents = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
//////////        foreach (var agent in agents)
//////////        {
//////////            if (agent == self) continue;
//////////            float d = Vector3.Distance(self.transform.position, agent.transform.position);
//////////            if (d < bestDist)
//////////            {
//////////                bestDist = d;
//////////                nearest = agent;
//////////            }
//////////        }

//////////        return nearest;
//////////    }
//////////}


////////////using UnityEngine;

////////////public class MoveState : AgentBaseState
////////////{
////////////    private float attackCooldown = 0f;

////////////    public override void Enter(AgentController agent)
////////////    {
////////////        //Debug.Log($"[{agent.name}] MoveState: 開始");
////////////    }

////////////    public override void Execute(AgentController agent)
////////////    {
////////////        LogAgentStatus(agent, "Moving"); // ← Attacking ではなく Moving に変更

////////////        attackCooldown -= Time.deltaTime;

////////////        // ----------------------------
////////////        // ① 攻撃範囲チェック
////////////        // ----------------------------
////////////        MonoBehaviour nearest = FindNearestEnemy(agent);
////////////        if (nearest != null)
////////////        {
////////////            float dist = Vector3.Distance(agent.transform.position, ((Component)nearest).transform.position);
////////////            if (dist <= agent.GetData().attackRange && attackCooldown <= 0f)
////////////            {
////////////                agent.transform.position = new Vector3(
////////////                    Mathf.Round(agent.transform.position.x * 100f) / 100f,
////////////                    agent.transform.position.y,
////////////                    Mathf.Round(agent.transform.position.z * 100f) / 100f
////////////                );

////////////                agent.currentPath = null;
////////////                agent.pathIndex = 0;
////////////                agent.ChangeState(new AttackState());
////////////                return;
////////////            }
////////////        }

////////////        // ----------------------------
////////////        // ② 経路再探索チェック
////////////        // ----------------------------
////////////        if (agent.needReplan)
////////////        {
////////////            agent.needReplan = false;
////////////            agent.ChangeState(new SearchState());
////////////            return;
////////////        }

////////////        // ----------------------------
////////////        // ③ 経路未設定なら探索へ
////////////        // ----------------------------
////////////        if (agent.currentPath == null || agent.currentPath.Length == 0)
////////////        {
////////////            agent.ChangeState(new SearchState());
////////////            return;
////////////        }

////////////        // ----------------------------
////////////        // ④ 移動速度が0（攻撃中）なら動かない
////////////        // ----------------------------
////////////        if (agent.GetData().moveSpeed <= 0f)
////////////            return;

////////////        //// ----------------------------
////////////        //// ⑤ 経路に沿って移動（修正版）
////////////        //// ----------------------------
////////////        //Vector3 target = agent.currentPath[agent.pathIndex];
////////////        //target.y = 0f;

////////////        //// ★ ここを自前のMoveTowardsではなく、AgentController.MoveTo()に変更！
////////////        //agent.MoveTo(target);

////////////        //// 到達判定
////////////        //if (Vector3.Distance(agent.transform.position, target) < 0.01f)
////////////        //{
////////////        //    agent.transform.position = target;
////////////        //    agent.pathIndex++;

////////////        //    if (agent.pathIndex >= agent.currentPath.Length)
////////////        //    {
////////////        //        agent.ChangeState(new GoalState());
////////////        //        return;
////////////        //    }
////////////        //}

////////////        //// ----------------------------
////////////        //// ⑥ Heatmap記録
////////////        //// ----------------------------
////////////        //HeatmapManager.Instance.RecordPosition(agent.transform.position);

////////////        //// ----------------------------
////////////        //// ⑦ CSVログ出力（MoveTo後なのでNextが更新済み）
////////////        //// ----------------------------
////////////        //CSVLogger.Log(
////////////        //    "Agent",
////////////        //    agent.name,
////////////        //    "MoveState",
////////////        //    "Moving",
////////////        //    "Goal",
////////////        //    agent.GetGoalPosition(),        // Target
////////////        //    agent.transform.position,       // Current（現フレーム位置）
////////////        //    agent.GetNextPosition(),        // Next（MoveToで更新された次フレーム位置）
////////////        //    agent.GetData().aiType.ToString()
////////////        //);
////////////        // ----------------------------
////////////        // ⑤ 経路に沿って移動
////////////        // ----------------------------
////////////        Vector3 target = agent.currentPath[agent.pathIndex];
////////////        target.y = 0f;

////////////        // ★ ここで移動前の位置を記録（Current）
////////////        Vector3 beforeMove = agent.transform.position;

////////////        // ★ 移動実行（内部で nextPosition を更新）
////////////        agent.MoveTo(target);

////////////        // 経路点に到達したら次へ
////////////        if (Vector3.Distance(agent.transform.position, target) < 0.01f)
////////////        {
////////////            agent.transform.position = target;
////////////            agent.pathIndex++;

////////////            if (agent.pathIndex >= agent.currentPath.Length)
////////////            {
////////////                agent.ChangeState(new GoalState());
////////////                return;
////////////            }
////////////        }

////////////        // ----------------------------
////////////        // ⑦ CSVログ出力（移動前→移動後）
////////////        // ----------------------------
////////////        //CSVLogger.Log(
////////////        //    "Agent",                          // type
////////////        //    agent.name,                       // name (＝Object列)
////////////        //    "MoveState",                      // state
////////////        //    "Moving",                         // action
////////////        //    "Goal",                           // target 名
////////////        //    agent.GetGoalPosition(),          // targetPos (Vector3)
////////////        //    beforeMove,                       // currentPos (移動前)
////////////        //    agent.GetNextPosition(),          // nextPos (移動後)
////////////        //    agent.GetData().aiType.ToString() // aiType
////////////        //);
////////////        //CSVLogger.Log(
////////////        //    agent.name,                       // ← Type列（AgentPrefab）※第1引数=type
////////////        //    "MoveState",                      // ← Object列（MoveState）※第2引数=name
////////////        //    "Moving",                         // ← State列
////////////        //    agent.GetData().aiType.ToString(),// ← Action列（DStar）
////////////        //    "Goal",                           // Target名
////////////        //    agent.GetGoalPosition(),          // TargetX/Y/Z
////////////        //    beforeMove,                       // CurrentX/Y/Z（移動前）
////////////        //    agent.GetNextPosition(),          // NextX/Y/Z（移動後）
////////////        //    agent.GetData().aiType.ToString() // AIType列（DStar）
////////////        //);
////////////        CSVLogger.Log(
////////////            "Agent",
////////////            agent.name,
////////////            "MoveState",
////////////            "Moving",
////////////            "Goal",                           
////////////            agent.GetGoalPosition(),          // TargetX/Y/Z
////////////            beforeMove,                       // CurrentX/Y/Z（移動前）
////////////            agent.GetNextPosition(),          // NextX/Y/Z（移動後）
////////////            agent.GetData().aiType.ToString() // AIType列（DStar）
////////////        );

////////////    }

////////////    public override void Exit(AgentController agent)
////////////    {
////////////        //Debug.Log($"[{agent.name}] MoveState: 終了");
////////////    }

////////////    private MonoBehaviour FindNearestEnemy(AgentController self)
////////////    {
////////////        float bestDist = float.MaxValue;
////////////        MonoBehaviour nearest = null;

////////////        AllyController[] allies = Object.FindObjectsByType<AllyController>(FindObjectsSortMode.None);
////////////        foreach (var ally in allies)
////////////        {
////////////            float d = Vector3.Distance(self.transform.position, ally.transform.position);
////////////            if (d < bestDist)
////////////            {
////////////                bestDist = d;
////////////                nearest = ally;
////////////            }
////////////        }

////////////        AgentController[] agents = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
////////////        foreach (var agent in agents)
////////////        {
////////////            if (agent == self) continue;
////////////            float d = Vector3.Distance(self.transform.position, agent.transform.position);
////////////            if (d < bestDist)
////////////            {
////////////                bestDist = d;
////////////                nearest = agent;
////////////            }
////////////        }

////////////        return nearest;
////////////    }
////////////}


//////////////using UnityEngine;

//////////////public class MoveState : AgentBaseState
//////////////{
//////////////    private float attackCooldown = 0f;

//////////////    public override void Enter(AgentController agent)
//////////////    {
//////////////        //Debug.Log($"[{agent.name}] MoveState: 開始");
//////////////    }

//////////////    public override void Execute(AgentController agent)
//////////////    {
//////////////        LogAgentStatus(agent, "Attacking");

//////////////        attackCooldown -= Time.deltaTime;

//////////////        // ----------------------------
//////////////        // ① 攻撃範囲チェック
//////////////        // ----------------------------
//////////////        MonoBehaviour nearest = FindNearestEnemy(agent);
//////////////        if (nearest != null)
//////////////        {
//////////////            float dist = Vector3.Distance(agent.transform.position, ((Component)nearest).transform.position);

//////////////            // 攻撃範囲内なら AttackState へ遷移して停止
//////////////            if (dist <= agent.GetData().attackRange && attackCooldown <= 0f)
//////////////            {
//////////////                // --- 位置を軽くスナップしてブレを防止 ---
//////////////                agent.transform.position = new Vector3(
//////////////                    Mathf.Round(agent.transform.position.x * 100f) / 100f,
//////////////                    agent.transform.position.y,
//////////////                    Mathf.Round(agent.transform.position.z * 100f) / 100f
//////////////                );

//////////////                // --- 経路破棄＆移動停止 ---
//////////////                agent.currentPath = null;
//////////////                agent.pathIndex = 0;

//////////////                // --- AttackStateへ遷移（このフレームでは動かない） ---
//////////////                agent.ChangeState(new AttackState());
//////////////                return;
//////////////            }
//////////////        }

//////////////        // ----------------------------
//////////////        // ② 経路再探索チェック
//////////////        // ----------------------------
//////////////        if (agent.needReplan)
//////////////        {
//////////////            agent.needReplan = false;
//////////////            agent.ChangeState(new SearchState());
//////////////            return;
//////////////        }

//////////////        // ----------------------------
//////////////        // ③ 経路未設定なら探索へ
//////////////        // ----------------------------
//////////////        if (agent.currentPath == null || agent.currentPath.Length == 0)
//////////////        {
//////////////            agent.ChangeState(new SearchState());
//////////////            return;
//////////////        }

//////////////        // ----------------------------
//////////////        // ④ 移動速度が0（攻撃中）なら動かない
//////////////        // ----------------------------
//////////////        if (agent.GetData().moveSpeed <= 0f)
//////////////            return;

//////////////        // ----------------------------
//////////////        // ⑤ 経路に沿って移動
//////////////        // ----------------------------
//////////////        Vector3 target = agent.currentPath[agent.pathIndex];
//////////////        Vector3 current = agent.transform.position;
//////////////        target.y = current.y = 0f;

//////////////        agent.transform.position = Vector3.MoveTowards(
//////////////            current,
//////////////            target,
//////////////            agent.GetData().moveSpeed * Time.deltaTime
//////////////        );

//////////////        // 経路点に到達したら次へ
//////////////        if (Vector3.Distance(agent.transform.position, target) < 0.01f)
//////////////        {
//////////////            agent.transform.position = target;
//////////////            agent.pathIndex++;

//////////////            if (agent.pathIndex >= agent.currentPath.Length)
//////////////            {
//////////////                agent.ChangeState(new GoalState());
//////////////                return;
//////////////            }
//////////////        }

//////////////        // ----------------------------
//////////////        // ⑥ Heatmap記録
//////////////        // ----------------------------
//////////////        HeatmapManager.Instance.RecordPosition(agent.transform.position);
//////////////    }

//////////////    public override void Exit(AgentController agent)
//////////////    {
//////////////        //Debug.Log($"[{agent.name}] MoveState: 終了");
//////////////    }

//////////////    // ----------------------------
//////////////    // 最寄りの敵を探す（Agent + Ally両対応）
//////////////    // ----------------------------
//////////////    private MonoBehaviour FindNearestEnemy(AgentController self)
//////////////    {
//////////////        float bestDist = float.MaxValue;
//////////////        MonoBehaviour nearest = null;

//////////////        // --- Ally（味方）を敵として検索 ---
//////////////        AllyController[] allies = Object.FindObjectsByType<AllyController>(FindObjectsSortMode.None);
//////////////        foreach (var ally in allies)
//////////////        {
//////////////            float d = Vector3.Distance(self.transform.position, ally.transform.position);
//////////////            if (d < bestDist)
//////////////            {
//////////////                bestDist = d;
//////////////                nearest = ally;
//////////////            }
//////////////        }

//////////////        // --- 他のAgent（自分以外）も含める ---
//////////////        AgentController[] agents = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
//////////////        foreach (var agent in agents)
//////////////        {
//////////////            if (agent == self) continue;
//////////////            float d = Vector3.Distance(self.transform.position, agent.transform.position);
//////////////            if (d < bestDist)
//////////////            {
//////////////                bestDist = d;
//////////////                nearest = agent;
//////////////            }
//////////////        }

//////////////        return nearest;
//////////////    }
//////////////}