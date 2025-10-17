using UnityEngine;

public class MoveState : AgentBaseState
{
    private float attackCooldown = 0f;

    public override void Enter(AgentController agent)
    {
        //Debug.Log($"[{agent.name}] MoveState: 開始");
    }

    public override void Execute(AgentController agent)
    {
        LogAgentStatus(agent, "Moving"); // ← Attacking ではなく Moving に変更

        attackCooldown -= Time.deltaTime;

        // ----------------------------
        // ① 攻撃範囲チェック
        // ----------------------------
        MonoBehaviour nearest = FindNearestEnemy(agent);
        if (nearest != null)
        {
            float dist = Vector3.Distance(agent.transform.position, ((Component)nearest).transform.position);
            if (dist <= agent.GetData().attackRange && attackCooldown <= 0f)
            {
                agent.transform.position = new Vector3(
                    Mathf.Round(agent.transform.position.x * 100f) / 100f,
                    agent.transform.position.y,
                    Mathf.Round(agent.transform.position.z * 100f) / 100f
                );

                agent.currentPath = null;
                agent.pathIndex = 0;
                agent.ChangeState(new AttackState());
                return;
            }
        }

        // ----------------------------
        // ② 経路再探索チェック
        // ----------------------------
        if (agent.needReplan)
        {
            agent.needReplan = false;
            agent.ChangeState(new SearchState());
            return;
        }

        // ----------------------------
        // ③ 経路未設定なら探索へ
        // ----------------------------
        if (agent.currentPath == null || agent.currentPath.Length == 0)
        {
            agent.ChangeState(new SearchState());
            return;
        }

        // ----------------------------
        // ④ 移動速度が0（攻撃中）なら動かない
        // ----------------------------
        if (agent.GetData().moveSpeed <= 0f)
            return;

        //// ----------------------------
        //// ⑤ 経路に沿って移動（修正版）
        //// ----------------------------
        //Vector3 target = agent.currentPath[agent.pathIndex];
        //target.y = 0f;

        //// ★ ここを自前のMoveTowardsではなく、AgentController.MoveTo()に変更！
        //agent.MoveTo(target);

        //// 到達判定
        //if (Vector3.Distance(agent.transform.position, target) < 0.01f)
        //{
        //    agent.transform.position = target;
        //    agent.pathIndex++;

        //    if (agent.pathIndex >= agent.currentPath.Length)
        //    {
        //        agent.ChangeState(new GoalState());
        //        return;
        //    }
        //}

        //// ----------------------------
        //// ⑥ Heatmap記録
        //// ----------------------------
        //HeatmapManager.Instance.RecordPosition(agent.transform.position);

        //// ----------------------------
        //// ⑦ CSVログ出力（MoveTo後なのでNextが更新済み）
        //// ----------------------------
        //CSVLogger.Log(
        //    "Agent",
        //    agent.name,
        //    "MoveState",
        //    "Moving",
        //    "Goal",
        //    agent.GetGoalPosition(),        // Target
        //    agent.transform.position,       // Current（現フレーム位置）
        //    agent.GetNextPosition(),        // Next（MoveToで更新された次フレーム位置）
        //    agent.GetData().aiType.ToString()
        //);
        // ----------------------------
        // ⑤ 経路に沿って移動
        // ----------------------------
        Vector3 target = agent.currentPath[agent.pathIndex];
        target.y = 0f;

        // ★ ここで移動前の位置を記録（Current）
        Vector3 beforeMove = agent.transform.position;

        // ★ 移動実行（内部で nextPosition を更新）
        agent.MoveTo(target);

        // 経路点に到達したら次へ
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

        // ----------------------------
        // ⑦ CSVログ出力（移動前→移動後）
        // ----------------------------
        //CSVLogger.Log(
        //    "Agent",                          // type
        //    agent.name,                       // name (＝Object列)
        //    "MoveState",                      // state
        //    "Moving",                         // action
        //    "Goal",                           // target 名
        //    agent.GetGoalPosition(),          // targetPos (Vector3)
        //    beforeMove,                       // currentPos (移動前)
        //    agent.GetNextPosition(),          // nextPos (移動後)
        //    agent.GetData().aiType.ToString() // aiType
        //);
        //CSVLogger.Log(
        //    agent.name,                       // ← Type列（AgentPrefab）※第1引数=type
        //    "MoveState",                      // ← Object列（MoveState）※第2引数=name
        //    "Moving",                         // ← State列
        //    agent.GetData().aiType.ToString(),// ← Action列（DStar）
        //    "Goal",                           // Target名
        //    agent.GetGoalPosition(),          // TargetX/Y/Z
        //    beforeMove,                       // CurrentX/Y/Z（移動前）
        //    agent.GetNextPosition(),          // NextX/Y/Z（移動後）
        //    agent.GetData().aiType.ToString() // AIType列（DStar）
        //);
        CSVLogger.Log(
            "Agent",
            agent.name,
            "MoveState",
            "Moving",
            "Goal",                           
            agent.GetGoalPosition(),          // TargetX/Y/Z
            beforeMove,                       // CurrentX/Y/Z（移動前）
            agent.GetNextPosition(),          // NextX/Y/Z（移動後）
            agent.GetData().aiType.ToString() // AIType列（DStar）
        );

    }

