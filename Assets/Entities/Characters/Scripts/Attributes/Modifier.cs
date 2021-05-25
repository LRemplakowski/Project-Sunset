using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modifier
{
    public ModifierType Type { get; private set; }
    public int Value { get; private set; }

    public Modifier(int value, ModifierType type)
    {
        this.Type = type;
        this.Value = value;
    }
}
