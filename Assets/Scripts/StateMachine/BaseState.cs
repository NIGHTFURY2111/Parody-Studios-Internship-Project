    /// <summary>
    /// Abstract base class for all states in the player state machine.
    /// Provides the core structure and functionality for the state pattern implementation.
    /// </summary>
public abstract class BaseState
{

    /// <summary>
    /// Reference to the PlayerStateMachine context that this state operates within.
    /// Provides access to player properties and actions.
    /// </summary>
    protected PlayerStateMachine ctx;
    /// <summary>
    /// Reference to the StateFactory that creates and manages all available states.
    /// Used for state transitions.
    /// </summary>
    protected StateFactory factory;

    /// <summary>
    /// Initializes a new instance of the BaseState class.
    /// </summary>
    /// <param name="ctx">The PlayerStateMachine that this state will operate on.</param>
    /// <param name="factory">The StateFactory for creating new states during transitions.</param>
    public BaseState(PlayerStateMachine ctx,StateFactory factory)
    {
        this.ctx = ctx;

        this.factory = factory;
    }


    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();
    public abstract void CheckSwitchState();
    /// <summary>
    /// Called after UpdateState in the Unity LateUpdate cycle.
    /// Optional override for states that need to perform actions after regular updates.
    /// </summary>
    public virtual void LateUpdateState() { }
    /// <summary>
    /// Called in the Unity FixedUpdate cycle for physics-related updates.
    /// Optional override for states that need to handle physics or forces.
    /// </summary>

    public virtual void FixedState() { }


    /// <summary>
    /// Handles the transition from the current state to a new state.
    /// Calls ExitState on the old state and EnterState on the new state.
    /// </summary>
    /// <param name="next">The new state to transition to.</param>
    protected void SwitchState(BaseState next)
    {
        ctx._currentState.ExitState();
        ctx._currentState = next;
        ctx._currentState.EnterState();
    }

}
