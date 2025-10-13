using SunsetSystems.Abilities;
using SunsetSystems.Localization;
using UnityEngine;

namespace SunsetSystems.Combat
{
    public interface ITargetable
    {
        Transform ProjectileTarget { get; }
        bool IsValidTarget(TargetableEntityType validTargetsFlag);
    }
}
