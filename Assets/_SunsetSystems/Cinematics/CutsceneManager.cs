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
        private FadeScreenManager _crossFade;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        //public void PlayCutscene(PlayableAsset asset, DirectorWrapMode wrapMode, bool doCrossFade = false)
        //{
        //    if (doCrossFade)
        //    {
        //        _crossFade.CycleFade(() => DoPlayNextCutscene(asset, wrapMode));
        //    }
        //    else
        //    {
        //        DoPlayNextCutscene(asset, wrapMode);
        //    }
        //}

        public void PlayCutscene(PlayableAsset asset, DirectorWrapMode wrapMode)
        {
            _crossFade.CycleFade(() => DoPlayNextCutscene(asset, wrapMode));
        }

        private void DoPlayNextCutscene(PlayableAsset asset, DirectorWrapMode wrapMode)
        {
            _playableDirector.Stop();
            _playableDirector.Play(asset, wrapMode);
            _playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
        }
    }
}
