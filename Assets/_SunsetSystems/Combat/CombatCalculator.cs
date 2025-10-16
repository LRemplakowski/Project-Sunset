using System;
using UnityEngine;

namespace SunsetSystems.Combat
{
    public static class CombatCalculator
    {
        public const double BASE_HIT_CHANCE = 0.9d;
        public const double SHORT_RANGE_HIT_PENALTY = 0.5d;
        public const double BASE_CRIT_CHANCE = 0.2d;
        public const double BASE_DODGE_CHANCE = 0.05d;
        public const double COVER_MODIFIER_LOW = 0.2d;
        public const double COVER_MODIFIER_HIGH = 0.5d;

        private static readonly System.Random _random = new();

        public static AttackResult CalculateAttackResult(in IAttackContext context)
        {
            var attackModifier = context.GetBaseAttackModifier();
            attackModifier += context.GetHeightAttackModifier();
            int damage = 0;
            int damageReduction = 0;
            int adjustedDamage = 0;
            double critChance = 0;
            double critRoll = 0;
            double attackerHitChance = CalculateHitChance(context) + attackModifier.HitChanceMod;
            double defenderDodgeChance = CalculateDodgeChance(context) + attackModifier.DodgeChanceMod;
            bool hit = false;
            bool crit = false;
            hit |= attackerHitChance - defenderDodgeChance >= 1d;
            double hitRoll = _random.NextDouble() + attackModifier.HitRollMod;
            hit |= hitRoll < attackerHitChance - defenderDodgeChance;
            hit |= attackModifier.ForceSuccesfulHit;
            if (hit)
            {
                critChance = CalculateCritChance(context) + attackModifier.CritChanceMod;
                damage = CalculateAttackDamage(context) + attackModifier.DamageMod;
                critRoll = _random.NextDouble() + attackModifier.CritRollMod;
                crit |= critRoll < critChance;
                crit |= attackModifier.ForceCritical;
                if (crit)
                {
                    damage = Mathf.RoundToInt(damage * GetCriticalDamageMultiplier(context));
                }
                damageReduction = CalculateDefenderDamageReduction(context) + attackModifier.DamageReductionMod;
                adjustedDamage = (damage > damageReduction ? damage - damageReduction : 1) + attackModifier.AdjustedDamageMod;
            }
            if (context.IsPlayerControlled(AttackParticipant.Attacker))
            {
                Debug.LogError($"ADDED 10 TO PLAYER DAMAGE! THIS IS TEMPORARY, REMOVE IT!");
                adjustedDamage += 10;
            }

            return new(attackerHitChance, defenderDodgeChance, hitRoll, critChance, critRoll, damage, damageReduction, adjustedDamage, hit, crit);
        }

        private static double CalculateCritChance(in IAttackContext context)
        {
            double result = BASE_CRIT_CHANCE;
            result += context.GetAttributeValue(AttackParticipant.Attacker, AttributeType.Wits) * 0.01d;
            return result;
        }

        private static float GetCriticalDamageMultiplier(in IAttackContext context)
        {
            return context.GetCriticalDamageMultiplier();
        }

        private static int CalculateDefenderDamageReduction(in IAttackContext context)
        {
            return context.GetDamageReduction();
        }

        private static int CalculateAttackDamage(in IAttackContext context)
        {
            int damage = context.GetAttackDamage();
            damage = Mathf.Clamp(damage, 1, int.MaxValue);
            return damage;
        }

        private static double CalculateHitChance(in IAttackContext attackContext)
        {
            double attributeModifier = attackContext.GetAttributeValue(AttackParticipant.Attacker, AttributeType.Wits);
            double result = 0d;
            int ceiledGridDistance = attackContext.GetGridDistanceBetweenParticipants();
            if (attackContext.GetAttackRangeData().ShortRange >= ceiledGridDistance)
            {
                result -= SHORT_RANGE_HIT_PENALTY;
            }
            result += BASE_HIT_CHANCE + (attributeModifier * 0.01d);
            return result;
        }

        private static double CalculateDodgeChance(in IAttackContext attackContext)
        {
            double result = BASE_DODGE_CHANCE;
            bool hasCover = CoverDetector.FiringLineObstructedByCover(attackContext, out ICover coverSource);
            if (hasCover)
            {
                int defenderDexterity = attackContext.GetAttributeValue(AttackParticipant.Target, AttributeType.Dexterity);
                switch (coverSource.Quality)
                {
                    case CoverQuality.Half:
                        result += COVER_MODIFIER_LOW + (defenderDexterity * 0.2d);
                        break;
                    case CoverQuality.Full:
                        result += COVER_MODIFIER_HIGH + (defenderDexterity * 0.5d);
                        break;
                }
            }
            return result;
        }
    }

    [Serializable]
    public struct AttackModifier
    {
        public double HitChanceMod, DodgeChanceMod, HitRollMod;
        public double CritChanceMod, CritRollMod;
        public int DamageMod, DamageReductionMod, AdjustedDamageMod;
        public bool ForceSuccesfulHit, ForceCritical;

        public static AttackModifier operator +(AttackModifier a, AttackModifier b)
        {
            return new AttackModifier
            {
                HitChanceMod = a.HitChanceMod + b.HitChanceMod,
                DodgeChanceMod = a.DodgeChanceMod + b.DodgeChanceMod,
                HitRollMod = a.HitRollMod + b.HitRollMod,
                CritChanceMod = a.CritChanceMod + b.CritChanceMod,
                CritRollMod = a.CritRollMod + b.CritRollMod,
                DamageMod = a.DamageMod + b.DamageMod,
                DamageReductionMod = a.DamageReductionMod + b.DamageReductionMod,
                AdjustedDamageMod = a.AdjustedDamageMod + b.AdjustedDamageMod,
                ForceSuccesfulHit = a.ForceSuccesfulHit || b.ForceSuccesfulHit,
                ForceCritical = a.ForceCritical || b.ForceCritical
            };
        }
    }


    public readonly struct AttackResult
    {
        public readonly double AttackerHitChance, DefenderDodgeChance, HitRoll;
        public readonly double CritChance, CritRoll;
        public readonly int Damage, DamageReduction, AdjustedDamage;
        public readonly bool Successful, Critical;

        public AttackResult(double attackerHitChance, double defenderDodgeChance, double hitRoll, double critChance, double critRoll, int damage, int damageReduction, int adjustedDamage, bool successful, bool critical)
        {
            AttackerHitChance = attackerHitChance;
            DefenderDodgeChance = defenderDodgeChance;
            HitRoll = hitRoll;
            CritChance = critChance;
            CritRoll = critRoll;
            Damage = damage;
            DamageReduction = damageReduction;
            AdjustedDamage = adjustedDamage;
            Successful = successful;
            Critical = critical;
        }
    }
}
