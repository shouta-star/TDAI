using UnityEngine;

public class PlayerTileEditor : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask tileLayer;
    [SerializeField] private TileType toolToType = TileType.Floor; // UIから切り替える

    private MapManager map;

    private void Awake()
    {
        if (!cam) cam = Camera.main;
        //map = FindObjectOfType<MapManager>();
        map = Object.FindFirstObjectByType<MapManager>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!TryPickTile(out var tile)) return;

            // 変更可否チェック
            if (!map.CanChange(tile.gridX, tile.gridZ, toolToType, out string reason))
            {
                Debug.LogWarning($"[TileEditor] 変更不可: {reason}");
                return;
            }

            map.ApplyChange(tile.gridX, tile.gridZ, toolToType);
        }
    }

    private bool TryPickTile(out TileComponent tile)
    {
        tile = null;
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 1000f, tileLayer))
        {
            tile = hit.collider.GetComponentInParent<TileComponent>();
            return tile != null;
        }
        return false;
    }

    // 外部UIから呼ぶ
    public void SetToolToType(TileType t) => toolToType = t;
}
