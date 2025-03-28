/// <summary>
/// Represents the movement state of the player.
/// This state is active when the player is moving while grounded.
/// </summary>
public class MoveState : BaseState
{
   
    public MoveState(PlayerStateMachine ctx, StateFactory factory) : base(ctx, factory)
    {
    }

    /// <summary>
    /// Called when entering the move state.
    /// Sets animation parameters to reflect grounded movement.
    /// </summary>
    public override void EnterState()
    {
        //ctx._anim.Play("Walk");
        ctx._anim.SetBool("IsGrounded", true);
        ctx._anim.SetFloat("Speed", ctx._playerVelocityInPlane.magnitude);
    }
    public override void UpdateState()
    {
        // Update the animation speed parameter based on movement
        ctx._anim.SetFloat("Speed", ctx._playerVelocityInPlane.magnitude);
        CheckSwitchState();
    }

    public override void FixedState()
    {
        ctx._velocity = ctx.MovementVector() * ctx._walkingSpeed + ctx._player_Up * ctx._player_Up_velocity ;
        ctx.FaceCamera();
    }


    public override void ExitState() { }

    /// <summary>
    /// Checks conditions for transitioning to other states.
    /// State transitions can occur when:
    /// - Player becomes airborne (to FallState)
    /// - No movement input is detected (to IdleState)
    /// - Jump button is pressed (to JumpState)
    /// </summary>
    public override void CheckSwitchState()
    {   //idle dash jump  slide fall
        
        //Fall
        if (!ctx._isGrounded) 
        {
            SwitchState(factory.Fall());
            return;
        }
        //Idle
        if (ctx.MoveDir.magnitude == 0f)
        {
            SwitchState(factory.Idle());
            return;
        }

        //Jump
        if (ctx._jump.WasPerformedThisFrame())
        {
            SwitchState(factory.Jump());
            return;
        }
    }
}
