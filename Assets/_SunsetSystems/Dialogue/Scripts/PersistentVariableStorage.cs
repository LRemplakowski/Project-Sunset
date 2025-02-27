using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using SunsetSystems.Data;
using SunsetSystems.Persistence;
using UnityEngine;
using Yarn.Unity;

namespace SunsetSystems.Dialogue
{
    public class PersistentVariableStorage : VariableStorageBehaviour, ISaveable, IResetable
    {
        [Title("References")]
        [SerializeField]
        private DialogueVariableConfig _variableInjectionConfig;

        [Title("Runtime")]
        [ShowInInspector]
        private Dictionary<string, float> _floats = new();
        [ShowInInspector]
        private Dictionary<string, string> _strings = new();
        [ShowInInspector]
        private Dictionary<string, bool> _bools = new();

        private readonly Dictionary<string, object> _variables = new();
       
        public string DataKey => DataKeyConstants.PERSISTANT_VARIABLE_STORAGE_DATA_KEY;

        private void Awake()
        {
          
            if (_variableInjectionConfig != null)
            {
                DialogueSaveData injectionData = _variableInjectionConfig.GetVariableInjectionData();
                SetAllVariables(injectionData._floats, injectionData._strings, injectionData._bools);
            }
            ISaveable.RegisterSaveable(this);
        }

        private void OnDestroy()
        {
            ISaveable.UnregisterSaveable(this);
        }

        public void ResetOnGameStart()
        {
            Clear();
            Awake();
        }

        public void ReinjectPreconfiguredVariables()
        {
            if (_variableInjectionConfig != null)
            {
                DialogueSaveData injectionData = _variableInjectionConfig.GetVariableInjectionData();
                SetAllVariables(injectionData._floats, injectionData._strings, injectionData._bools, false);
            }
        }

        public override void Clear()
        {
            _floats.Clear();
            _strings.Clear();
            _bools.Clear();
            _variables.Clear();
        }

        public override bool Contains(string variableName)
        {
            return _variables.ContainsKey(variableName);
        }

        public override (Dictionary<string, float>, Dictionary<string, string>, Dictionary<string, bool>) GetAllVariables()
        {
            return (_floats, _strings, _bools);
        }

        public bool InjectSaveData(object data)
        {
            if (data is not DialogueSaveData savedData)
                return false;
            SetAllVariables(savedData._floats, savedData._strings, savedData._bools);
            return true;
        }

        public object GetSaveData()
        {
            return new DialogueSaveData(_floats, _strings, _bools);
        }

        public override void SetAllVariables(Dictionary<string, float> floats, Dictionary<string, string> strings, Dictionary<string, bool> bools, bool clear = true)
        {
            if (clear)
            {
                Clear();
            }
            floats.Keys.ForEach(key => SetValue(key, floats[key]));
            strings.Keys.ForEach(key => SetValue(key, strings[key]));
            bools.Keys.ForEach(key => SetValue(key, bools[key]));
        }

        public override void SetValue(string variableName, string stringValue)
        {
            _strings[variableName] = stringValue;
            _variables[variableName] = stringValue;
        }

        public override void SetValue(string variableName, float floatValue)
        {
            _floats[variableName] = floatValue;
            _variables[variableName] = floatValue;
        }

        public override void SetValue(string variableName, bool boolValue)
        {
            _bools[variableName] = boolValue;
            _variables[variableName] = boolValue;
        }

        public override bool TryGetValue<T>(string variableName, out T result)
        {
            result = default;
            if (_variables.TryGetValue(variableName, out object value))
            {
                result = (T)value;
                return true;
            }
            return false;
        }
    }

    [Serializable]
    public class DialogueSaveData
    {
        [OdinSerialize]
        public Dictionary<string, float> _floats = new();
        [OdinSerialize]
        public Dictionary<string, string> _strings = new();
        [OdinSerialize]
        public Dictionary<string, bool> _bools = new();

        public DialogueSaveData(Dictionary<string, float> _floats, Dictionary<string, string> _strings, Dictionary<string, bool> _bools)
        {
            this._floats = new(_floats);
            this._strings = new(_strings);
            this._bools = new(_bools);
        }

        public DialogueSaveData()
        {

        }
    }
}
