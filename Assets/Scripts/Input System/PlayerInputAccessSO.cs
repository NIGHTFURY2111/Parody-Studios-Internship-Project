using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Input Accessor", menuName = "Scriptable Object/Input/Input Accessors")]

public class InputAccessSO : ScriptableObject
{
    public InputReaderSO ReadInput;

    InputBuffer<object> buffer => ReadInput.InputBuffer;
    public bool ConsumeInput(string ActionName) => buffer.MarkInputAsUsed(ActionName);

    public string ListInputs() => buffer.List();


    #region Movement
    public bool Movement( out object value ) 
    {
        bool output = buffer.GetInput("Movement", out value);
        return output;
    }

    public bool Movement()
    {
        return Movement(out object _);
    }
    #endregion

    #region Camera
    public bool Camera( out Vector2 value ) 
    {
        bool output = buffer.GetInput("Camera", out object val);
        value = (Vector2)val;
        return output;

    }

    public bool Camera()
    {
        return Camera(out Vector2 val);
    }
    #endregion

    #region Jump
    public bool Jump( out object value ) 
    {
        bool output = buffer.GetInput("Jump", out value);
        return output;

    }

    public bool Jump()
    {
        return Jump(out object _);
    }
    #endregion

    #region Dash
    public bool Dash( out object value ) 
    {
        bool output = buffer.GetInput("Dash", out value);
        return output;

    }

    public bool Dash()
    {
        return Dash(out object _);
    }
    #endregion

    #region Slide
    public bool Slide( out object value ) 
    {
        bool output = buffer.GetInput("Slide", out value);
        return output;

    }

    public bool Slide()
    {
        return Slide(out object _);
    }
    #endregion

    #region Shoot
    public bool Shoot( out object value ) 
    {
        bool output = buffer.GetInput("Shoot", out value);
        return output;

    }

    public bool Shoot()
    {
        return Shoot(out object _);
    }
    #endregion

    #region Grapple
    public bool Grapple( out object value ) 
    {
        bool output = buffer.GetInput("Grapple", out value);
        return output;

    }

    public bool Grapple()
    {
        return Grapple(out object _);
    }
    #endregion
}
