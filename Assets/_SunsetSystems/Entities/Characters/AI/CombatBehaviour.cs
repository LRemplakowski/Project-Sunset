using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using SunsetSystems.Abilities;
using SunsetSystems.ActionSystem;
using SunsetSystems.Animation;
using SunsetSystems.Entities;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Equipment;
using SunsetSystems.Localization;
using UltEvents;
using UnityEngine;

namespace SunsetSystems.Combat
{
    public class CombatBehaviour : SerializedMonoBehaviour, ICombatant, ITargetable, IFactionMember, IDamageable, INamedObject
    {
        [Title("Config")]
        [SerializeField]
        private Vector3 _combatNameplateOffset = new(0, 2, 0);

        [Title("References")]
        [SerializeField]
        private Transform _raycastOrigin;
        public Vector3 RaycastOrigin => _raycastOrigin.position;
        [SerializeField, Required]
        private IWeaponManager _weaponManager;
        [field: SerializeField, Required]
        public ICreature Owner { get; private set; }
        [SerializeField, Required]
        private AnimationManager _animationController;

        private ICombatContext _combatContext;

        #region Unity Messages
        private void OnEnable()
        {
            CombatManager.OnCombatStart += OnCombatBegin;
            CombatManager.OnCombatRoundBegin += OnCombatRoundBegin;
            CombatManager.OnCombatRoundEnd += OnCombatRoundEnd;
            CombatManager.OnCombatEnd += OnCombatEnd;
        }

        private void Start()
        {
            _combatContext = new CombatContext(this);
        }

        private void OnDisable()
        {
            CombatManager.OnCombatRoundBegin -= OnCombatRoundBegin;
            CombatManager.OnCombatRoundEnd -= OnCombatRoundEnd;
            CombatManager.OnCombatStart -= OnCombatBegin;
            CombatManager.OnCombatEnd -= OnCombatEnd;
        }

        private void Update()
        {
            if (!CombatManager.Instance.IsCurrentActiveActor(this))
                return;
            if (HasActionsQueued || CanMove(this) || CanAct(this) || GetContext().IsPlayerControlled)
                return;
            SignalEndTurn();

            static bool CanMove(ICombatant combatant) => combatant.GetContext().MovementManager.GetCanMove();
            static bool CanAct(ICombatant combatant) => combatant.GetContext().ActionPointManager.CanUseActionPoints();
        }
        #endregion

        private void OnCombatBegin(IEnumerable<ICombatant> actorsInCombat)
        {
            _weaponManager.SetSelectedWeapon(SelectedWeapon.Primary);
            if (actorsInCombat.Contains(this))
            {
                _animationController.SetCombatAnimationsActive(true);
            }
        }

        private void OnCombatEnd(IEnumerable<ICombatant> _)
        {
            _animationController.SetCombatAnimationsActive(false);
        }

        private void OnCombatRoundBegin(ICombatant currentActor)
        {

        }

        private void OnCombatRoundEnd(ICombatant currentActor)
        {

        }

        #region ITargetable
        public Transform ProjectileTarget => _raycastOrigin;

        public bool IsValidTarget(TargetableEntityType validTargetsFlag) 
        {
            return References.CreatureData.CreatureType switch
            {
                CreatureType.Mortal => validTargetsFlag.HasFlag(TargetableEntityType.Mortal),
                CreatureType.Ghul => validTargetsFlag.HasFlag(TargetableEntityType.Ghoul),
                CreatureType.Vampire => validTargetsFlag.HasFlag(TargetableEntityType.Vampire),
                _ => false,
            };
        }
        #endregion

        #region IDamageable
        public void TakeDamage(int amount)
        {
            Owner.References.StatsManager.TakeDamage(amount);
            OnDamageTaken?.InvokeSafe(this);
        }
        #endregion

        #region ICombatant
        public MonoBehaviour CoroutineRunner => Owner.CoroutineRunner;

        public Vector3 AimingOrigin => RaycastOrigin;
        public Vector3 NameplatePosition => References.Transform.position + _combatNameplateOffset;

        [field: Title("Events")]
        [field: SerializeField]
        public UltEvent<ICombatant> OnDamageTaken { get; set; }

        public ICreatureReferences References => Owner.References;

        public EntityAction PeekCurrentAction => Owner.PeekCurrentAction;

        public bool HasActionsQueued => Owner.HasActionsQueued;

        public Transform Transform => Owner.Transform;

        public Awaitable PerformAction(EntityAction action, bool clearQueue = false)
        {
            return Owner.PerformAction(action, clearQueue);
        }

        public void SignalEndTurn()
        {
            if (CombatManager.Instance.IsCurrentActiveActor(this))
                CombatManager.Instance.NextRound();
        }
        #endregion

        #region INamedObject
        public string GetLocalizedName() => References.CreatureData.FullName;
        #endregion

        #region IContextProvider
        public ICombatContext GetContext()
        {
            return _combatContext;
        }
        #endregion

        #region IFactionMember
        public Faction GetFaction()
        {
            return Owner.References.CreatureData.Faction;
        }

        public bool IsFriendlyTowards(IFactionMember other)
        {
            return Owner.References.CreatureData.Faction switch
            {
                Faction.None => false,
                Faction.Hostile => other.GetFaction() is Faction.Hostile,
                Faction.Neutral => false,
                Faction.Friendly => other.GetFaction() is Faction.Friendly || other.GetFaction() is Faction.PlayerControlled,
                Faction.PlayerControlled => other.GetFaction() is Faction.Friendly || other.GetFaction() is Faction.PlayerControlled,
                Faction.AttackAll => false,
                _ => false,
            };
        }

        public bool IsHostileTowards(IFactionMember other)
        {
            return Owner.References.CreatureData.Faction switch
            {
                Faction.None => false,
                Faction.Hostile => other.GetFaction() is not Faction.Hostile,
                Faction.Neutral => false,
                Faction.Friendly => other.GetFaction() is Faction.Hostile || other.GetFaction() is Faction.AttackAll,
                Faction.PlayerControlled => other.GetFaction() is Faction.Hostile || other.GetFaction() is Faction.AttackAll,
                Faction.AttackAll => true,
                _ => false,
            };
        }

        public bool IsMe(IFactionMember other)
        {
            return other?.Equals(this) ?? false;
        }
        #endregion

        public override string ToString()
        {
            return $"{base.ToString()} - {Owner}";
        }
    }
}


