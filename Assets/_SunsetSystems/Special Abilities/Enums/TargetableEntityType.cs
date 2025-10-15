using System;

namespace SunsetSystems.Abilities
{
    [Flags]
    public enum TargetableEntityType
    {
        None = 0,
        Mortal = 1 << 0,
        Ghoul = 1 << 1,
        Vampire = 1 << 2,
        Object = 1 << 3,
        Position = 1 << 4,
        Self = 1 << 5,
        Dead = 1 << 6,
        Alive = 1 << 7,
        Any = int.MaxValue,
    }
}