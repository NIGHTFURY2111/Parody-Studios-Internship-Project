using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class FallState : BaseState
{
    float timeInAir;
    public FallState(PlayerStateMachine ctx, StateFactory factory) : base(ctx, factory)
    {
    }

    public override void EnterState() 
    { 
        timeInAir = ctx._TimerInAirBeforeGameOver;
        //ctx._anim.Play("Fall");
        ctx._anim.SetBool("IsGrounded", false);
    }

    public override void FixedState()
    {
        ctx._force(ctx.MovementVector() * ctx._forceAppliedInAir, ForceMode.Acceleration);
    }

    public override void UpdateState()
    {
        if (timeInAir > 0f) {
            timeInAir -= Time.deltaTime;
        }
        else if (timeInAir <= 0f)
        {
            ctx._gameManager.GameLost();
        }


        CheckSwitchState();
    }


    public override void ExitState() { ctx._anim.SetBool("IsGrounded", true); }
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

