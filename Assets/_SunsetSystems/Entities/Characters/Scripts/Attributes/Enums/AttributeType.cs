using System;

[Flags]
public enum AttributeType
{
    Strength = 1,
    Dexterity = 1 << 1,
    Stamina = 1 << 2,
    Charisma = 1 << 3,
    Manipulation = 1 << 4,
    Composure = 1 << 5,
    Intelligence = 1 << 6,
    Wits = 1 << 7,
    Resolve = 1 << 8,
    Speed = 1 << 9,
    Invalid = 0
}
