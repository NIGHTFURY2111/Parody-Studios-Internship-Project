using UnityEngine;


/// <summary>
/// Represents the falling state of the player.
/// This state is active when the player is airborne and not in a controlled jump.
/// </summary>
public class FallState : BaseState
{
    float timeInAir;
    public FallState(PlayerStateMachine ctx, StateFactory factory) : base(ctx, factory)
    {
    }

    public override void EnterState() 
    { 
        timeInAir = ctx._timeInAirBeforeGameOver;
        //ctx._anim.Play("Fall");
        ctx._anim.SetBool("IsGrounded", false);
    }

    public override void FixedState()
    {
        ctx.Force(ctx.MovementVector() * ctx._forceAppliedInAir, ForceMode.Acceleration);
    }

    public override void UpdateState()
    {
        if (timeInAir > 0f) 
        {
            timeInAir -= Time.deltaTime;
        }
        else
        {
            ctx._gameManager.GameLost();
        }
        ctx.FaceCamera();
        CheckSwitchState();
    }


    public override void ExitState() { ctx._anim.SetBool("IsGrounded", true); }

    /// <summary>
    /// Checks conditions for transitioning to other states.
    /// Currently only transitions to IdleState when player becomes grounded.
    /// </summary>
    public override void CheckSwitchState()
    {   //idle slide

        //Idle
        if (ctx._isGrounded)
        {
            SwitchState(factory.Idle());
            return;
        }
    }

}

