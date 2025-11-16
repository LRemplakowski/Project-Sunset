using System;

namespace SunsetSystems.Blackboard
{
    public interface IBlackboard
    {
        event Action<string> OnDataUpdated;

        void SetValue<T>(string key, T value);
        T ReadValue<T>(string key);
        bool TryReadValue<T>(string key, out T value);
        bool ContainsKey(string key);
    }
}
