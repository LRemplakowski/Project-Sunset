using Sirenix.OdinInspector;
using SunsetSystems.Entities.Characters;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UnityEngine;

namespace SunsetSystems.UMA
{
    [CreateAssetMenu(fileName = "New UMA Config", menuName = "Sunset Core/UMA Config")]
    public class ScriptableUMAConfig : SerializedScriptableObject
    {
        [field: SerializeField]
        public Dictionary<BodyType, RaceData> BodyRaceData { get; private set; } = new();
        [field: SerializeField]
        public Dictionary<RaceData, RuntimeAnimatorController> RaceAnimators { get; private set; } = new();
    }
}
