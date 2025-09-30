using System.Collections.Generic;
using System.Linq;
using SunsetSystems.Abilities;
using SunsetSystems.ActorResources;
using SunsetSystems.Equipment;
using UnityEngine;

namespace SunsetSystems.Combat
{
    public class CombatContext : ICombatContext
    {
        private readonly ICombatant _source;

        public CombatContext(ICombatant source)
        {
            _source = source;
        }

        public GameObject GameObject => _source.References.GameObject;

        public Transform Transform => _source.References.Transform;

        public Vector3 AimingOrigin => _source.AimingOrigin;

        public Vector3Int GridPosition => CombatManager.Instance.CurrentEncounter.GridManager.WorldPositionToGridPosition(Transform.position);

        public bool IsInCover => CurrentCoverSources.Count() > 0;

        public bool IsAlive => _source.References.StatsManager.IsAlive();

        public bool IsPlayerControlled => _source.References.CreatureData.Faction is Faction.PlayerControlled;

        public bool IsUsingPrimaryWeapon => WeaponManager.GetSelectedWeapon().Equals(WeaponManager.GetPrimaryWeapon());

        public bool IsSelectedWeaponUsingAmmo => WeaponManager.GetSelectedWeapon().GetWeaponUsesAmmo();

        public int SelectedWeaponDamageBonus => WeaponManager.GetSelectedWeapon().GetDamageData().DamageModifier;

        public int SelectedWeaponCurrentAmmo => WeaponManager.GetSelectedWeaponAmmoData().CurrentAmmo;

        public int SelectedWeaponMaxAmmo => WeaponManager.GetSelectedWeaponAmmoData().MaxAmmo;

        public IMovementPointUser MovementManager => _source.References.MovementManager;

        public IActionPointUser ActionPointManager => _source.References.ActionPointManager;

        public IBloodPointUser BloodPointManager => _source.References.BloodPointManager;

        public IAbilityUser AbilityUser => _source.References.AbilityUser;

        public IWeaponManager WeaponManager => _source.References.WeaponManager;

        public IEnumerable<ICover> CurrentCoverSources => GetCoverFromCurrentPosition(Transform.position);

        public int GetAttributeValue(AttributeType attribute)
        {
            return _source.References.StatsManager.GetAttribute(attribute).Value;
        }

        public int GetSkillValue(SkillType skill)
        {
            return _source.References.StatsManager.GetSkill(skill).Value;
        }

        private static IEnumerable<ICover> GetCoverFromCurrentPosition(Vector3 worldPosition)
        {
            if (CombatManager.Instance.CurrentEncounter == null)
                return new List<ICover>();
            var gridManager = CombatManager.Instance.CurrentEncounter.GridManager;
            var gridPos = gridManager.WorldPositionToGridPosition(worldPosition);
            var gridCell = gridManager[gridPos];
            return gridCell.AdjacentCoverSources;
        }
    }
}
