using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SunsetSystems.Blackboard
{
    public abstract class ScriptableVariableBase : SerializedScriptableObject
    {
        private static SaveableBlackboard _saveableBlackboard = new();
        protected static IBlackboard VariableBlackboard => _saveableBlackboard;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitialize()
        {
            _saveableBlackboard = new SaveableBlackboard();
        }

        public static BlackboardSaveData GetSaveData()
        {
            return new BlackboardSaveData
            {
                Data = new Dictionary<string, object>(_saveableBlackboard.GetData())
            };
        }

        public static void InjectSaveData(BlackboardSaveData saveData)
        {
            _saveableBlackboard = new SaveableBlackboard(saveData.Data);
        }

        private class SaveableBlackboard : DefaultBlackboard
        {
            public SaveableBlackboard() : base() { }
            public SaveableBlackboard(Dictionary<string, object> initialData) : base(initialData) { }
            public Dictionary<string, object> GetData()
            {
                return _data;
            }
            public void ClearData() => _data.Clear();
        }
    }
}
