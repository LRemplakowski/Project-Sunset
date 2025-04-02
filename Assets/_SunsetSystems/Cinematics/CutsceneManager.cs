using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace SunsetSystems.Cinematics
{
    public class CutsceneManager : SerializedMonoBehaviour
    {
        public static CutsceneManager Instance { get; private set; }

        [SerializeField, Required]
        private PlayableDirector _playableDirector;
        [SerializeField, Required]
        private CanvasGroup _crossFadeCanvasGroup;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void PlayCutscene(PlayableAsset asset, DirectorWrapMode wrapMode)
        {
            _playableDirector.Stop();
            _playableDirector.Play(asset, wrapMode);
            _playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
        }
    }
}
