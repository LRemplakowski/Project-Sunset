using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SunsetSystems.Abilities
{
    [System.Serializable]
    public class DisciplineOLD : BaseStat
    {
        [field: SerializeField]
        public Sprite Icon { get; private set; }

        [SerializeField, Range(0, 5)]
        private int baseValue = 0;

        [SerializeField, ReadOnly]
        private DisciplineType disciplineType;

        public override string Name => disciplineType.ToString();

        [SerializeField]
        private List<DisciplinePower> knownPowers = new();

        public DisciplineOLD(DisciplineOLD existing) : base(existing)
        {
            baseValue = existing.baseValue;
            knownPowers = new(existing.knownPowers);
            disciplineType = existing.disciplineType;
        }

        public DisciplinePower GetPower(int index)
        {
            try
            {
                return knownPowers[index];
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogException(e);
                return ScriptableObject.CreateInstance<DisciplinePower>();
            }
        }

        public List<DisciplinePower> GetKnownPowers() => new(knownPowers);

        public DisciplineOLD(DisciplineType disciplineType)
        {
            this.disciplineType = disciplineType;
        }

        public override int GetValue(ModifierType modifierTypesFlag)
        {
            int finalValue = baseValue;
            if (Modifiers != null)
                Modifiers.ForEach(m => finalValue += (modifierTypesFlag & m.Type) > 0 ? m.Value : 0);
            return finalValue;
        }

        protected override void SetValueImpl(int value)
        {
            this.baseValue = value;
        }

        public DisciplineType GetDisciplineType()
        {
            return disciplineType;
        }
    }
}