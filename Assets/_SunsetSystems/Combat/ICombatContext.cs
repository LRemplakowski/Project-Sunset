using System.Collections.Generic;
using SunsetSystems.Abilities;
using SunsetSystems.ActorResources;
using SunsetSystems.Equipment;
using UnityEngine;

namespace SunsetSystems.Combat
{
    public interface ICombatContext
    {
        GameObject GameObject { get; }
        Transform Transform { get; }
        Vector3 AimingOrigin { get; }
        Vector3Int GridPosition { get; }
        bool IsInCover { get; }
        bool IsAlive { get; }
        bool IsPlayerControlled { get; }
        bool IsUsingPrimaryWeapon { get; }
        bool IsSelectedWeaponUsingAmmo { get; }
        int SelectedWeaponDamageBonus { get; }
        int SelectedWeaponCurrentAmmo { get; }
        int SelectedWeaponMaxAmmo { get; }

        IMovementPointUser MovementManager { get; }
        IActionPointUser ActionPointManager { get; }
        IBloodPointUser BloodPointManager { get; }
        IAbilityUser AbilityUser { get; }
        IWeaponManager WeaponManager { get; }

        IEnumerable<ICover> CurrentCoverSources { get; }

        int GetAttributeValue(AttributeType attribute);
        int GetSkillValue(SkillType skill);
    }
}
