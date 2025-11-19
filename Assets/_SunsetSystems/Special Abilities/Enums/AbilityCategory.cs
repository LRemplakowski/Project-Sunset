using System;

namespace SunsetSystems.Abilities
{
    [Flags]
    public enum AbilityCategory
    {
        Movement = 1 << 0, 
        Attack = 1 << 1, 
        Support = 1 << 2, 
        Magical = 1 << 3, 
        Debuff = 1 << 4,

        Dangerous = Attack | Debuff
    }
}
