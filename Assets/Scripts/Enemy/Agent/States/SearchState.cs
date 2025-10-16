using UnityEngine;

public class SearchState : AgentBaseState
{
    //public override void Enter(AgentController agent)
    //{
    //    //Debug.Log($"[{agent.name}] SearchState: Œo˜H’TõŠJn");

    //    LogAgentStatus(agent, "Searching Path");

    //    //AIManager aiManager = GameObject.FindObjectOfType<AIManager>();
    //    AIManager aiManager = Object.FindFirstObjectByType<AIManager>();

    //    Vector3 start = agent.transform.position;
    //    Vector3 goal = agent.GetGoalPosition();

    //    Vector3[] path = aiManager.GetPath(start, goal);
    //    agent.SetPath(path);

    //    agent.ChangeState(new MoveState());
    //}
    public override void Enter(AgentController agent)
    {
        //Debug.Log($"[{agent.name}] SearchState: Œo˜H’TõŠJn");
        LogAgentStatus(agent, "Searching Path");

        // AgentController‚É“‡‚³‚ê‚½Œo˜H’TõŒÄ‚Ño‚µ‚ğg—p
        agent.RecalculatePath();

        // Œo˜H‚ªŒ©‚Â‚©‚Á‚½‚çMoveState‚Ö‘JˆÚ
        agent.ChangeState(new MoveState());
    }

    public override void Execute(AgentController agent) { }

    public override void Exit(AgentController agent)
    {
        //Debug.Log($"[{agent.name}] SearchState: I—¹");
    }
}
