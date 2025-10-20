using UnityEngine;

public class MovingWall : MonoBehaviour
{
    [SerializeField] private float switchInterval = 5f;
    [SerializeField] private float blockRadius = 0.5f;   // 壁の半径
    [SerializeField] private bool isBlocked = true;      // 最初は壁状態にする

    private float timer;
    private Renderer rend;
    private bool initialized = false;                    // 起動直後の1フレームだけUpdateをスキップ

    void Awake()
    {
        rend = GetComponent<Renderer>();
        // 明示的に壁状態で開始し、即AIManagerへ登録
        ApplyState();
    }

    void Update()
    {
        // 起動直後1フレームはスキップ（GameManagerが初期探索を走らせるため）
        if (!initialized)
        {
            initialized = true;
            return;
        }

        timer += Time.deltaTime;
        if (timer >= switchInterval)                     // ★ interval → switchInterval に修正
        {
            timer = 0f;
            isBlocked = !isBlocked;
            ApplyState();
        }
    }

    void ApplyState()
    {
        if (rend != null)
            rend.material.color = isBlocked ? Color.red : Color.gray;

        // 壁状態をAIManagerへ通知（半径も渡す）
        if (AIManager.Instance != null)
            AIManager.Instance.OnDynamicObstacleChanged(transform.position, blockRadius, isBlocked);

        Debug.Log($"[MovingWall] {name} → {(isBlocked ? "Blocked" : "Open")} at {transform.position}");
    }
}


//using UnityEngine;

//public class MovingWall : MonoBehaviour
//{
//    [SerializeField] private float switchInterval = 5f;
//    [SerializeField] private float blockRadius = 0.5f; // 壁の半径を追加
//    private float timer;
//    private bool isBlocked = true;
//    private Renderer rend;

//    //private float timer;
//    private bool initialized = false;

//    void Awake()
//    {
//        rend = GetComponent<Renderer>();
//        isBlocked = true; // 明示的に壁から開始
//        ApplyState();     // ★ Awake() で壁登録
//    }

//    void Start()
//    {
//        //rend = GetComponent<Renderer>();
//        //ApplyState();
//    }

//    void Update()
//    {
//        //timer += Time.deltaTime;
//        //if (timer >= switchInterval)
//        //{
//        //    timer = 0f;
//        //    isBlocked = !isBlocked;
//        //    ApplyState();
//        //}

//        if (!initialized)
//        {
//            // Awake直後の1フレームはスキップ（GameManagerが初期探索を完了するまで）
//            initialized = true;
//            return;
//        }

//        timer += Time.deltaTime;
//        if (timer >= interval)
//        {
//            timer = 0f;
//            isBlocked = !isBlocked;
//            ApplyState();
//        }
//    }

//    void ApplyState()
//    {
//        if (rend != null)
//            rend.material.color = isBlocked ? Color.red : Color.gray;

//        if (AIManager.Instance != null)
//            AIManager.Instance.OnDynamicObstacleChanged(transform.position, blockRadius, isBlocked);

//        Debug.Log($"[MovingWall] {name} → {(isBlocked ? "Blocked" : "Open")} at {transform.position}");
//    }
//}


////using UnityEngine;
////public class MovingWall : MonoBehaviour
////{
////    [SerializeField] private float switchInterval = 5f;
////    private float timer;
////    private bool isBlocked = true;
////    private Renderer rend;

////    void Start()
////    {
////        rend = GetComponent<Renderer>();
////        ApplyState();
////    }

////    void Update()
////    {
////        timer += Time.deltaTime;
////        if (timer >= switchInterval)
////        {
////            timer = 0f;
////            isBlocked = !isBlocked;
////            ApplyState();
////        }
////    }

////    void ApplyState()
////    {
////        if (rend != null)
////            rend.material.color = isBlocked ? Color.red : Color.gray;

////        if (AIManager.Instance != null)
////            AIManager.Instance.OnDynamicObstacleChanged(transform.position, isBlocked);

////        Debug.Log($"[MovingWall] {name} → {(isBlocked ? "Blocked" : "Open")} at {transform.position}");
////    }
////}
