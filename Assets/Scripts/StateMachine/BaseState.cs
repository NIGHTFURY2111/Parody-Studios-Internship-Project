using System.Diagnostics;

public abstract class BaseState
{
    protected PlayerStateMachine ctx;
    protected StateFactory factory;
    public BaseState(PlayerStateMachine ctx,StateFactory factory)
    {
        this.ctx = ctx;
        this.factory = factory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchState();
    public virtual void LateUpdateState() { }
    public virtual void FixedState() { }

    protected void SwitchState(BaseState next)
    {
        ctx._currentState.ExitState();
        ctx._currentState = next;
        ctx._currentState.EnterState();
    }

}
