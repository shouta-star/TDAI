using UnityEngine;

[CreateAssetMenu(fileName = "AgentData", menuName = "AI/AgentData")]
public class AgentData : ScriptableObject
{
    public float moveSpeed = 2f;
    public float searchInterval = 1f;

    public float attackPower; //UŒ‚—Í
    public float attackRange; //UŒ‚”ÍˆÍ
    public float attackInterval; //UŒ‚ŠÔŠu

    public Color agentColor = Color.cyan;
    public AIType aiType;
}

public enum AIType
{
    AStar,
    DStar,
    NavMesh
}
