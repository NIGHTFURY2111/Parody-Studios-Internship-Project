using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class IdleState : BaseState
{
    public IdleState(PlayerStateMachine ctx, StateFactory factory) : base(ctx, factory)
    {

    }
    public override void ExitState()
    {
       
    }

    public override void EnterState()
    {
        ctx._velocity = Vector3.zero;
    }

    public override void UpdateState()
    {
        CheckSwitchState();
    }

    public override void CheckSwitchState()
    {   //jump walk dash slide fall
        

        //Move
        if (ctx._move.IsInProgress())
        {
            SwitchState(factory.Move());
            return;
        }

        //Jump
        if (ctx._jump.WasPerformedThisFrame())
        {
            SwitchState(factory.Jump());
            return;
        }

        //fall
        if (!ctx._isGrounded)
        {
            SwitchState(factory.Fall());
            return;
        }

    }
}
