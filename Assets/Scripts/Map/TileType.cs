public enum TileType
{
    Floor,   // 通常床（walkable=true, cost=1）
    Wall,    // 壁（walkable=false）
    Danger,  // 危険床（walkable=true, cost=1）※ダメージ表現は別で
    Slow,    // 遅い床（walkable=true, cost=2 など）
    Start,   // 入口表示用（walkable=true, cost=1）
    Goal     // 目的地表示用（walkable=true, cost=1）
}
