using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SunsetSystems.Audio
{
    [CreateAssetMenu(fileName = "New Playlist Config", menuName = "Sunset Audio/Playlist Config")]
    public class PlaylistConfig : SerializedScriptableObject, IPlaylist
    {
        [SerializeField]
        private List<AssetReference> _tracks = new();

        private readonly Dictionary<int, AsyncOperationHandle<AudioClip>> _loadedTrackHandles = new();

        private int _currentTrackIndex = 0;
        private bool _firstTrackRequest = true;

        public async UniTask<AudioClip> NextTrack()
        {
            if (_tracks.Count <= 0)
                return default;
            int nextTrackIndex = GetNextTrackIndex();
            var result = await LoadOrGetTrackByIndex(nextTrackIndex);
            _currentTrackIndex = nextTrackIndex;
            return result;
        }

        public async UniTask<AudioClip> PreviousTrack()
        {
            if (_tracks.Count <= 0)
                return default;
            int previousTrackIndex = GetPreviousTrackIndex();
            var result = await LoadOrGetTrackByIndex(previousTrackIndex);
            _currentTrackIndex = previousTrackIndex;
            return result;
        }

        private int GetNextTrackIndex()
        {
            if (_tracks.Count <= 0)
                return -1;

            if (_firstTrackRequest)
            {
                _firstTrackRequest = false;
                return _currentTrackIndex;
            }

            return _currentTrackIndex + 1 >= _tracks.Count ? 0 : _currentTrackIndex + 1;
        }

        private int GetPreviousTrackIndex()
        {
            if (_tracks.Count <= 0)
                return -1;

            if (_firstTrackRequest)
            {
                _firstTrackRequest = false;
                return _currentTrackIndex;
            }

            return _currentTrackIndex - 1 < 0 ? _tracks.Count - 1 : _currentTrackIndex - 1;
        }

        public AudioClip GetCurrentTrack()
        {
            if (_loadedTrackHandles.TryGetValue(_currentTrackIndex, out var handle) && handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }
            return default;
        }

        public void ReleaseReferences()
        {
            foreach (var handle in _loadedTrackHandles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _loadedTrackHandles.Clear();
        }

        private async UniTask<AudioClip> LoadOrGetTrackByIndex(int trackIndex)
        {
            if (_loadedTrackHandles.TryGetValue(trackIndex, out var existingHandle))
            {
                if (existingHandle.Status == AsyncOperationStatus.Succeeded)
                    return existingHandle.Result;
            }

            try
            {
                AssetReference track = _tracks[trackIndex];
                var handle = track.LoadAssetAsync<AudioClip>();
                await handle.ToUniTask();

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _loadedTrackHandles[trackIndex] = handle;
                    return handle.Result;
                }

                Debug.LogError($"Failed to load AudioClip at index {trackIndex} from Addressables.");
                return default;
            }
            catch (IndexOutOfRangeException indexException)
            {
                Debug.LogError($"{nameof(PlaylistConfig)} >>> Failed to load an audio clip at index {trackIndex}!");
                Debug.LogException(indexException, this);
                return AudioClip.Create("FALLBACK", 0, 0, 0, false);
            }
        }
    }
}
