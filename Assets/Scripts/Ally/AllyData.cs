using UnityEngine;

[CreateAssetMenu(fileName = "AllyData", menuName = "AI/AllyData")]
public class AllyData : ScriptableObject
{
    public float moveSpeed = 2f;
    public float searchInterval = 1f;

    public float attackPower; //UŒ‚—Í
    public float attackRange; //UŒ‚”ÍˆÍ
    public float attackInterval; //UŒ‚ŠÔŠu

    public Color agentColor = Color.cyan;
    public AllyAIType aiType;
}

public enum AllyAIType
{
    AStar,
    BehaviorTree,
    Utility,
    GOAP
}
