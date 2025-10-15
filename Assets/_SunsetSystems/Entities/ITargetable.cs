using SunsetSystems.Abilities;
using SunsetSystems.Entities;
using UnityEngine;

namespace SunsetSystems.Combat
{
    public interface ITargetable : IContextProvider<ITargetableContext>
    {
        Transform ProjectileTarget { get; }
        bool IsValidTarget(TargetableEntityType validTargetsFlag);
    }
}
