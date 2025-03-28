using System.Collections;
using UnityEngine;

/// <summary>
/// Represents the jumping state of the player.
/// This state is active when the player initiates a jump from the ground.
/// </summary>
public class JumpState : BaseState
{
    bool jumpCompleted;

    public JumpState(PlayerStateMachine ctx, StateFactory factory) : base(ctx, factory)
    {
    }

    public override void EnterState()
    {
        jumpCompleted = false;
        //ctx._anim.Play("Fall");  // Using fall animation for jump
        ctx._anim.SetBool("IsGrounded", false);
        ctx.StartCoroutine(Jumping());
    }
    public override void UpdateState() 
    {
        if (jumpCompleted) 
        {
            CheckSwitchState();
        }
    }

    /// <summary>
    /// Coroutine that handles the jumping physics.
    /// Adds upward velocity to simulate a jump and then waits briefly before allowing state transitions.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    IEnumerator Jumping()
    {
        ctx._velocity += ctx._player_Up * ctx._jumpSpeed;
        yield return new WaitForSecondsRealtime(0.2f);     
        CheckSwitchState();
        jumpCompleted = true;
    }

    public override void ExitState() { }


    /// <summary>
    /// Checks conditions for transitioning to other states.
    /// Transitions to IdleState if grounded, or to FallState if airborne.
    /// </summary>
    public override void CheckSwitchState()
    {   
        //idle OR Fall
        SwitchState( ctx._isGrounded ? factory.Idle(): factory.Fall());

        return;    
    }

}
