using UnityEngine;
using PathIntelligence;
using System.Collections;

/// <summary>
/// TileType に応じた効果（Damage・Slow・Healなど）を適用する。
/// NavMeshAgent は使わず、AgentData.moveSpeed を直接操作する。
/// </summary>
[RequireComponent(typeof(AgentHealth))]
public class AgentTileEffectHandler : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private AgentData agentData; // AgentDataから速度値を参照
    private AgentHealth health;
    private Coroutine slowCoroutine;

    [Header("Danger設定")]
    [SerializeField, Tooltip("Dangerタイルに入ったときのダメージ量")]
    private float dangerDamage = 10f;

    [Header("Slow設定")]
    [SerializeField, Tooltip("Slowタイルでの速度倍率（0.5=半減）")]
    private float slowRatio = 0.5f;
    [SerializeField, Tooltip("Slow効果の継続時間（秒）")]
    private float slowDuration = 3f;

    [Header("Heal設定")]
    [SerializeField, Tooltip("Healタイルに入ったときの回復量")]
    private float healAmount = 10f;

    private void Awake()
    {
        health = GetComponent<AgentHealth>();
    }

    /// <summary>
    /// AgentTileWatcher から呼ばれる：タイルに入ったときの処理
    /// </summary>
    public void OnEnterTile(TileType type)
    {
        switch (type)
        {
            case TileType.Danger:
                ApplyDanger();
                break;

            case TileType.Slow:
                ApplySlow();
                break;

            case TileType.Goal:
                ApplyHeal();
                break;

            default:
                break;
        }
    }

    //=============================
    // 各Tile効果
    //=============================

    private void ApplyDanger()
    {
        if (health == null) return;
        health.TakeDamage(dangerDamage, "DangerTile");
    }

    private void ApplyHeal()
    {
        if (health == null) return;
        health.Heal(healAmount, "HealTile");
    }

    private void ApplySlow()
    {
        if (agentData == null)
        {
            Debug.LogWarning("[EffectHandler] AgentData が設定されていません。Slow効果を適用できません。");
            return;
        }

        if (slowCoroutine != null)
            StopCoroutine(slowCoroutine);

        slowCoroutine = StartCoroutine(SlowEffect());
    }

    private IEnumerator SlowEffect()
    {
        float originalSpeed = agentData.moveSpeed;
        float slowedSpeed = originalSpeed * slowRatio;

        agentData.moveSpeed = slowedSpeed;
        Debug.Log($"[EffectHandler] Slow適用: {originalSpeed:F2} → {slowedSpeed:F2} (ratio={slowRatio:F2}) for {slowDuration:F1}s");

        yield return new WaitForSeconds(slowDuration);

        agentData.moveSpeed = originalSpeed;
        Debug.Log($"[EffectHandler] Slow解除: {agentData.moveSpeed:F2}");

        slowCoroutine = null;
    }
}


//using UnityEngine;
//using PathIntelligence;
//using System.Collections;

///// <summary>
///// �^�C���^�C�v�ɉ������G�[�W�F���g�ւ̌��ʂ��ꊇ�Ǘ�����n���h���[�B
///// Danger / Slow / Heal �Ȃǂ̊��M�~�b�N�������ŏ�������B
///// </summary>
//[RequireComponent(typeof(AgentHealth))]
//[DisallowMultipleComponent]
//public class AgentTileEffectHandler : MonoBehaviour
//{
//    [Header("�Q�Ɛݒ�")]
//    [SerializeField] private AgentData agentData; // ���x�̊�l���擾����
//    private AgentHealth health;
//    private UnityEngine.AI.NavMeshAgent agent;

//    [Header("Danger�ݒ�")]
//    [SerializeField, Tooltip("Danger�^�C���ɓ��������̃_���[�W��")]
//    private float dangerDamage = 10f;

//    [Header("Slow�ݒ�")]
//    [SerializeField, Tooltip("Slow�^�C���ɓ��������̑��x�{���i0.5 = 50���j")]
//    private float slowRatio = 0.5f;
//    [SerializeField, Tooltip("Slow���ʂ������b��")]
//    private float slowDuration = 3f;

//    [Header("Heal�ݒ�")]
//    [SerializeField, Tooltip("Heal�^�C���ɓ��������̉񕜗�")]
//    private float healAmount = 10f;

//    //private AgentHealth health;
//   // private UnityEngine.AI.NavMeshAgent agent;
//    private Coroutine slowCoroutine;

//    private void Awake()
//    {
//        health = GetComponent<AgentHealth>();
//        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
//    }

//    /// <summary>
//    /// AgentTileWatcher ����Ă΂��F�^�C���ɓ������u�Ԃ̏���
//    /// </summary>
//    public void OnEnterTile(TileType tileType)
//    {
//        switch (tileType)
//        {
//            case TileType.Danger:
//                ApplyDanger();
//                break;

//            case TileType.Slow:
//                ApplySlow();
//                break;

//            case TileType.Start:
//            case TileType.Floor:
//                // �������Ȃ�
//                break;

//            case TileType.Goal:
//                ApplyHeal();
//                break;

//            default:
//                break;
//        }
//    }

//    //=====================================
//    // �e���ʏ���
//    //=====================================

//    private void ApplyDanger()
//    {
//        if (health == null) return;
//        health.TakeDamage(dangerDamage, "DangerTile");
//    }

