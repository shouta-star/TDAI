using UnityEngine;

public class SearchState : AgentBaseState
{
    public override void Enter(AgentController agent)
    {
        LogAgentStatus(agent, "Searching Path");

        // Œo˜H‚ª–¢İ’è‚Ü‚½‚Í‹ó‚Ì‚Æ‚«‚Ì‚İÄŒvZ‚ğs‚¤
        if (agent.currentPath == null || agent.currentPath.Length <= 1)
        {
            agent.RecalculatePath();
        }
    }

    public override void Execute(AgentController agent)
    {
        // Œo˜H‚ª‘¶İ‚·‚ê‚ÎˆÚ“®ƒXƒe[ƒg‚Ö‘JˆÚ
        if (agent.currentPath != null && agent.currentPath.Length > 0)
        {
            agent.ChangeState(new MoveState());
        }
    }

    public override void Exit(AgentController agent)
    {
        // “Á‚Éˆ—‚È‚µ
    }
}


//using UnityEngine;

//public class SearchState : AgentBaseState
//{
//    //public override void Enter(AgentController agent)
//    //{
//    //    //Debug.Log($"[{agent.name}] SearchState: Œo˜H’TõŠJn");

//    //    LogAgentStatus(agent, "Searching Path");

//    //    //AIManager aiManager = GameObject.FindObjectOfType<AIManager>();
//    //    AIManager aiManager = Object.FindFirstObjectByType<AIManager>();

//    //    Vector3 start = agent.transform.position;
//    //    Vector3 goal = agent.GetGoalPosition();

//    //    Vector3[] path = aiManager.GetPath(start, goal);
//    //    agent.SetPath(path);

//    //    agent.ChangeState(new MoveState());
//    //}
//    public override void Enter(AgentController agent)
//    {
//        //Debug.Log($"[{agent.name}] SearchState: Œo˜H’TõŠJn");
//        LogAgentStatus(agent, "Searching Path");

//        // AgentController‚É“‡‚³‚ê‚½Œo˜H’TõŒÄ‚Ño‚µ‚ğg—p
//        agent.RecalculatePath();
//        //agent.RequestPathRecalc();

//        // Œo˜H‚ªŒ©‚Â‚©‚Á‚½‚çMoveState‚Ö‘JˆÚ
//        agent.ChangeState(new MoveState());
//    }

//    public override void Execute(AgentController agent) { }

//    public override void Exit(AgentController agent)
//    {
//        //Debug.Log($"[{agent.name}] SearchState: I—¹");
//    }
//}
