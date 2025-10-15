using UnityEngine;
using PathIntelligence;

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
        // ターゲットを探索（Agent/Ally両方を候補に含める）
        var enemy = FindNearestEnemy(agent);   // 戻り値: MonoBehaviour
        if (enemy == null)
        {
            agent.ChangeState(new SearchState());
            return;
        }

        // 距離チェック
        float dist = Vector3.Distance(agent.transform.position, enemy.transform.position);
        if (dist > agent.GetData().attackRange)
        {
            // 攻撃範囲外なら速度を戻して移動へ
            agent.GetData().moveSpeed = originalMoveSpeed;
            agent.ChangeState(new MoveState());
            return;
        }

        // 攻撃タイミング
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;

            // ===== 攻撃対象ごとの処理 =====
            if (enemy is AgentController enemyAgent)
            {
                // 敵(Agent)を攻撃
                agent.Attack(enemyAgent);
            }
            else if (enemy is AllyController ally)
            {
                // 味方(Ally)を攻撃
                var hp = ally.GetComponent<AllyHealth>();
                if (hp != null)
                {
                    hp.TakeDamage(agent.GetData().attackPower, agent.name);
                    Debug.Log($"[{agent.name}] が {ally.name} に {agent.GetData().attackPower} ダメージ！");
                }
                else
                {
                    Debug.LogWarning($"[AttackState] {ally.name} に AllyHealth が見つかりません");
                }
            }
        }
    }


    public override void Exit(AgentController agent)
    {
        // 念のためここでも復元（Searchに行くケースなどの保険）
        agent.GetData().moveSpeed = originalMoveSpeed;
        Debug.Log($"[{agent.name}] AttackState: Exit speed -> {agent.GetData().moveSpeed}");
    }

    private MonoBehaviour FindNearestEnemy(AgentController self)
    {
        var agents = Object.FindObjectsByType<AgentController>(FindObjectsSortMode.None);
        var allies = Object.FindObjectsByType<AllyController>(FindObjectsSortMode.None);

        MonoBehaviour best = null;
        float bestDist = float.MaxValue;

        void Try(MonoBehaviour mb)
        {
            if (mb == null || mb == self) return;
            float d = Vector3.Distance(self.transform.position, mb.transform.position);
            if (d < bestDist) { bestDist = d; best = mb; }
        }

        foreach (var a in agents) Try(a);
        foreach (var a in allies) Try(a);

        return best;
    }

}