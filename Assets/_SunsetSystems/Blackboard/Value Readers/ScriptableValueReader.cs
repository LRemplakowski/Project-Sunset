using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace SunsetSystems.Blackboard
{
    public abstract class ScriptableValueReader<T> : SerializedMonoBehaviour
    {
        [SerializeField]
        protected IScriptableVariable<T> _variable;

        [SerializeField]
        protected UltEvent<T> OnValueUpdated;

        private void Awake()
        {
            if (_variable != null)
            {
                _variable.OnValueChanged += ValueUpdated;
            }
        }

        private void Start()
        {
            if (_variable != null)
            {
                ValueUpdated(_variable.Value);
            }
        }

        private void OnDestroy()
        {
            if (_variable != null)
            {
                _variable.OnValueChanged -= ValueUpdated;
            }
        }

        protected virtual void ValueUpdated(T value)
        {
            OnValueUpdated?.Invoke(value);
        }
    }
}
