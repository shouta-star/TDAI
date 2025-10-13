using UnityEngine;
using PathIntelligence; // AgentHealth がこの名前空間内の場合

/// <summary>
/// Agentが今いるタイルを取得し、Dangerタイルに入った瞬間だけダメージを与える。
/// </summary>
[RequireComponent(typeof(AgentHealth))]
public class AgentTileWatcherOneShot : MonoBehaviour
{
    [Header("Ray 設定")]
    [SerializeField] private float rayStartHeight = 0.5f;
    [SerializeField] private float rayLength = 2.0f;
    [SerializeField] private LayerMask tileLayerMask; // 例: Tile レイヤー

    [Header("ダメージ設定")]
    [SerializeField] private float damageOnEnter = 10f;

    private AgentHealth health;
    private TileComponent lastTile; // 直前に踏んでいたタイル（参照が変わった瞬間のみ反応）

    private void Awake()
    {
        health = GetComponent<AgentHealth>();
    }

    private void Update()
    {
        var origin = transform.position + Vector3.up * rayStartHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLength, tileLayerMask, QueryTriggerInteraction.Collide))
        {
            var tile = hit.collider.GetComponentInParent<TileComponent>() ?? hit.collider.GetComponent<TileComponent>();

            // タイルが切り替わった瞬間だけ処理
            if (tile != null && tile != lastTile)
            {
                if (tile.type == TileType.Danger)
                {
                    health.TakeDamage(damageOnEnter, "DangerTile(Enter)");
                }
                lastTile = tile;
            }
        }
        else
        {
            // 足元にタイルが無い（ジャンプ中など）は一旦リセットしても良い
            lastTile = null;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        var origin = transform.position + Vector3.up * rayStartHeight;
        Gizmos.DrawLine(origin, origin + Vector3.down * rayLength);
    }
#endif
}
