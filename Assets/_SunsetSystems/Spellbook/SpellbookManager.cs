using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace SunsetSystems.Abilities
{
    public class SpellbookManager : SerializedMonoBehaviour, ISpellbookManager, IAbilitySource
    {
        [OdinSerialize]
        private Dictionary<string, DisciplineData> _knownPowers = new();

        public IReadOnlyCollection<IDisciplinePower> KnownPowers => _knownPowers.Values.SelectMany(data => data.KnownPowers).ToList();
        public IReadOnlyCollection<DisciplineData> KnownDisciplines => throw new NotImplementedException();

        public bool IsPowerKnown(IDisciplinePower power)
        {
            return IsPowerKnown(power.ID);
        }

        public bool IsPowerKnown(string powerID)
        {
            return _knownPowers.ContainsKey(powerID);
        }

        public bool TryLearnPower(IDisciplinePower power)
        {
            if (_knownPowers.TryGetValue(power.Discipline.ID, out var disciplineData))
            {
                if (power.Level > disciplineData.CurrentLevel)
                    disciplineData.SetCurrentLevel(power.Level);
                return disciplineData.TryAddPower(power);
            }
            else
            {
                disciplineData = new DisciplineData();
                disciplineData.SetCurrentLevel(power.Level);
                disciplineData.SetDisciplineAsset(power.Discipline);
                bool result = disciplineData.TryAddPower(power);
                _knownPowers[power.Discipline.ID] = disciplineData;
                return result;
            }
        }

        private List<DisciplineData> GetDisciplineDataFromPowers()
        {
            List<DisciplineData> result = new();
            
            return result;
        }

        public IEnumerable<IAbilityConfig> GetAbilities()
        {
            return _knownPowers.OfType<IAbilityConfig>().ToList();
        }
    }
}
