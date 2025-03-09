using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Redcode.Awaiting;
using Sirenix.OdinInspector;
using SunsetSystems.Core.SceneLoading;
using SunsetSystems.Game;
using UnityEngine;

namespace SunsetSystems.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundtrackController : SerializedMonoBehaviour
    {
        [SerializeField]
        private AudioSource _soundtrackSource;
        [SerializeField, Range(0.01f, 10f)]
        private float _trackTransitionTime = 1f;
        [SerializeField, DictionaryDrawerSettings(IsReadOnly = true)]
        private Dictionary<GameState, IPlaylist> _statePlaylistPairs = new();
        [ShowInInspector, ReadOnly]
        private Dictionary<GameState, IPlaylist> _playlistOverrides = new();
        [SerializeField]
        private float _defaultVolume = .5f;
        private float _cachedVolume;
        private float _currentVolume;
        public float Volume
        {
            get
            {
                return _currentVolume;
            }
            set
            {
                _currentVolume = value;
                _soundtrackSource.volume = value;
            }
        }

        private Task _cachedPlaylistTask;
        private IPlaylist _lastPlaylist;
        private bool _playSoundtrack;
        private ScenePlaylistData _lastPlaylistData = new();

        private void Awake()
        {
            _soundtrackSource ??= GetComponent<AudioSource>();
            Volume = _defaultVolume;
            _cachedVolume = Volume;
        }

        private void OnValidate()
        {
            _statePlaylistPairs ??= new();
            foreach (GameState state in Enum.GetValues(typeof(GameState)))
            {
                if (_statePlaylistPairs.ContainsKey(state) is false)
                    _statePlaylistPairs[state] = null;
            }
        }

        public void PlayStatePlaylist(GameState gameState)
        {
            _playSoundtrack = true;
            if (GetStatePlaylist(gameState, out var statePlaylist) && statePlaylist != _lastPlaylist)
            {
                _lastPlaylist = statePlaylist;
                _cachedPlaylistTask = ExecutePlaylist(statePlaylist);
            }
        }

        private bool GetStatePlaylist(GameState state, out IPlaylist playlist)
        {
            playlist = default;
            if (_playlistOverrides.TryGetValue(state, out playlist))
                return true;
            else if (_statePlaylistPairs.TryGetValue(state, out playlist))
                return true;
            else
                return false;
        }

        public void StopSoundtrackImmediate()
        {
            _playSoundtrack = false;
            _soundtrackSource.Stop();
            _cachedPlaylistTask = null;
        }

        private async Task ExecutePlaylist(IPlaylist config)
        {
            Task myTask = _cachedPlaylistTask;
            while (_playSoundtrack)
            {
                await FadeOutSource();
                _soundtrackSource.Stop();
                AudioClip newTrack = config.NextTrack();
                _soundtrackSource.clip = newTrack;
                _soundtrackSource.Play();
                await FadeInSource();
                await new WaitForSecondsRealtime(newTrack.length);
                if (_soundtrackSource.clip != null && myTask != _cachedPlaylistTask)
                    break;
            }
        }

        private async Task FadeOutSource()
        {
            float fadeTime = _trackTransitionTime / 2;
            float timeElapsed = 0f;
            _cachedVolume = Volume;
            while (timeElapsed / fadeTime <= 1)
            {
                Volume = Mathf.Clamp01(_cachedVolume - Mathf.Lerp(0, _cachedVolume, timeElapsed / fadeTime));
                timeElapsed += Time.deltaTime;
                await new WaitForUpdate();
            }
        }

        private async Task FadeInSource()
        {
            float fadeTime = _trackTransitionTime / 2;
            float timeElapsed = 0f;
            while (timeElapsed / fadeTime <= 1)
            {
                Volume = Mathf.Clamp01(Mathf.Lerp(0, _cachedVolume, timeElapsed / fadeTime));
                timeElapsed += Time.deltaTime;
                await new WaitForUpdate();
            }
        }

        public void InjectPlaylistData(ScenePlaylistData playlistData)
        {
            //ReleasePreviousDataIfExists();
            //_lastPlaylistData = playlistData;
            //Dictionary<GameState, Task<IPlaylist>> operations = new();
            if (playlistData.Exploration != null)
            {
                //var op = playlistData.Exploration.LoadAssetAsync<IPlaylist>();
                //operations[GameState.Exploration] = op.Task;
                _statePlaylistPairs[GameState.Exploration] = playlistData.Exploration;
            }
            if (playlistData.Combat != null)
            {
                //var op = playlistData.Combat.LoadAssetAsync<IPlaylist>();
                //operations[GameState.Combat] = op.Task;
                _statePlaylistPairs[GameState.Combat] = playlistData.Combat;
            }
            if (playlistData.Dialogue != null)
            {
                //var op = playlistData.Dialogue.LoadAssetAsync<IPlaylist>();
                //operations[GameState.Dialogue] = op.Task;
                _statePlaylistPairs[GameState.Dialogue] = playlistData.Dialogue;
            }
            //await Task.WhenAll(operations.Values);
            //foreach (var statePlaylist in operations)
            //{
            //    _statePlaylistPairs[statePlaylist.Key] = statePlaylist.Value.Result;
            //}
            PlayStatePlaylist(GameState.Exploration);
        }

        public void InjectPlaylistDataAsOverrides(ScenePlaylistData playlistData)
        {
            if (playlistData.Exploration != null)
            {
                _playlistOverrides[GameState.Exploration] = playlistData.Exploration;
            }
            if (playlistData.Combat != null)
            {
                _playlistOverrides[GameState.Combat] = playlistData.Combat;
            }
            if (playlistData.Dialogue != null)
            {
                _playlistOverrides[GameState.Dialogue] = playlistData.Dialogue;
            }
            PlayStatePlaylist(GameState.Exploration);
        }

        public ScenePlaylistData GetCurrentOverridesAsPlaylistData()
        {
            ScenePlaylistData playlistData = new();
            _playlistOverrides.TryGetValue(GameState.Exploration, out playlistData.Exploration);
            _playlistOverrides.TryGetValue(GameState.Combat, out playlistData.Combat);
            _playlistOverrides.TryGetValue(GameState.Dialogue, out playlistData.Dialogue);
            return playlistData;
        }

        //private void ReleasePreviousDataIfExists()
        //{
        //    _lastPlaylistData.Dialogue?.ReleaseAsset();
        //    _lastPlaylistData.Combat?.ReleaseAsset();
        //    _lastPlaylistData.Dialogue?.ReleaseAsset();
        //}

        public void SetStatePlaylistOverride(GameState state, IPlaylist playlistOverride)
        {
            if (_playlistOverrides.TryGetValue(state, out var existingOverride) || existingOverride != playlistOverride)
                    _playlistOverrides[state] = playlistOverride;
            PlayStatePlaylist(GameManager.Instance.CachedGameState);
        }

        public void ClearStatePlaylistOverride(GameState state)
        {
            _playlistOverrides.Remove(state);
            PlayStatePlaylist(GameManager.Instance.CachedGameState);
        }
    }
}
