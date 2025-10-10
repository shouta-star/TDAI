using UnityEngine;
using System;

[DisallowMultipleComponent]
public class TileComponent : MonoBehaviour
{
    [Header("Config")]
    public TileType type = TileType.Floor;
    [Tooltip("MapManager の nodeSpacing と整合するグリッド座標")]
    public int gridX;
    public int gridZ;

    [Header("Runtime")]
    public bool walkable = true;
    public float moveCost = 1f;

    [Header("Visuals (任意)")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color floorColor = new Color(0.75f, 0.77f, 0.81f);
    [SerializeField] private Color wallColor = new Color(0.37f, 0.40f, 0.43f);
    [SerializeField] private Color dangerColor = new Color(0.89f, 0.34f, 0.30f, 0.85f);
    [SerializeField] private Color slowColor = new Color(0.95f, 0.76f, 0.31f, 0.95f);
    [SerializeField] private Color startColor = new Color(0.48f, 0.84f, 0.48f);
    [SerializeField] private Color goalColor = new Color(0.61f, 0.48f, 0.95f);

    public event Action<TileType, TileType> OnTypeChanged; // (from, to)

    private void Reset()
    {
        targetRenderer = GetComponentInChildren<Renderer>();
    }

    private void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponentInChildren<Renderer>();
        ApplyTypeInternal(type, silent: true);
    }

    public Vector3 GetWorldCenter(float nodeSpacing)
    {
        return new Vector3(gridX * nodeSpacing, transform.position.y, gridZ * nodeSpacing);
    }

    /// <summary>
    /// MapManager から呼ばれる。見た目/論理を同期してイベントも出す。
    /// </summary>
    public void ApplyType(TileType to, bool silent = false)
    {
        var from = type;
        if (from == to) return;

        ApplyTypeInternal(to, silent: false);

        if (!silent)
            OnTypeChanged?.Invoke(from, to);
    }

    private void ApplyTypeInternal(TileType to, bool silent)
    {
        type = to;
        switch (to)
        {
            case TileType.Floor:
                walkable = true; moveCost = 1f; SetColor(floorColor); break;
            case TileType.Wall:
                walkable = false; moveCost = Mathf.Infinity; SetColor(wallColor); break;
            case TileType.Danger:
                walkable = true; moveCost = 1f; SetColor(dangerColor); break;
            case TileType.Slow:
                walkable = true; moveCost = 2f; SetColor(slowColor); break;
            case TileType.Start:
                walkable = true; moveCost = 1f; SetColor(startColor); break;
            case TileType.Goal:
                walkable = true; moveCost = 1f; SetColor(goalColor); break;
        }
    }

    private void SetColor(Color c)
    {
        if (!targetRenderer) return;
        // マテリアルインスタンス化（Batching重視ならPropertyBlock推奨）
        var mat = targetRenderer.material;
        mat.color = c;
    }
}
