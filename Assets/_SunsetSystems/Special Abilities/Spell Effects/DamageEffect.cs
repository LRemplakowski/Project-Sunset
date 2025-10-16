using Sirenix.OdinInspector;
using SunsetSystems.Combat;
using SunsetSystems.DynamicLog;
using UnityEngine;

namespace SunsetSystems.Abilities
{

    [CreateAssetMenu(fileName = "New Damage Effect", menuName = "Sunset Abilities/Effects/Damage Effect")]
    public class DamageEffect : SerializedScriptableObject, IAbilityEffect
    {
        [SerializeField]
        private AttributeType _damageAttribute;
        [SerializeField]
        private SkillType _damageSkill;
        [SerializeField]
        private int _baseDamage = 1;

        public void ResolveEffect(IAbilityConfig ability, IAbilityContext context)
        {
            if (context.TargetObject is IDamageable damageable)
            {
                var damage = GetDamage(ability, context);
                damageable.TakeDamage(damage);
                string logMessage = LogUtility.LogMessageFromAbilityDamage(ability, context, damage);
                DynamicLogManager.Instance.PostLogMessage(logMessage);
            }
            else
            {
                Debug.Log($"Target object {context.TargetObject} is not damageable.", context.TargetObject as UnityEngine.Object);
            }
        }

        private int GetDamage(IAbilityConfig ability, IAbilityContext context)
        {
            int attributeBonus = context.SourceCombatBehaviour.References.StatsManager.GetAttribute(_damageAttribute).Value;
            int skillBonus = context.SourceCombatBehaviour.References.StatsManager.GetSkill(_damageSkill).Value;
            int powerLevel = 0;
            if (ability is SpellAbility spellAbility)
                powerLevel = spellAbility.Level;
            return _baseDamage * (attributeBonus + skillBonus + powerLevel);
        }
    }
}
