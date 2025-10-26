using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SunsetSystems.UI
{
    public class TypewriterSliderView : MonoBehaviour, ISliderView
    {
        public event Action<float> OnValueChange;

        [SerializeField]
        private TextMeshProUGUI _valueDisplayText;
        [SerializeField]
        private Slider _slider;

        public void InitializeView(float value)
        {
            _slider.value = value;
        }

        public void UpdateView(float value)
        {
            if (value > 0)
                _valueDisplayText.text = $"Typewriter Speed: {Mathf.RoundToInt(value)} letters per second";
            else
                _valueDisplayText.text = $"Typewriter Speed: Disabled";
            OnValueChange?.Invoke(value);
        }
    }
}
