using Sirenix.OdinInspector;
using UnityEngine;

namespace SunsetSystems.Abilities
{
    [CreateAssetMenu(fileName = "New Discipline", menuName = "Sunset Core/Discipline")]
    public class DisciplineAsset : SerializedScriptableObject, IDiscipline
    {
        [SerializeField]
        private string _scriptID = "DISCIPLINE_ID";
        [SerializeField]
        private string _fallbackName = "New Discipline";
        [SerializeField]
        private string _fallbackDescription = "Discipline Description";

        public string ID => _scriptID;
        public string Name => _fallbackName;
        public string Description => _fallbackDescription;

        public int CompareTo(IDiscipline other)
        {
            return Name.CompareTo(other.Name);
        }
    }
}
