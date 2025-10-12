using UnityEngine;
using PathIntelligence; // AgentHealth を使う場合

[RequireComponent(typeof(Collider))]
public class DangerTileDetector : MonoBehaviour
{
    [Tooltip("このTileに関連付けられたTileComponent")]
    [SerializeField] private TileComponent tile;

    [Tooltip("Dangerタイルに入ったときのダメージ量")]
    [SerializeField] private float damageOnEnter = 10f;

    private void Awake()
    {
        // TileComponent を自動取得
        if (!tile)
            tile = GetComponentInParent<TileComponent>();

        // Collider をトリガーに設定
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // TileComponent が Danger でなければ何もしない
        if (tile == null || tile.type != TileType.Danger) return;

        // AgentHealth を探してダメージを与える
        var health = other.GetComponentInParent<AgentHealth>() ?? other.GetComponent<AgentHealth>();
        if (health != null)
        {
            health.TakeDamage(damageOnEnter, "DangerTile");
        }
    }
}
