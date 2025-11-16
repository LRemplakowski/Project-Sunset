using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using SunsetSystems.Dice;
using SunsetSystems.UI.Utils;
using UnityEngine;

namespace SunsetSystems.Abilities
{
    [Serializable]
    public class DisciplineData : IDisciplineInfo, IUserInfertaceDataProvider<IDisciplineInfo>, IIntValue
    {
        [OdinSerialize]
        private IDiscipline _discipline;
        [SerializeField]
        private int _currentLevel;
        [OdinSerialize]
        private Dictionary<string, IDisciplinePower> _knownPowers = new();

        public IDisciplineInfo UIData => this;
        public IDiscipline Discipline => _discipline;
        public int CurrentLevel => _currentLevel;
        public IReadOnlyCollection<IDisciplinePower> KnownPowers => _knownPowers.Values;

        public int GetValue() => _currentLevel;

        public void SetDisciplineAsset(IDiscipline discipline) => _discipline = discipline;
        public void SetCurrentLevel(int level) => _currentLevel = level;
        public bool TryAddPower(IDisciplinePower power) => _knownPowers.TryAdd(power.ID, power);
    }
}
