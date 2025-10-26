using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SunsetSystems.UI
{
    public interface ISliderView
    {
        event Action<float> OnValueChange;

        void InitializeView(float value);
        void UpdateView(float value);
    }

    public class VolumeSliderView : MonoBehaviour, ISliderView
    {
        public event Action<float> OnValueChange;

        [SerializeField]
        private Slider _slider;
        [SerializeField]
        private TextMeshProUGUI _valueDisplayText;
        [SerializeField]
        private string _volumeTypePrefix;

        public void InitializeView(float value)
        {
            _slider.value = value;
        }

        public void UpdateView(float value)
        {
            if (value > 0)
                _valueDisplayText.text = $"{_volumeTypePrefix} volume: {Mathf.RoundToInt((value * 100))}%";
            else
                _valueDisplayText.text = $"{_volumeTypePrefix} volume: OFF";
            OnValueChange?.Invoke(value);
        }
    }
}
