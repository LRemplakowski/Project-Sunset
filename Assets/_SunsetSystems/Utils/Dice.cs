using System;
using System.Collections.Generic;
using UnityEngine;
using SunsetSystems.Abilities;

namespace SunsetSystems.Dice
{
    public interface IIntValue
    {
        int GetValue();
    }

    public abstract class DicePool<T, U> where T : IIntValue where U : IIntValue
    {
        public T First { get; protected set; }
        public U Second { get; protected set; }

        public int GetPoolSize()
        {
            return First.GetValue() + Second.GetValue();
        }

        //public int GetPoolSize(bool includeModifiers)
        //{
        //    return First.GetValue(includeModifiers) + Second.GetValue(includeModifiers);
        //}

        //public int GetPoolSize(ModifierType modifierTypesFlag)
        //{
        //    return First.GetValue(modifierTypesFlag) + Second.GetValue(modifierTypesFlag);
        //}
    }

    public sealed class TwoAttributePool : DicePool<CreatureAttribute, CreatureAttribute>
    {
        public TwoAttributePool(CreatureAttribute first, CreatureAttribute second)
        {
            First = first;
            Second = second;
        }
    }

    public sealed class AttributeSkillPool : DicePool<CreatureAttribute, Skill>
    {
        public AttributeSkillPool(CreatureAttribute attribute, Skill skill)
        {
            First = attribute;
            Second = skill;
        }
    }

    public sealed class TwoSkillPool : DicePool<Skill, Skill>
    {
        public TwoSkillPool(Skill first, Skill second)
        {
            First = first;
            Second = second;
        }
    }

    public sealed class DisciplineAttributePool : DicePool<DisciplineData, CreatureAttribute>
    {
        public DisciplineAttributePool(DisciplineData discipline, CreatureAttribute attribute)
        {
            First = discipline;
            Second = attribute;
        }
    }

    public sealed class DisciplineSkillPool : DicePool<DisciplineData, Skill>
    {
        public DisciplineSkillPool(DisciplineData discipline, Skill skill)
        {
            First = discipline;
            Second = skill;
        }
    }

    public class Outcome
    {
        public readonly int successes;
        public readonly bool isCritical, isMessy, isBestialFailure;
        public readonly List<int> normalDice = new(), hungerDice = new();

        public Outcome(int successes, bool isCritical, bool isMessy, bool isBestialFailure, List<int> normalDice, List<int> hungerDice)
        {
            this.successes = successes;
            this.isCritical = isCritical;
            this.isMessy = isMessy;
            this.isBestialFailure = isBestialFailure;
            this.normalDice = normalDice;
            this.hungerDice = hungerDice;
        }

        public Outcome(int successes, bool isCritical, List<int> normalDice)
        {
            this.successes = successes;
            this.isCritical = isCritical;
            isMessy = false;
            isBestialFailure = false;
            this.normalDice = normalDice;
        }
    }

    public static class Roll
    {
        private static readonly System.Random r = new();
#pragma warning disable IDE1006 // Style nazewnictwa
        public static Outcome d10(int normalDice, int hungerDice)
#pragma warning restore IDE1006 // Style nazewnictwa
        {
            int[] normals = new int[normalDice];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = r.Next(1, 11);
            }
            int[] hunger = new int[hungerDice];
            for (int i = 0; i < hunger.Length; i++)
            {
                hunger[i] = r.Next(1, 11);
            }

            int successes = Array.FindAll(normals, n => n >= 6).Length + Array.FindAll(hunger, h => h >= 6).Length;
            int tens = Array.FindAll(normals, n => n == 10).Length;
            int hungerTens = Array.FindAll(hunger, h => h == 10).Length;
            bool isCritical = false;
            if (tens + hungerTens >= 2)
            {
                successes += tens + hungerTens - ((tens + hungerTens) % 2);
                isCritical = true;
            }
            bool isMessy = isCritical && hungerTens > 0;
            Debug.Log("d10:"
                + "\nnormal dice: " + DiceRollToString(normals)
                + "\nhunger dice: " + DiceRollToString(hunger)
                + "\nsuccesses: " + successes
                + "\ntens: " + tens
                + "\nhungerTens: " + hungerTens
                + "\nisCritical? " + isCritical
                + "\nisMessy?" + isMessy);
            return new Outcome(successes, isCritical, isMessy, false, new List<int>(normals), new List<int>(hunger));
        }

#pragma warning disable IDE1006 // Style nazewnictwa
        public static Outcome d10(int normalDice, int hungerDice, int dc)
#pragma warning restore IDE1006 // Style nazewnictwa
        {
            int[] normals = new int[normalDice];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = r.Next(1, 11);
            }
            int[] hunger = new int[hungerDice];
            for (int i = 0; i < hunger.Length; i++)
            {
                hunger[i] = r.Next(1, 11);
            }

            int successes = Array.FindAll(normals, n => n >= 6).Length + Array.FindAll(hunger, h => h >= 6).Length;
            int tens = Array.FindAll(normals, n => n == 10).Length;
            int hungerOnes = Array.FindAll(hunger, h => h == 1).Length;
            int hungerTens = Array.FindAll(hunger, h => h == 10).Length;
            bool isCritical = false;
            if (tens + hungerTens >= 2)
            {
                successes += tens + hungerTens - ((tens + hungerTens) % 2);
                isCritical = true;
            }
            bool isMessy = isCritical && hungerTens > 0;
            bool isBestial = hungerOnes > 0 && successes < dc;
            Debug.Log("d10:"
                + "\nnormal dice: " + DiceRollToString(normals)
                + "\nhunger dice: " + DiceRollToString(hunger)
                + "\nsuccesses: " + successes
                + "\nDC: " + dc
                + "\ntens: " + tens
                + "\nhungerTens: " + hungerTens
                + "\nisCritical? " + isCritical
                + "\nisMessy?" + isMessy
                + "\nisBestial? " + isBestial);
            return new Outcome(successes, isCritical, isMessy, isBestial, new List<int>(normals), new List<int>(hunger));
        }

#pragma warning disable IDE1006 // Style nazewnictwa
        public static Outcome d10(int dice)
#pragma warning restore IDE1006 // Style nazewnictwa
        {
            int[] roll = new int[dice];
            for (int i = 0; i < roll.Length; i++)
            {
                roll[i] = r.Next(1, 11);
            }

            int successes = Array.FindAll(roll, n => n >= 6).Length;
            int tens = Array.FindAll(roll, n => n == 10).Length;
            bool isCritical = false;
            if (tens >= 2)
            {
                successes += tens % 2 == 0 ? tens : tens - 1;
                isCritical = true;
            }
            Debug.Log("d10:"
                + "\ndice: " + DiceRollToString(roll)
                + "\nsuccesses: " + successes
                + "\ntens: " + tens
                + "\nisCritical? " + isCritical);
            return new Outcome(successes, isCritical, new List<int>(roll));
        }

        private static string DiceRollToString(int[] roll)
        {
            string result = "";
            foreach (int i in roll)
            {
                result = result + i + ", ";
            }
            return result;
        }
    }
}