    public override void Exit(AgentController agent)
    {
        //Debug.Log($"[{agent.name}] MoveState: 終了");
    }

    private MonoBehaviour FindNearestEnemy(AgentController self)
    {
        float bestDist = float.MaxValue;
        MonoBehaviour nearest = null;

        AllyController[] allies = Object.FindObjectsByType<AllyController>(FindObjectsSortMode.None);
        foreach (var ally in allies)
        {
            float d = Vector3.Distance(self.transform.position, ally.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                nearest = ally;
            }
        }

        AgentController[] agents = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
        foreach (var agent in agents)
        {
            if (agent == self) continue;
            float d = Vector3.Distance(self.transform.position, agent.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                nearest = agent;
            }
        }

        return nearest;
    }
}


//using UnityEngine;

//public class MoveState : AgentBaseState
//{
//    private float attackCooldown = 0f;

//    public override void Enter(AgentController agent)
//    {
//        //Debug.Log($"[{agent.name}] MoveState: 開始");
//    }

//    public override void Execute(AgentController agent)
//    {
//        LogAgentStatus(agent, "Attacking");

//        attackCooldown -= Time.deltaTime;

//        // ----------------------------
//        // ① 攻撃範囲チェック
//        // ----------------------------
//        MonoBehaviour nearest = FindNearestEnemy(agent);
//        if (nearest != null)
//        {
//            float dist = Vector3.Distance(agent.transform.position, ((Component)nearest).transform.position);

//            // 攻撃範囲内なら AttackState へ遷移して停止
//            if (dist <= agent.GetData().attackRange && attackCooldown <= 0f)
//            {
//                // --- 位置を軽くスナップしてブレを防止 ---
//                agent.transform.position = new Vector3(
//                    Mathf.Round(agent.transform.position.x * 100f) / 100f,
//                    agent.transform.position.y,
//                    Mathf.Round(agent.transform.position.z * 100f) / 100f
//                );

//                // --- 経路破棄＆移動停止 ---
//                agent.currentPath = null;
//                agent.pathIndex = 0;

//                // --- AttackStateへ遷移（このフレームでは動かない） ---
//                agent.ChangeState(new AttackState());
//                return;
//            }
//        }

//        // ----------------------------
//        // ② 経路再探索チェック
//        // ----------------------------
//        if (agent.needReplan)
//        {
//            agent.needReplan = false;
//            agent.ChangeState(new SearchState());
//            return;
//        }

//        // ----------------------------
//        // ③ 経路未設定なら探索へ
//        // ----------------------------
//        if (agent.currentPath == null || agent.currentPath.Length == 0)
//        {
//            agent.ChangeState(new SearchState());
//            return;
//        }

//        // ----------------------------
//        // ④ 移動速度が0（攻撃中）なら動かない
//        // ----------------------------
//        if (agent.GetData().moveSpeed <= 0f)
//            return;

//        // ----------------------------
//        // ⑤ 経路に沿って移動
//        // ----------------------------
//        Vector3 target = agent.currentPath[agent.pathIndex];
//        Vector3 current = agent.transform.position;
//        target.y = current.y = 0f;

//        agent.transform.position = Vector3.MoveTowards(
//            current,
//            target,
//            agent.GetData().moveSpeed * Time.deltaTime
//        );

//        // 経路点に到達したら次へ
//        if (Vector3.Distance(agent.transform.position, target) < 0.01f)
//        {
//            agent.transform.position = target;
//            agent.pathIndex++;

//            if (agent.pathIndex >= agent.currentPath.Length)
//            {
//                agent.ChangeState(new GoalState());
//                return;
//            }
//        }

//        // ----------------------------
//        // ⑥ Heatmap記録
//        // ----------------------------
//        HeatmapManager.Instance.RecordPosition(agent.transform.position);
//    }

//    public override void Exit(AgentController agent)
//    {
//        //Debug.Log($"[{agent.name}] MoveState: 終了");
//    }

//    // ----------------------------
//    // 最寄りの敵を探す（Agent + Ally両対応）
//    // ----------------------------
//    private MonoBehaviour FindNearestEnemy(AgentController self)
//    {
//        float bestDist = float.MaxValue;
//        MonoBehaviour nearest = null;

//        // --- Ally（味方）を敵として検索 ---
//        AllyController[] allies = Object.FindObjectsByType<AllyController>(FindObjectsSortMode.None);
//        foreach (var ally in allies)
//        {
//            float d = Vector3.Distance(self.transform.position, ally.transform.position);
//            if (d < bestDist)
//            {
//                bestDist = d;
//                nearest = ally;
//            }
//        }

//        // --- 他のAgent（自分以外）も含める ---
//        AgentController[] agents = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
//        foreach (var agent in agents)
//        {
//            if (agent == self) continue;
//            float d = Vector3.Distance(self.transform.position, agent.transform.position);
//            if (d < bestDist)
//            {
//                bestDist = d;
//                nearest = agent;
//            }
//        }

//        return nearest;
//    }
//}