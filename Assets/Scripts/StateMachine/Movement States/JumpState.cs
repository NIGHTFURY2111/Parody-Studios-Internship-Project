using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class JumpState : BaseState
{
    float tgtVelocity;
    bool jumpCompleted;
    //private float speed;
    //private bool slideCheck = false;

    float test;
    public JumpState(PlayerStateMachine ctx, StateFactory factory) : base(ctx, factory)
    {
        //instance = this;
    }
   
    public override void EnterState()
    {
        //speed = (slideCheck)? ctx._slideSpeed:ctx._walkingSpeed;


        //tgtVelocity = ctx._getPCC._velocityMagnitude;

        jumpCompleted = false;



        ctx._moveDirectionY = ctx._jumpSpeed;



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
        //idle
        SwitchState( ctx._isGrounded ? factory.Idle(): factory.Fall());

        return;    
    }

}
