using UnityEngine;

public class MovingWall : MonoBehaviour
{
    [SerializeField] private float switchInterval = 5f;
    [SerializeField] private float blockRadius = 0.5f; // ï«ÇÃîºåaÇí«â¡
    private float timer;
    private bool isBlocked = true;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        ApplyState();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= switchInterval)
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

        if (AIManager.Instance != null)
            AIManager.Instance.OnDynamicObstacleChanged(transform.position, blockRadius, isBlocked);

        Debug.Log($"[MovingWall] {name} Å® {(isBlocked ? "Blocked" : "Open")} at {transform.position}");
    }
}


//using UnityEngine;
//public class MovingWall : MonoBehaviour
//{
//    [SerializeField] private float switchInterval = 5f;
//    private float timer;
//    private bool isBlocked = true;
//    private Renderer rend;

//    void Start()
//    {
//        rend = GetComponent<Renderer>();
//        ApplyState();
//    }

//    void Update()
//    {
//        timer += Time.deltaTime;
//        if (timer >= switchInterval)
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
//            AIManager.Instance.OnDynamicObstacleChanged(transform.position, isBlocked);

//        Debug.Log($"[MovingWall] {name} Å® {(isBlocked ? "Blocked" : "Open")} at {transform.position}");
//    }
//}
