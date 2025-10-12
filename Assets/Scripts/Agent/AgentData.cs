using UnityEngine;

[CreateAssetMenu(fileName = "AgentData", menuName = "AI/AgentData")]
public class AgentData : ScriptableObject
{
    public float moveSpeed = 2f;
    public float searchInterval = 1f;
    public Color agentColor = Color.cyan;
    public AIType aiType;
}

public enum AIType
{
    AStar,
    BehaviorTree,
    Utility,
    GOAP,
    ChatGPT
}
