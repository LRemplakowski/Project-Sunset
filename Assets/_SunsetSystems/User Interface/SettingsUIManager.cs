using SunsetSystems.Audio;
using SunsetSystems.Core;
using SunsetSystems.Dialogue;
using SunsetSystems.UI;
using UnityEngine;

namespace SunsetSystems
{
    public class SettingsUIManager : MonoBehaviour
    {
        [SerializeField]
        private VolumeSliderView _musicSlider, _sfxSlider;
        [SerializeField]
        private TypewriterSliderView _typewriterSlider;

        private void OnEnable()
        {
            if (PlayerPrefs.HasKey(SettingsConstants.MUSIC_VOLUME_KEY))
                _musicSlider.InitializeView(PlayerPrefs.GetFloat(SettingsConstants.MUSIC_VOLUME_KEY));
            else
                _musicSlider.InitializeView(AudioManager.Instance.MusicDefaultValue);
            if (PlayerPrefs.HasKey(SettingsConstants.SFX_VOLUME_KEY))
                _sfxSlider.InitializeView(PlayerPrefs.GetFloat(SettingsConstants.SFX_VOLUME_KEY));
            else
                _sfxSlider.InitializeView(AudioManager.Instance.SFXDefaultValue);
            if (PlayerPrefs.HasKey(SettingsConstants.TYPEWRITER_SPEED_KEY))
                _typewriterSlider.InitializeView(PlayerPrefs.GetInt(SettingsConstants.TYPEWRITER_SPEED_KEY));
            else
                _typewriterSlider.InitializeView(DialogueManager.Instance.DefaultTypewriterValue);
            _musicSlider.OnValueChange += SignalMusicVolumeChange;
            _sfxSlider.OnValueChange += SignalSFXVolumeChange;
            _typewriterSlider.OnValueChange += SignalTypewriterSpeedChange;
        }

        private void OnDisable()
        {
            _musicSlider.OnValueChange -= SignalMusicVolumeChange;
            _sfxSlider.OnValueChange -= SignalSFXVolumeChange;
            _typewriterSlider.OnValueChange -= SignalTypewriterSpeedChange;
        }

        public void SignalMusicVolumeChange(float volume)
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }

        public void SignalSFXVolumeChange(float volume)
        {
            AudioManager.Instance.SetSFXVolume(volume);
        }

        private void SignalTypewriterSpeedChange(float speed)
        {
            DialogueManager.Instance.SetTypewriterSpeed(speed);
        }
    }
}
