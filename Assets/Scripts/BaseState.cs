public abstract class AgentBaseState
{
    public abstract void Enter(AgentController agent);
    public abstract void Execute(AgentController agent);
    public abstract void Exit(AgentController agent);
}
