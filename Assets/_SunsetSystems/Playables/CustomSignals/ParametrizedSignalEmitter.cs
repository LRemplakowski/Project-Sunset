using UnityEngine;
using UnityEngine.Timeline;

namespace SunsetSystems.Playables
{
    public abstract class ParametrizedSignalEmitter<T> : SignalEmitter
    {
        [field: SerializeField]
        public T Parameter { get; private set; }
    }
}
