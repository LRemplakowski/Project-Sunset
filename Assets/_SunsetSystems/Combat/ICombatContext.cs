using System.Collections.Generic;
using SunsetSystems.Abilities;
using SunsetSystems.ActorResources;
using SunsetSystems.Equipment;
using UnityEngine;

namespace SunsetSystems.Combat
{
    public interface ITargetableContext
    {
        GameObject GameObject { get; }
        Transform Transform { get; }
        Vector3Int GridPosition { get; }
    }

    public interface ICombatContext : ITargetableContext
    {
        Vector3 AimingOrigin { get; }
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

    public class DefaultCombatContext : ICombatContext
    {
        private readonly GameObject _gameObject;

        public virtual GameObject GameObject => _gameObject;
        public virtual Transform Transform => GameObject.transform;
        public virtual Vector3 AimingOrigin => Transform.position;
        public virtual Vector3Int GridPosition => CombatManager.Instance.CurrentEncounter.GridManager.WorldPositionToGridPosition(Transform.position);
        public virtual bool IsInCover => false;
        public virtual bool IsAlive => false;
        public virtual bool IsPlayerControlled => false;
        public virtual bool IsUsingPrimaryWeapon => false;
        public virtual bool IsSelectedWeaponUsingAmmo => false;
        public virtual int SelectedWeaponDamageBonus => 0;
        public virtual int SelectedWeaponCurrentAmmo => 0;
        public virtual int SelectedWeaponMaxAmmo => 0;
        public virtual IMovementPointUser MovementManager => null;
        public virtual IActionPointUser ActionPointManager => null;
        public virtual IBloodPointUser BloodPointManager => null;
        public virtual IAbilityUser AbilityUser => null;
        public virtual IWeaponManager WeaponManager => null;
        public virtual IEnumerable<ICover> CurrentCoverSources => new List<ICover>();

        public virtual int GetAttributeValue(AttributeType attribute) => 0;
        public virtual int GetSkillValue(SkillType skill) => 0;

        public DefaultCombatContext(GameObject go)
        {
            _gameObject = go;
        }
    }

}
