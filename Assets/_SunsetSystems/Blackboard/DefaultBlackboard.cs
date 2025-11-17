using System;
using System.Collections.Generic;

namespace SunsetSystems.Blackboard
{
    public class DefaultBlackboard : IBlackboard
    {
        protected readonly Dictionary<string, object> _data;
        public event Action<string> OnDataUpdated;

        public DefaultBlackboard() 
        { 
            _data = new Dictionary<string, object>();
        }

        public DefaultBlackboard(Dictionary<string, object> initialData)
        {
            _data = new Dictionary<string, object>(initialData);
        }

        public void SetValue<T>(string key, T value)
        {
            _data[key] = value;
            OnDataUpdated?.Invoke(key);
        }

        public T ReadValue<T>(string key)
        {
            if (_data.TryGetValue(key, out object data) && data is T typedValue)
            {
                return typedValue;
            }
            return default;
        }

        public bool TryReadValue<T>(string key, out T value)
        {
            value = default;
            if (_data.TryGetValue(key, out object data) && data is T typedValue)
            {
                value = typedValue;
                return true;
            }
            return false;
        }

        public bool ContainsKey(string key)
        {
            return _data.ContainsKey(key);
        }
    }
}
