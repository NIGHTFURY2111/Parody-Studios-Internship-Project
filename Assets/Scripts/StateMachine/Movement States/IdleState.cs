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

    public override void EnterState()
    {
        ctx._velocity = Vector3.zero;
        //ctx._anim.Play("Idle");
        ctx._anim.SetBool("IsGrounded", true);
        ctx._anim.SetFloat("Speed", 0f);
    }

    public override void UpdateState()
    {
        CheckSwitchState();
    }

    public override void ExitState() { }
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
