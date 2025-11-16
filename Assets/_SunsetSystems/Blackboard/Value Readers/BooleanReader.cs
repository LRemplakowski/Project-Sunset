using UltEvents;
using UnityEngine;

namespace SunsetSystems.Blackboard
{
    public class BooleanReader : ScriptableValueReader<bool> 
    {
        [SerializeField]
        private UltEvent OnTrue;
        [SerializeField]
        private UltEvent OnFalse;

        protected override void ValueUpdated(bool value)
        {
            base.ValueUpdated(value);
            if (value)
            {
                OnTrue?.Invoke();
            }
            else
            {
                OnFalse?.Invoke();
            }
        }
    }
}
