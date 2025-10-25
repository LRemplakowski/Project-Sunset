using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SunsetSystems.Abilities
{
    [CreateAssetMenu(fileName = "New Fake Spell", menuName = "Sunset Abilities/Fake Spell")]
    public class FakeSpell : SerializedScriptableObject, IDisciplinePower
    {
        [SerializeField]
        private Guid _id = Guid.NewGuid();
        [SerializeField]
        private string _uniqueScriptName = string.Empty;
        [SerializeField]
        private string _name = "New Fake Power";
        [SerializeField, MultiLineProperty]
        private string _description = string.Empty;
        [SerializeField]
        private IDiscipline _discipline;
        [SerializeField, PropertyRange(1, 5)]
        private int _level = 1;

        public string ID => _id.ToString();
        public string ScriptName => _uniqueScriptName;
        public string Name => _name;
        public string Description => _description;
        public IDiscipline Discipline => _discipline;
        public int Level => _level;
    }
}