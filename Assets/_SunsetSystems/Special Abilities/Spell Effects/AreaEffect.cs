using System.Linq;
using Sirenix.OdinInspector;
using SunsetSystems.ActionSystem;
using SunsetSystems.Combat;
using SunsetSystems.Combat.Grid;
using SunsetSystems.Entities.Interfaces;
using UnityEngine;

namespace SunsetSystems.Abilities
{
    [CreateAssetMenu(fileName = "New Area Effect", menuName = "Sunset Abilities/Effects/Area Effect")]
    public class AreaEffect : SerializedScriptableObject, IAbilityEffect
    {
        //[SerializeField]
        //private AreaShape _shape;
        [SerializeField]
        private LayerMask _targetLayer;
        [SerializeField]
        private TargetableEntityType _targetableType;
        [SerializeField]
        private int _areaSize;
        [SerializeField]
        private IAbilityEffect _effect;

        public void ResolveEffect(IAbilityConfig ability, IAbilityContext context)
        {
            var overlapingTargets = Physics.OverlapSphere(context.TargetObject.ProjectileTarget.position, _areaSize, _targetLayer)
                                           .Select(FindTargetable)
                                           .Where(targetable => targetable != null && targetable.IsValidTarget(context.SourceCombatBehaviour, _targetableType))
                                           .ToList();
            foreach (var target in overlapingTargets)
            {
                _effect.ResolveEffect(ability, new AbilityContext(context, target));
            }
        }

        private static ITargetable FindTargetable(Collider collider)
        {
            if (collider.TryGetComponent(out IEntityReferences references))
            {
                if (references.GetCachedComponentInChildren<ITargetable>() is ITargetable targetable)
                {
                    return targetable;
                }
            }
            return null;
        }

        private enum AreaShape
        {
            Circle,
            Cone,
            Line,
            Rectangle
        }

        private class AbilityContext : IAbilityContext
        {
            private readonly IAbilityContext _baseContext;
            private readonly ITargetable _targetObject;
            public IActionPerformer SourceActionPerformer => _baseContext.SourceActionPerformer;
            public ICombatant SourceCombatBehaviour => _baseContext.SourceCombatBehaviour;
            public ITargetable TargetObject => _targetObject;
            public GridManager GridManager => _baseContext.GridManager;

            public AbilityContext(IAbilityContext baseContext, ITargetable target)
            {
                _baseContext = baseContext;
                _targetObject = target;
            }
        }
    }
}
