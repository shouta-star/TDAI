using UnityEngine;

namespace PathIntelligence
{
    /// <summary>
    /// エージェントのHPを管理する純粋なコンポーネント。
    /// ダメージや回復の処理は外部スクリプト（例：AgentTileEffectHandler）が呼び出す。
    /// </summary>
    public class AgentHealth : MonoBehaviour
    {
        [Header("基本パラメータ")]
        [SerializeField] private float maxHP = 100f;
        [SerializeField] private float currentHP = 100f;
        [SerializeField] private bool invincible = false;
        [Tooltip("毎秒の自動回復量（0で無効）")]
        [SerializeField] private float autoRegenPerSec = 0f;

        private bool isDead = false;

        public float MaxHP => maxHP;
        public float CurrentHP => currentHP;
        public bool IsDead => isDead;

        private void Start()
        {
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            //Debug.Log($"[AgentHealth] {gameObject.name} 初期化: HP={currentHP}/{maxHP}");
        }

        private void Update()
        {
            if (!isDead && autoRegenPerSec > 0f && currentHP < maxHP)
            {
                Heal(autoRegenPerSec * Time.deltaTime);
            }
        }

        //=============================
        // HP操作メソッド
        //=============================
        public void TakeDamage(float amount, string source = "Unknown")
        {
            if (isDead || invincible) return;

            currentHP -= Mathf.Abs(amount);
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);

            //Debug.Log($"[AgentHealth] {gameObject.name} が {source} により {amount:F1} ダメージ → {currentHP:F1}/{maxHP}");

            if (currentHP <= 0f)
            {
                Die(source);
            }
        }

        public void Heal(float amount, string source = "Unknown")
        {
            if (isDead || amount <= 0f) return;

            currentHP += amount;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);

            //Debug.Log($"[AgentHealth] {gameObject.name} が {source} により {amount:F1} 回復 → {currentHP:F1}/{maxHP}");
        }

        private void Die(string source)
        {
            if (isDead) return;
            isDead = true;

            //Debug.Log($"[AgentHealth] {gameObject.name} が死亡（原因: {source}）");

            Destroy(gameObject, 0.5f);
        }

        public void ResetHP()
        {
            isDead = false;
            currentHP = maxHP;
        }

        public void SetInvincible(bool value)
        {
            invincible = value;
        }
    }
}