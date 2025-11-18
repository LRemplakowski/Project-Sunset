using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using SunsetSystems.Abilities;
using SunsetSystems.ActorResources;
using SunsetSystems.Combat;
using SunsetSystems.Combat.Grid;
using SunsetSystems.Entities;
using SunsetSystems.Equipment;
using SunsetSystems.Game;
using SunsetSystems.Inventory;
using SunsetSystems.Utils.Extensions;
using UnityEngine;

namespace SunsetSystems.AI
{
    public class AIBehaviourContext : SerializedMonoBehaviour
    {
        [Title("Config")]
        [SerializeField]
        private ICombatant _combatBehaviour;
        [SerializeField]
        private IContextProvider<ICombatContext> _combatContextSource;
        [SerializeField]
        private IFactionMember _thisFaction;
        [SerializeField]
        private IMovementPointUser _movementPointUser;
        [SerializeField]
        private IActionPointUser _actionPointUser;
        [SerializeField]
        private IBloodPointUser _bloodPointUser;

        private ICombatContext CombatContext => _combatContextSource.GetContext();
        [Title("Runtime")]
        [ShowInInspector, ReadOnly]
        public IAbilityConfig SelectedAbility { get; set; }
        [ShowInInspector, ReadOnly]
        public ITargetable SelectedTarget { get; set; }
        [ShowInInspector, ReadOnly]
        public IGridCell SelectedPosition { get; set; }

        private CombatManager _combatManager;
        private GameManager _gameManager;

        private void Start()
        {
            _combatManager = CombatManager.Instance;
            _gameManager = GameManager.Instance;
        }

        public bool IsMyTurn()
        {
            return _combatManager.IsCurrentActiveActor(_combatBehaviour);
        }

        public bool CanMove()
        {
            return _combatBehaviour.HasActionsQueued is false && CombatContext.MovementManager.GetCanMove();
        }

        public bool IsTurnMode()
        {
            return _gameManager.IsCurrentState(GameState.Combat);
        }

        public bool IsInCover()
        {
            return CombatContext.IsInCover;
        }

        public bool IsCurrentTargetInAbilityRange()
        {
            return IsInAbilityRange(SelectedAbility, CombatContext, SelectedTarget);
        }

        public IAbilityUser GetAbilityUser() => CombatContext.AbilityUser;
        public ICombatant GetCombatant() => _combatBehaviour;

        public int GetTargetsInWeaponRange()
        {
            return _combatManager.LivingActors.Count(actor =>
            {
                return actor is ITargetable targetable
                       && IsHostileToMe(targetable)
                       && IsInAbilityRange(SelectedAbility, CombatContext, targetable);
            });
        }

        private bool IsHostileToMe(ITargetable target)
        {
            if (target is not IFactionMember factionMember)
                return false;
            return factionMember.IsHostileTowards(_thisFaction);
        }

        private static bool IsInAbilityRange(IAbilityConfig ability, ICombatContext attacker, ITargetable target)
        {
            if (ability == null || target == null || attacker == null)
                return false;
            var abilityUser = attacker.AbilityUser;
            abilityUser.SetCurrentTargetObject(target);
            var abilityTargetingData = ability.GetTargetingData(abilityUser.GetCurrentAbilityContext());
            Vector3Int attackerPosition = attacker.GridPosition;
            Vector3Int targetPosition = target.GetContext().GridPosition;
            var gridDistance = Vector3Int.Distance(attackerPosition, targetPosition);
            bool result = gridDistance <= abilityTargetingData.GetRangeData().MaxRange + .5f;
            return result;
        }

        public bool SelectNextPosition()
        {
            var lastSelectedPosition = SelectedPosition;
            var movementRange = _movementPointUser.GetCurrentMovementPoints() / 2;
            var gridManager = _combatManager.CurrentEncounter.GridManager;
            var positionsInRange = AIHelpers.GetPositionsInRange(_combatBehaviour, movementRange, gridManager);
            var selectedAbilityRange = SelectedAbility.GetTargetingData(CombatContext.AbilityUser.GetCurrentAbilityContext());
            if (positionsInRange.Count() > 0)
            {
                SelectedPosition = SelectPositionByWeapon(positionsInRange, selectedAbilityRange, SelectedTarget);
            }
            return lastSelectedPosition != SelectedPosition;
        }

        static IGridCell SelectPositionByWeapon(IEnumerable<IGridCell> gridCells, IAbilityTargetingData weapon, ITargetable target)
        {
            if (target == null || weapon == null)
                return gridCells.GetRandom();
            var rangeData = weapon.GetRangeData();
            float maxRange = rangeData.MaxRange + .5f;
            IGridCell result = null;
            switch (weapon.GetRangeType())
            {
                case AbilityRange.Melee:
                    result = gridCells.Where(cell => Vector3Int.Distance(target.GetContext().GridPosition, cell.GridPosition) <= maxRange)
                                      .GetRandom();
                    result ??= gridCells.OrderBy(cell => Vector3Int.Distance(target.GetContext().GridPosition, cell.GridPosition))
                                        .FirstOrDefault();
                    break;
                case AbilityRange.Ranged:
                    result = gridCells.Where(cell => Vector3Int.Distance(target.GetContext().GridPosition, cell.GridPosition) <= maxRange)
                                      .Where(cell => Vector3Int.Distance(target.GetContext().GridPosition, cell.GridPosition) >= rangeData.ShortRange)
                                      .GetRandom();
                    break;
            }
            result ??= gridCells.GetRandom();
            return result;
        }

        public ITargetable[] GetAllHostiles()
        {
            return _combatManager.LivingActors
                .Where(actor => IsHostileToMe(actor as ITargetable))
                .Select(actor => actor as ITargetable)
                .ToArray();
        }

        public WeaponAmmoData GetSelectedWeaponAmmoData()
        {
            return _combatBehaviour.References.WeaponManager.GetSelectedWeaponAmmoData();
        }

        public IWeapon GetWeapon()
        {
            return _combatBehaviour.References.WeaponManager.GetSelectedWeapon();
        }

        public void ReloadAmmo()
        {
            _combatBehaviour.References.WeaponManager.ReloadSelectedWeapon();
        }

        public bool GetHasEnoughActionPoints(IAbilityConfig selectedAbility)
        {
            var abilityUser = CombatContext.AbilityUser;
            abilityUser.SetCurrentTargetObject(SelectedTarget);
            return abilityUser.GetCanAffordAbility(selectedAbility);
        }
    }
}
