using Redcode.Awaiting;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SunsetSystems.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SFXController : SerializedMonoBehaviour
    {
        [SerializeField, Required]
        private SFXResourceCache sceneSFXCacheAsset;
        [SerializeField]
        private AudioSource _sfxSource, _typewriterSource;

        [SerializeField]
        private List<AudioClip> _typewriterLoop;
        [SerializeField]
        private AudioClip _typewriterEndBell;
        [SerializeField]
        private float _defaultVolume = .5f;
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
                _sfxSource.volume = value;
                _typewriterSource.volume = value;
            }
        }

        private static readonly System.Random _random = new();

        public bool DoPlayTypewriterLoop { get; set; }

        private void Awake()
        {
            _sfxSource ??= GetComponent<AudioSource>();
            _sfxSource ??= gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _typewriterSource ??= GetComponents<AudioSource>().FirstOrDefault(s => s != _sfxSource);
            _typewriterSource ??= gameObject.AddComponent<AudioSource>();
            _typewriterSource.loop = false;
            Volume = _defaultVolume;
        }

        public void PlayOneShot(string sfxName)
        {
            if (sceneSFXCacheAsset.SfxAssetCache.TryGetValue(sfxName, out AudioClip clip))
                PlayOneShot(clip);
            else
                Debug.LogError($"No clip with name {sfxName} found in scene SFX Cache!");
        }

        public void PlayOneShot(AudioClip clip)
        {
            _sfxSource.Stop();
            _sfxSource.PlayOneShot(clip);
        }

        public void PlayTyperwriterLoop()
        {
            DoPlayTypewriterLoop = true;
            PlayTyperwriterSFX();
        }

        private async void PlayTyperwriterSFX()
        {
            AudioClip clip = _typewriterLoop[_random.Next(0, _typewriterLoop.Count)];
            if (DoPlayTypewriterLoop)
            {
                _typewriterSource.Stop();
                _typewriterSource.PlayOneShot(clip);
                await new WaitForSeconds(clip.length);
                PlayTyperwriterSFX();
            }
        }

        public void StopTyperwriterLoop()
        {
            _typewriterSource.Stop();
            DoPlayTypewriterLoop = false;
            _typewriterSource.PlayOneShot(_typewriterEndBell);
        }

        public void StopSFX()
        {
            _sfxSource.Stop();
        }
    }
}
