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
        private DirectorUtility _directorUtility;
        [SerializeField, Required]
        private FadeScreenManager _crossFade;

        private void OnValidate()
        {
            EnsureReferences();
        }

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

        private void Start()
        {
            EnsureReferences();
            SubscribeDirectorEvents();
        }

        private void EnsureReferences()
        {
            if (_playableDirector == null)
            {
                _playableDirector = FindAnyObjectByType<PlayableDirector>(FindObjectsInactive.Include);
            }
            if (_directorUtility == null)
            {
                _directorUtility = FindAnyObjectByType<DirectorUtility>(FindObjectsInactive.Include);
            }
            if (_crossFade == null)
            {
                _crossFade = FindAnyObjectByType<FadeScreenManager>(FindObjectsInactive.Include);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeDirectorEvents();
            if (SunsetInputHandler.Instance != null)
                SunsetInputHandler.Instance.ClearInputOverride(this);
        }

        private void SubscribeDirectorEvents()
        {
            if (_playableDirector == null) return;

            _playableDirector.played += OnCutscenePlay;
            _playableDirector.stopped += OnCutsceneStop;
        }

        private void UnsubscribeDirectorEvents()
        {
            if (_playableDirector == null) return;

            _playableDirector.played -= OnCutscenePlay;
            _playableDirector.stopped -= OnCutsceneStop;
        }

        [Button]
        public void PlayCutscene(PlayableAsset asset, DirectorWrapMode wrapMode, bool doCrossFade = false)
        {
            if (doCrossFade)
            {
                _crossFade.CycleFade(() => DoPlayNextCutscene(asset, wrapMode));
            }
            else
            {
                DoPlayNextCutscene(asset, wrapMode);
            }
        }

        public void SetDirector(PlayableDirector director)
        {
            StopCutscene();
            UnsubscribeDirectorEvents();
            _playableDirector = director;
            SubscribeDirectorEvents();
        }

        public void PlayCutscene(PlayableAsset asset, DirectorWrapMode wrapMode)
        {
            _crossFade.CycleFade(() => DoPlayNextCutscene(asset, wrapMode));
        }

        [Button]
        public void StopCutscene(bool doCrossFade = false)
        {
            if (doCrossFade)
            {
                _crossFade.CycleFade(() => _playableDirector.Stop());
            }
            else
            {
                _playableDirector.Stop();
            }
        }

        private void DoPlayNextCutscene(PlayableAsset asset, DirectorWrapMode wrapMode)
        {
            _playableDirector.Stop();
            _playableDirector.Play(asset, wrapMode);
            _directorUtility.ClearPlaybackControl();
            _playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
        }

        private void OnCutsceneStop(PlayableDirector director)
        {
            SunsetInputHandler.Instance.ClearInputOverride(this);
        }

        private void OnCutscenePlay(PlayableDirector director)
        {
            SunsetInputHandler.Instance.OverrideInput(this, SunsetInputHandler.UI_MAP);
        }
    }
}
