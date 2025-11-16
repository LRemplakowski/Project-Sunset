using System;
using UnityEngine;

namespace SunsetSystems.Blackboard
{
    public abstract class ScriptableVariable<T> : ScriptableVariableBase, IScriptableVariable<T>
    {
        public event Action<T> OnValueChanged;

        [SerializeField]
        private Guid _key = Guid.NewGuid();
        [SerializeField]
        private T _defaultValue;
        private string Key => _key.ToString();

        public T Value
        {
            get
            {
                if (VariableBlackboard.ContainsKey(Key) == false)
                {
                    VariableBlackboard.SetValue(Key, _defaultValue);
                    return _defaultValue;
                }
                return VariableBlackboard.ReadValue<T>(Key);
            }

            set => VariableBlackboard.SetValue(Key, value);
        }

        private void Awake()
        {
            VariableBlackboard.OnDataUpdated += OnBlackboardUpdated;
        }

        private void OnDestroy()
        {
            VariableBlackboard.OnDataUpdated -= OnBlackboardUpdated;
        }

        private void OnBlackboardUpdated(string key)
        {
            if (key == Key)
            {
                OnValueChanged?.Invoke(Value);
            }
        }
    }
}
