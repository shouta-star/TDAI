using UnityEngine;
using UnityEngine.Events;

namespace PathIntelligence
{
    /// <summary>
    /// 研究実験用エージェント共通ヘルス管理クラス。
    /// BT / Utility / GOAP いずれのAIでも利用可能。
    /// </summary>
    public class AgentHealth : MonoBehaviour
    {
        [Header("基本パラメータ")]
        [SerializeField] private float maxHP = 100f;
        [SerializeField] private float currentHP = 100f;
        [SerializeField] private bool invincible = false;
        [Tooltip("毎秒の自動回復量（0で無効）")]
        [SerializeField] private float autoRegenPerSec = 0f;

        [Header("研究用ログ設定")]
        [SerializeField] private bool enableDebugLog = true;
        [SerializeField] private bool autoDestroyOnDeath = false;

        [Header("イベント")]
        public UnityEvent<float> OnDamaged;   // 被ダメージ時
        public UnityEvent<float> OnHealed;    // 回復時
        public UnityEvent OnDeath;            // 死亡時

        private bool isDead = false;

        //=============================
        // 公開プロパティ
        //=============================
        public float MaxHP => maxHP;
        public float CurrentHP => currentHP;
        public bool IsDead => isDead;

        private void Start()
        {
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            if (enableDebugLog)
                Debug.Log($"[AgentHealth] {gameObject.name} 初期化: HP={currentHP}/{maxHP}");
        }

        private void Update()
        {
            if (!isDead && autoRegenPerSec > 0f && currentHP < maxHP)
            {
                Heal(autoRegenPerSec * Time.deltaTime);
            }
        }

        //=============================
        // ダメージ処理
        //=============================
        public void TakeDamage(float amount, string source = "Unknown")
        {
            if (isDead || invincible) return;

            currentHP -= Mathf.Abs(amount);
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);

            if (enableDebugLog)
                Debug.Log($"[AgentHealth] {gameObject.name} が {source} により {amount:F1} ダメージを受けた → {currentHP:F1}/{maxHP}");

            OnDamaged?.Invoke(amount);

            if (currentHP <= 0f)
            {
                Die(source);
            }
        }

        //=============================
        // 回復処理
        //=============================
        public void Heal(float amount, string source = "Unknown")
        {
            if (isDead || amount <= 0f) return;

            currentHP += amount;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);

            if (enableDebugLog)
                Debug.Log($"[AgentHealth] {gameObject.name} が {source} により {amount:F1} 回復 → {currentHP:F1}/{maxHP}");

            OnHealed?.Invoke(amount);
        }

        //=============================
        // 死亡処理
        //=============================
        private void Die(string source)
        {
            if (isDead) return;
            isDead = true;

            if (enableDebugLog)
                Debug.Log($"[AgentHealth] {gameObject.name} が死亡（原因: {source}）");

            OnDeath?.Invoke();

            if (autoDestroyOnDeath)
                Destroy(gameObject, 0.5f);
        }

        //=============================
        // 外部アクセス用
        //=============================
        public void ResetHP()
        {
            isDead = false;
            currentHP = maxHP;
            if (enableDebugLog)
                Debug.Log($"[AgentHealth] {gameObject.name} HPリセット: {currentHP}/{maxHP}");
        }

        public void SetInvincible(bool value)
        {
            invincible = value;
            if (enableDebugLog)
                Debug.Log($"[AgentHealth] {gameObject.name} 無敵モード: {invincible}");
        }

        //=============================
        // ギミック連携用
        //=============================
        private void OnTriggerStay(Collider other)
        {
            // ダメージゾーン
            if (other.CompareTag("DamageZone"))
            {
                TakeDamage(Time.deltaTime * 5f, "DamageZone");
            }

            // 回復ゾーン
            if (other.CompareTag("HealZone"))
            {
                Heal(Time.deltaTime * 3f, "HealZone");
            }
        }
    }
}
