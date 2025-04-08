using System;
using UnityEngine;
using UnityEngine.Playables;

namespace SunsetSystems.Playables
{
    [Serializable]
    public struct PlayCutsceneData
    {
        [SerializeField]
        private PlayableAsset _playableAsset;
        [SerializeField]
        private WrapMode _wrapMode;

        public readonly PlayableAsset GetPlayableAsset() => _playableAsset;
        public readonly WrapMode GetWrapMode() => _wrapMode;
    }
}
