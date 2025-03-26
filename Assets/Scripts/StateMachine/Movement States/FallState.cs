using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class FallState : BaseState
{
    public FallState(PlayerStateMachine ctx, StateFactory factory) : base(ctx, factory)
    {
    }

    public override void EnterState()
    {
    }

    public override void FixedState()
    {
        ctx._force(ctx.MovementVector() * ctx._forceAppliedInAir, ForceMode.Acceleration);
    }

    public override void UpdateState()
    {
        CheckSwitchState();
    }


    public override void ExitState()
    {
    }
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

