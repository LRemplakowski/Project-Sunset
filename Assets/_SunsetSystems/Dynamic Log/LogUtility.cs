using SunsetSystems.Abilities;
using SunsetSystems.Combat;
using SunsetSystems.Inventory.Data;
using SunsetSystems.Localization;
using UnityEngine;

namespace SunsetSystems.DynamicLog
{
    public static class LogUtility
    {
        public static string LogMessageFromAbilityDamage(IAbilityConfig ability, IAbilityContext context, int damage)
        {
            if (context.SourceCombatBehaviour is not INamedObject namedAttacker)
            {
                Debug.LogError($"Creating log message from ability damage failed! SourceCombatBehaviour {context.SourceCombatBehaviour} is not ICombatant!");
                return "";
            }
            if (context.TargetObject is not INamedObject namedTarget)
            {
                Debug.LogError($"Creating log message from ability damage failed! TargetObject {context.TargetObject} is not ITargetable!");
                return "";
            }
            AttackResult fakeResult = new(0, 0, 0, 0, 0, damage, 0, damage, true, false);
            string casterName = namedAttacker.GetLocalizedName().Trim();
            string targetName = namedTarget.GetLocalizedName().Trim();
            string abilityName = ability.GetAbilityUIData().GetLocalizedName().Trim();
            return $"{casterName} uses {abilityName} on {targetName} and deals {fakeResult.AdjustedDamage} damage!";
        }

        public static string LogMessageFromAttackDamge(ICombatant attacker, ITargetable target, int damage)
        {
            AttackResult fakeResult = new(0, 0, 0, 0, 0, damage, 0, damage, true, false);
            return LogMessageFromAttackResult(attacker, target, in fakeResult);
        }

        public static string LogMessageFromAttackResult(ICombatant attacker, ITargetable target, in AttackResult attack)
        {
            if (attacker is not INamedObject namedAttacker)
            {
                Debug.LogError($"Creating log message from attack result failed! Attacker {attacker} is not INamedObject!");
                return "";
            }
            if (target is not INamedObject namedTarget)
            {
                Debug.LogError($"Creating log message from attack result failed! Target {target} is not INamedObject!");
                return "";
            }

            var attackerName = namedAttacker.GetLocalizedName();
            var targetName = namedTarget.GetLocalizedName();
            string result;
            if (attack.Successful)
            {
                if (attack.Critical)
                {
                    result = $"{attackerName.Trim()} critically hits {targetName.Trim()} and deals {attack.AdjustedDamage} damage!";
                }
                else
                {
                    result = $"{attackerName.Trim()} hits {targetName.Trim()} and deals {attack.AdjustedDamage} damage!";
                }
            }
            else
            {
                result = $"{attackerName.Trim()} misses {targetName.Trim()}!";
            }
            return result;
        }

        public static string LogMessageFromItemPickup(IBaseItem item)
        {
            return $"Item gained: {item.Name}";
        }

        public static string LogMessageFromItemLoss(IBaseItem item)
        {
            return $"Item lost: {item.Name}";
        }
    }
}
