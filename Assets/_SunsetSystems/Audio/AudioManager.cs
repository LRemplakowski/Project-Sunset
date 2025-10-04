using Sirenix.OdinInspector;
using SunsetSystems.Core;
using SunsetSystems.Game;
using UnityEngine;
using UnityEngine.Audio;

namespace SunsetSystems.Audio
{
    public class AudioManager : SerializedMonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField]
        private AudioMixer _audioMixer;
        [SerializeField]
        private SFXController _sfxController;
        [SerializeField]
        private SoundtrackController _soundtrackController;

        [field: SerializeField]
        public float MusicDefaultValue { get; private set; } = .5f;
        [field: SerializeField]
        public float SFXDefaultValue { get; private set; } = .5f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (PlayerPrefs.HasKey(SettingsConstants.MUSIC_VOLUME_KEY))
                SetMusicVolume(PlayerPrefs.GetFloat(SettingsConstants.MUSIC_VOLUME_KEY));
            if (PlayerPrefs.HasKey(SettingsConstants.SFX_VOLUME_KEY))
                SetSFXVolume(PlayerPrefs.GetFloat(SettingsConstants.SFX_VOLUME_KEY));
            OnGameStateChanged(GameManager.Instance.CachedGameState);
            GameManager.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState newGameState)
        {
            _soundtrackController.PlayStatePlaylist(newGameState);
        }

        public void PlaySFXOneShot(string sfxName)
        {
            _sfxController.PlayOneShot(sfxName);
        }

        public void PlaySFXOneShot(AudioClip clip)
        {
            _sfxController.PlayOneShot(clip);
        }

        public void PlayTyperwriterLoop()
        {
            _sfxController.PlayTyperwriterLoop();
        }

        public void PlayTypewriterEnd()
        {
            _sfxController.StopTyperwriterLoop();
        }

        public void StopSFXPlayback()
        {
            _sfxController.StopSFX();
        }

        public void SetMusicVolume(float volume)
        {
            PlayerPrefs.SetFloat(SettingsConstants.MUSIC_VOLUME_KEY, volume);
            PlayerPrefs.Save();
            _soundtrackController.Volume = volume;
        }

        public void SetSFXVolume(float volume)
        {
            PlayerPrefs.SetFloat(SettingsConstants.SFX_VOLUME_KEY, volume);
            PlayerPrefs.Save();
            _sfxController.Volume = volume;
        }

        public void InjectPlaylistData(ScenePlaylistData playlistData)
        {
            _soundtrackController.InjectPlaylistData(playlistData);
        }

        public void InjectPlaylistDataAsOverrides(ScenePlaylistData playlistData)
        {
            _soundtrackController.InjectPlaylistDataAsOverrides(playlistData);
        }

        public ScenePlaylistData GetCurrentPlaylistOverrides()
        {
            //return _soundtrackController.GetCurrentOverridesAsPlaylistData();
            return new();
        }

        public void SetPlaylistOverride(GameState state, IPlaylist playlist) => _soundtrackController.SetStatePlaylistOverride(state, playlist);

        public void ClearPlaylistOverride(GameState state) => _soundtrackController.ClearStatePlaylistOverride(state);
    }
}
