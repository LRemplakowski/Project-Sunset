using SunsetSystems.Animation;
using SunsetSystems.Combat;
using SunsetSystems.Entities.Characters.Navigation;
using SunsetSystems.Equipment;
using SunsetSystems.Abilities;
using SunsetSystems.UMA;
using UnityEngine;
using SunsetSystems.ActorResources;
using Sirenix.Serialization;

namespace SunsetSystems.Entities.Characters
{
    public class CreatureReferenceManager : CachedReferenceManager, ICreatureReferences
    {
        [field: SerializeField]
        public CreatureData CreatureData { get; private set; }
        [field: SerializeField]
        public StatsManager StatsManager { get; private set; }
        [OdinSerialize]
        public Transform Body { get; private set; }
        [field: SerializeField]
        public INavigationManager NavigationManager { get; private set; }
        [field: SerializeField]
        public ICombatant CombatBehaviour { get; private set; }
        [field: SerializeField]
        public ITargetable Targetable { get; private set; }
        [field: SerializeField]
        public IEquipmentManager EquipmentManager { get; private set; }
        [field: SerializeField]
        public IWeaponManager WeaponManager { get; private set; }
        [field: SerializeField]
        public IUMAManager UMAManager { get; private set; }
        [field: SerializeField]
        public ISpellbookManager SpellbookManager { get; private set; }
        [field: SerializeField]
        public IAnimationManager AnimationManager { get; private set; }
        [field: SerializeField]
        public IAbilityUser AbilityUser { get; private set; }
        [field: SerializeField]
        public IMovementPointUser MovementManager { get; private set; }
        [field: SerializeField]
        public IActionPointUser ActionPointManager { get; private set; }
        [field: SerializeField]
        public IBloodPointUser BloodPointManager { get; private set; }
    }
}
