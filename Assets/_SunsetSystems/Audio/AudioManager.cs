using System;
using Sirenix.OdinInspector;
using SunsetSystems.Core;
using SunsetSystems.Core.SceneLoading;
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

            GameManager.OnGameStateChanged += OnGameStateChanged;
            LevelLoader.OnLevelLoadEnd += OnLevelLoadEnd;
        }

        private void Start()
        {
            SetMusicVolume(GetSavedMusicVolume());
            SetSFXVolume(GetSavedSFXVolume());
            OnGameStateChanged(GameManager.Instance.CachedGameState);
        }

        private void OnDestroy()
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
            LevelLoader.OnLevelLoadEnd -= OnLevelLoadEnd;
        }

        private void OnGameStateChanged(GameState newGameState)
        {
            _soundtrackController.PlayStatePlaylist(newGameState);
            SetMusicVolume(GetSavedMusicVolume());
            SetSFXVolume(GetSavedSFXVolume());
        }

        private void OnLevelLoadEnd()
        {
            SetMusicVolume(GetSavedMusicVolume());
            SetSFXVolume(GetSavedSFXVolume());
        }

        private float GetSavedMusicVolume()
        {
            if (PlayerPrefs.HasKey(SettingsConstants.MUSIC_VOLUME_KEY))
            {
                return PlayerPrefs.GetFloat(SettingsConstants.MUSIC_VOLUME_KEY);
            }
            else
            {
                return MusicDefaultValue;
            }
        }

        private float GetSavedSFXVolume()
        {
            if (PlayerPrefs.HasKey(SettingsConstants.SFX_VOLUME_KEY))
            {
                return PlayerPrefs.GetFloat(SettingsConstants.SFX_VOLUME_KEY);
            }
            else
            {
                return SFXDefaultValue;
            }
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
            return _soundtrackController.GetCurrentOverridesAsPlaylistData();
        }

        public void SetPlaylistOverride(GameState state, IPlaylist playlist) => _soundtrackController.SetStatePlaylistOverride(state, playlist);

        public void ClearPlaylistOverride(GameState state) => _soundtrackController.ClearStatePlaylistOverride(state);
    }
}
