using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

#region InputStruct defination
struct InputStruct<T>
{
    string name;
    T value;
    float time; 
    float bufferTime;
    bool used;

    public InputStruct(string name, T value, float bufferTime)
    {
        this.name = name;
        this.time = Time.time;
        this.value = value;
        this.used = false;
        this.bufferTime = bufferTime;
    }

    public string Name => name;
    public float InputTime => time;
    public T Value => value;
    public bool Used => used;
    public float BufferTime => bufferTime;

    public void UpdateValue(T value)
    {
        time = Time.time;
        this.value = value;
        used = false;
    }

    public void UpdateBufferTime(float newBufferTime)
    {
        bufferTime = newBufferTime;
    }

    public void MarkAsUsed()
    {
        used = true;
    }

    /// <summary>
    /// Checks if the input is expired based on its buffer time.
    /// </summary>
    public bool IsExpired()
    {
        return Time.time - time > bufferTime;
    }
}
#endregion

public class InputBuffer<T>
{

    private Dictionary<string, InputStruct<T>> buffer = new ();

    /// <summary>
    /// Adds or updates an input in the buffer with a specific buffer time.
    /// </summary>
    public void AddInput(string name, T value, float bufferTime)
    {
        if (buffer.ContainsKey(name))
        {
            // Update existing input
            InputStruct<T> input = buffer[name];
            input.UpdateValue(value);
            input.UpdateBufferTime(bufferTime); // Optionally update buffer time
            buffer[name] = input;
        }
        else
        {
            // Add new input
            buffer[name] = new InputStruct<T>(name, value, bufferTime);
        }
    }

    /// <summary>
    /// Gets an input from the buffer if it exists and hasn't expired.
    /// </summary>
    public bool GetInput(string name, out T value)
    {
        if (buffer.ContainsKey(name))
        {
            InputStruct<T> input = buffer[name];

            if (!input.Used && !input.IsExpired())
            {
                value = input.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Marks the input as used without reading its value.
    /// </summary>
    public bool MarkInputAsUsed(string name)
    {
        if (!buffer.TryGetValue(name, out InputStruct<T> input))
            return false; // Input does not exist

        if (input.IsExpired())
            return false; // Input expired, cannot be marked as used

        input.MarkAsUsed();
        buffer[name] = input; // Ensure struct update is applied

        return true; // Successfully marked as used
    }


    /// <summary>
    /// Clears the entire buffer.
    /// </summary>
    public void Clear()
    {
        buffer.Clear();
    }

    /// <summary>
    /// Lists all inputs in the buffer (for debugging purposes).
    /// </summary>
    public string List()
    {
        string outp = "";
        foreach (var kvp in buffer)
        {
            var input = kvp.Value;
            outp += $"{input.Name}\t{input.Value}\t{input.Used}\t{input.InputTime}\t{input.BufferTime}\n";
            //outp += kvp.ToString()+"\n";
        }
        return outp;
    }
}

