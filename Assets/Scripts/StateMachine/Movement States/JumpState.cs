using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class JumpState : BaseState
{
    bool jumpCompleted;

    public JumpState(PlayerStateMachine ctx, StateFactory factory) : base(ctx, factory)
    {
    }
   
    public override void EnterState()
    {
        jumpCompleted = false;

        ctx.StartCoroutine(Jumping());
    }
    public override void FixedState()
    {
    }

    public override void UpdateState()
    {
        if (jumpCompleted) 
        {
            CheckSwitchState();
        }
        
    }
    public override void ExitState()
    {
    }

    IEnumerator Jumping()
    {
        ctx._velocity += ctx._player_Up * ctx._jumpSpeed;
        yield return new WaitForSecondsRealtime(0.2f);     
        CheckSwitchState();
        jumpCompleted = true;
    }

    public override void CheckSwitchState()
    {   
        //idle OR Fall
        SwitchState( ctx._isGrounded ? factory.Idle(): factory.Fall());

        return;    
    }

}
