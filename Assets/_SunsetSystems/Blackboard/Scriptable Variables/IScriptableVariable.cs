using System;

namespace SunsetSystems.Blackboard
{
    public interface IScriptableVariable<T>
    {
        event Action<T> OnValueChanged;
        T Value { get; set; }
    }
}
