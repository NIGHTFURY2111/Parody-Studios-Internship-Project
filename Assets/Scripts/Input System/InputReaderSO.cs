using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Object/Input/Input Reader")]
public class InputReaderSO : ScriptableObject, PlayerInputAction.IPlayerActions
{
    [SerializeField] float defaultBufferTime;
    private InputBuffer<object> _InputBuffer;
    public InputBuffer<object> InputBuffer => _InputBuffer; 

    PlayerInputAction input;
    private void OnEnable()
    {
        if (input == null)
        {
            input = new PlayerInputAction();
            input.Player.SetCallbacks(this);
        }
        input.Player.Enable();

        if (_InputBuffer == null)
            _InputBuffer = new();
    }

    private void OnDisable()
    {
        if (input != null) input.Player.Disable();
    }

    public void OnCamera(InputAction.CallbackContext context)
    {
        StoreInputVector2(context);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        StoreInputBool(context);
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        StoreInputVector2(context);
    }

    public void OnSwitchGravity(InputAction.CallbackContext context)
    {
        StoreInputBool(context);
    }

    void StoreInputBool(InputAction.CallbackContext context, float bufferTime = float.NaN)
    {
        if (context.action.WasPerformedThisFrame())
        {
            //there is no actual way to check if the input is a bool or not thanks to unity in all its glory
            _InputBuffer.AddInput(context.action.name, context.ReadValue<float>()>0.5f, bufferTime == float.NaN ? defaultBufferTime : bufferTime);
        }
    }

    void StoreInputFloat(InputAction.CallbackContext context, float bufferTime = float.NaN)
    {
        if (context.action.WasPerformedThisFrame())
        {
            _InputBuffer.AddInput(context.action.name, context.ReadValue<float>(), bufferTime == float.NaN ? defaultBufferTime : bufferTime);
        }
    }

    void StoreInputVector2(InputAction.CallbackContext context, float bufferTime = float.NaN)
    {
        _InputBuffer.AddInput(context.action.name, context.ReadValue<Vector2>(), bufferTime == float.NaN ? defaultBufferTime : bufferTime);
    }
}
