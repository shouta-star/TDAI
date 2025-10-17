using UnityEngine;

public abstract class AllyBaseState
{
    public abstract void Enter(AllyController ally);
    public abstract void Execute(AllyController ally);
    public abstract void Exit(AllyController ally);

    // ============================
    // 共通ログ出力関数
    // ============================
    //protected void LogAllyStatus(AllyController ally, string action)
    //{
    //    string stateName = GetType().Name;
    //    string targetName = "None";
    //    Vector3 targetPos = Vector3.zero;

    //    // --- 現在の攻撃ターゲットを探索（敵 = Agent） ---
    //    MonoBehaviour nearest = FindNearestEnemy(ally);
    //    if (nearest != null)
    //    {
    //        targetName = nearest.name;
    //        targetPos = ((Component)nearest).transform.position;
    //    }

    //    Vector3 self = ally.transform.position;

    //    Debug.Log(
    //        $"[AllyLog] {ally.name} | State={stateName} | Action={action} | " +
    //        $"Target={targetName} | TargetPos=({targetPos.x:F2},{targetPos.y:F2},{targetPos.z:F2}) | " +
    //        $"SelfPos=({self.x:F2},{self.y:F2},{self.z:F2})"
    //    );
    //}
    //protected void LogAllyStatus(AllyController ally, string action)
    //{
    //    string stateName = GetType().Name;
    //    string targetName = "None";
    //    Vector3 targetPos = Vector3.zero;

    //    MonoBehaviour nearest = FindNearestEnemy(ally);
    //    if (nearest != null)
    //    {
    //        targetName = nearest.name;
    //        targetPos = ((Component)nearest).transform.position;
    //    }

    //    Vector3 self = ally.transform.position;

    //    Debug.Log($"[AllyLog] {ally.name} | State={stateName} | Action={action} | Target={targetName}");

    //    //CSVLogger.Log("Ally", ally.name, stateName, action, targetName, targetPos, self);
    //    CSVLogger.Log(
    //        "Ally",            // type
    //        ally.name,         // name
    //        stateName,         // state
    //        action,            // action
    //        targetName,        // target（敵名 or None）
    //        targetPos,         // Target座標（敵の位置）
    //        ally.transform.position,    // Current
    //        ally.GetNextPosition()      // Next（次フレーム位置）
    //                            // ,"Ally"          // ←もしAITypeを追加したい場合はコメント解除
    //    );
    //}
    protected void LogAllyStatus(AllyController ally, string action)
    {
        string stateName = GetType().Name;
        string targetName = "None";
        Vector3 targetPos = Vector3.zero;

        // --- 最も近い敵（Agent）を探す ---
        MonoBehaviour nearest = FindNearestEnemy(ally);
        if (nearest != null)
        {
            targetName = nearest.name;
            targetPos = ((Component)nearest).transform.position;
        }

        Vector3 current = ally.transform.position;
        Vector3 next = current; // ★ nextPosition未実装なら現位置を使う（後で ally.GetNextPosition() に変更可）

        Debug.Log($"[AllyLog] {ally.name} | State={stateName} | Action={action} | Target={targetName}");

        // ★ Target / Current / Next の3座標をCSV出力
        CSVLogger.Log(
            "Ally",            // type
            ally.name,         // name
            stateName,         // state
            action,            // action
            targetName,        // target（敵名 or None）
            targetPos,         // Target座標（敵の位置）
            current,           // Current（現フレーム位置）
            next               // Next（次フレーム想定位置。現状はcurrentと同じ）
                               // ,"Ally"          // ←もしAITypeを追加したい場合はコメント解除
        );
    }



    // ============================
    // 共通ターゲット探索（敵 = Agent）
    // ============================
    protected MonoBehaviour FindNearestEnemy(AllyController self)
    {
        float bestDist = float.MaxValue;
        MonoBehaviour nearest = null;

        AgentController[] agents = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
        foreach (var agent in agents)
        {
            if (agent == null) continue;
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
