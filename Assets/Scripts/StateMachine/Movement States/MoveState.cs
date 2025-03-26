using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class MoveState : BaseState
{
   
    public MoveState(PlayerStateMachine ctx, StateFactory factory) : base(ctx, factory)
    {
    }

    public override void EnterState() { }

    public override void UpdateState()
    {
        CheckSwitchState();
    }

    public override void FixedState()
    {
        ctx._velocity = ctx.MovementVector() * ctx._walkingSpeed + ctx._player_Up * ctx._player_Up_velocity ;
        ctx.FaceCamera();
    }


    public override void ExitState() { }
    public override void CheckSwitchState()
    {   //idle dash jump  slide fall
        
        //Fall
        if (!ctx._isGrounded) 
        {
            SwitchState(factory.Fall());
            return;
        }
        //Idle
        if (ctx.MoveDir().magnitude == 0f)
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
