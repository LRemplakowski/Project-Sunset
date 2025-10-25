using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using SunsetSystems.Entities.Characters;
using UnityEngine;

namespace SunsetSystems.Abilities
{
    public class SpellbookManager : SerializedMonoBehaviour, ISpellbookManager, IAbilitySource
    {
        [OdinSerialize]
        private Dictionary<string, DisciplineData> _knownPowers = new();

        public IReadOnlyCollection<string> KnownPowerIDs => _knownPowers.Values.SelectMany(data => data.KnownPowers).Select(power => power.ID).ToList();
        public IReadOnlyCollection<IDisciplinePower> KnownPowers => _knownPowers.Values.SelectMany(data => data.KnownPowers).ToList();
        public IReadOnlyCollection<IDisciplineInfo> KnownDisciplines => _knownPowers.Values;

        public bool IsPowerKnown(IDisciplinePower power)
        {
            return _knownPowers.TryGetValue(power.Discipline.ID, out var disciplineData) && disciplineData.KnownPowers.Contains(power);
        }

        public bool TryLearnPower(string powerID)
        {
            var abDatabase = AbilityDatabase.Instance;
            if (abDatabase && abDatabase.TryGetEntryByReadableID(powerID, out var powerConfig) && powerConfig is IDisciplinePower disciplinePpwer)
            {
                return TryLearnPower(disciplinePpwer);
            }
            return false;
        }

        [Button]
        public bool TryLearnPower(IDisciplinePower power)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
            if (_knownPowers.TryGetValue(power.Discipline.ID, out var disciplineData) && disciplineData != null)
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

        public IEnumerable<IAbilityConfig> GetAbilities()
        {
            return KnownPowers.OfType<IAbilityConfig>().ToList();
        }

        public void CopyFromTemplate(ICreatureTemplate template)
        {
            if (template.KnownPowers == null) return;

            template.KnownPowers.ForEach(power => TryLearnPower(power));
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
    }
}
