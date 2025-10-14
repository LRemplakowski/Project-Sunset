using SunsetSystems.UMA;
using SunsetSystems.Entities.Characters.Navigation;
using SunsetSystems.Entities.Interfaces;
using SunsetSystems.Equipment;
using SunsetSystems.Combat;
using SunsetSystems.Abilities;
using SunsetSystems.Animation;
using SunsetSystems.ActorResources;
using UnityEngine;

namespace SunsetSystems.Entities.Characters
{
    public interface ICreatureReferences : IEntityReferences
    {
        CreatureData CreatureData { get; }
        StatsManager StatsManager { get; }
        INavigationManager NavigationManager { get; }
        ICombatant CombatBehaviour { get; }
        ITargetable Targetable { get; }
        IEquipmentManager EquipmentManager { get; }
        IWeaponManager WeaponManager { get; }
        IUMAManager UMAManager { get; }
        ISpellbookManager SpellbookManager { get; }
        IAnimationManager AnimationManager { get; }
        IAbilityUser AbilityUser { get; }
        IMovementPointUser MovementManager { get; }
        IActionPointUser ActionPointManager { get; }
        IBloodPointUser BloodPointManager { get; }
        Transform Body { get; }
    }
}