//    private void ApplyHeal()
//    {
//        if (health == null) return;
//        health.Heal(healAmount, "HealTile");
//    }

//    //private void ApplySlow()
//    //{
//    //    if (agent == null) return;

//    //    // ���łɃX���[���Ȃ�㏑��
//    //    if (slowCoroutine != null)
//    //    {
//    //        StopCoroutine(slowCoroutine);
//    //    }
//    //    slowCoroutine = StartCoroutine(SlowEffectCoroutine());
//    //}

//    //private IEnumerator SlowEffectCoroutine()
//    //{
//    //    float originalSpeed = agent.speed;
//    //    agent.speed = originalSpeed * slowRatio;
//    //    Debug.Log($"[AgentTileEffectHandler] Slow�K�p: {agent.speed:F2}�i{slowDuration:F1}s�j");

//    //    yield return new WaitForSeconds(slowDuration);

//    //    agent.speed = originalSpeed;
//    //    Debug.Log($"[AgentTileEffectHandler] Slow����: {agent.speed:F2}");

//    //    slowCoroutine = null;
//    //}

//    private void ApplySlow()
//    {
//        Debug.Log($"[EffectHandler] ApplySlow() 呼び出し確認: agent={agent != null}, agentData={agentData != null}");


//        if (agentData == null || agent == null) return;

//        // ���łɃX���[���Ȃ烊�Z�b�g
//        if (slowCoroutine != null)
//            StopCoroutine(slowCoroutine);

//        slowCoroutine = StartCoroutine(SlowEffect());
//    }

//    //private IEnumerator SlowEffect()
//    //{
//    //    Debug.Log("AAA");

//    //    float baseSpeed = agentData.moveSpeed;       // ScriptableObject ����擾
//    //    float originalSpeed = agent.speed;           // ���݂� NavMeshAgent.speed
//    //    agent.speed = baseSpeed * slowRatio;         // AgentData ����ɒቺ

//    //    Debug.Log($"[AgentTileEffectHandler] Slow�K�p: speed={agent.speed:F2} (for {slowDuration:F1}s)");

//    //    yield return new WaitForSeconds(slowDuration);

//    //    agent.speed = baseSpeed; // ���ɖ߂�
//    //    Debug.Log($"[AgentTileEffectHandler] Slow����: speed={agent.speed:F2}");

//    //    slowCoroutine = null;
//    //}
//    private IEnumerator SlowEffect()
//    {
//        float baseSpeed = agentData.moveSpeed;
//        float before = agent.speed;

//        agent.speed = baseSpeed * slowRatio;
//        Debug.Log($"[EffectHandler] Slow適用: speed(set) {before:F2} → {agent.speed:F2} (base={baseSpeed:F2}, ratio={slowRatio:F2})");

//        yield return new WaitForSeconds(0.5f);
//        Debug.Log($"[EffectHandler] 実速度 velocity.magnitude = {agent.velocity.magnitude:F2}");

//        yield return new WaitForSeconds(slowDuration - 0.5f);

//        agent.speed = baseSpeed;
//        Debug.Log($"[EffectHandler] Slow解除: speed(set)={agent.speed:F2}");
//    }

//}


////using UnityEngine;
////using PathIntelligence;
////using System.Collections;

////[RequireComponent(typeof(AgentHealth))]
////public class AgentTileEffectHandler : MonoBehaviour
////{
////    [Header("Danger�ݒ�")]
////    [SerializeField] private float damageOnEnter = 10f;

////    [Header("Slow�ݒ�")]
////    [SerializeField] private float slowRatio = 0.5f;
////    [SerializeField] private float slowDuration = 3f;

////    private AgentHealth health;
////    private UnityEngine.AI.NavMeshAgent agent; // NavMeshAgent�𗘗p����O��
////    private Coroutine slowCoroutine;

////    private void Awake()
////    {
////        health = GetComponent<AgentHealth>();
////        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
////    }

////    /// <summary>
////    /// TileWatcher ����Ăяo�����
////    /// </summary>
////    public void OnEnterTile(TileType type)
////    {
////        switch (type)
////        {
////            case TileType.Danger:
////                ApplyDamage();
////                break;

////            case TileType.Slow:
////                ApplySlow();
////                break;

////                // ����̊g��
////                // case TileType.Heal:
////                //     ApplyHeal();
////                //     break;
////        }
////    }

////    private void ApplyDamage()
////    {
////        health.TakeDamage(damageOnEnter, "DangerTile(Enter)");
////    }

////    private void ApplySlow()
////    {
////        if (agent == null) return;

////        // ���łɃX���[���Ȃ烊�Z�b�g
////        if (slowCoroutine != null)
////        {
////            StopCoroutine(slowCoroutine);
////        }
////        slowCoroutine = StartCoroutine(SlowEffect());
////    }

////    private IEnumerator SlowEffect()
////    {
////        float originalSpeed = agent.speed;
////        agent.speed = originalSpeed * slowRatio;

////        Debug.Log($"[AgentTileEffectHandler] Slow�K�p: {agent.speed:F2} for {slowDuration:F1}s");
////        yield return new WaitForSeconds(slowDuration);

////        agent.speed = originalSpeed;
////        Debug.Log($"[AgentTileEffectHandler] Slow����: {agent.speed:F2}");

////        slowCoroutine = null;
////    }
////}
