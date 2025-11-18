using System;
using UnityEngine;

namespace SunsetSystems.ActionSystem
{
    [Serializable]
    public class Delay : Condition
    {
        [SerializeReference]
        private Condition _after;
        [SerializeField]
        private float _delayTime;
        private float _startTime;
        private bool _isStarted;

        public Delay(float delay, Condition after)
        {
            this._delayTime = delay;
            this._after = after;
            _isStarted = false;
        }

        public override bool IsMet()
        {
            if (_isStarted)
            {
                return Time.time - _startTime >= _delayTime;
            }
            else
            {
                bool shouldStart = _after.IsMet();
                if (shouldStart) 
                    _startTime = Time.time;
                _isStarted = shouldStart;
                return false;
            }
        }
    }
}
