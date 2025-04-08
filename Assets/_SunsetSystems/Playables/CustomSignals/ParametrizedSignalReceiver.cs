using System.Collections.Generic;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SunsetSystems.Playables
{
    public abstract class ParametrizedSignalReceiver<T> : SerializedMonoBehaviour, INotificationReceiver
    {
        [SerializeField, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout, KeyLabel = "Signal", ValueLabel = "Response")]
        private Dictionary<SignalAsset, UltEvent<T>> _signals = new();

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is ParametrizedSignalEmitter<T> parametrizedSignal)
            {
                if (_signals.TryGetValue(parametrizedSignal.asset, out var signalEvent))
                {
                    signalEvent?.InvokeSafe(parametrizedSignal.Parameter);
                }
            }
        }
    }
}
