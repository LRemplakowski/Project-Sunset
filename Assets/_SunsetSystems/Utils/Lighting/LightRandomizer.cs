using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace SunsetSystems.Utils.Lighting
{
    [RequireComponent(typeof(Light))]
    [RequireComponent(typeof(HDAdditionalLightData))]
    public class LightRandomizer : MonoBehaviour
    {
        private const float TEMPERATURE_MIN = 1500;
        private const float TEMPERATURE_MAX = 20000;

        private const float INTENSITY_MIN = 0;
        private const float INTENSITY_MAX = 3183;

        [SerializeField, HideInInspector]
        private Light _light;
        [SerializeField, HideInInspector]
        private HDAdditionalLightData _lightData;

        [SerializeField, ShowIf("@this._light.lightmapBakeType == LightmapBakeType.Realtime")]
        private bool _randomizeAtRuntime = false;
        [SerializeField]
        private bool _randomizeColor = false;
        [SerializeField, ShowIf("@this._randomizeColor")]
        private Gradient _colorRange;
        [SerializeField]
        private bool _randomizeTemperature = false;
        [SerializeField, ShowIf("@this._randomizeTemperature"), MinMaxSlider(TEMPERATURE_MIN, TEMPERATURE_MAX, true)]
        private Vector2 _temperatureRange = new Vector2(TEMPERATURE_MIN, TEMPERATURE_MAX);
        [SerializeField]
        private bool _randomizeIntensity = false;
        [SerializeField, ShowIf("@this._randomizeIntensity"), MinMaxSlider(INTENSITY_MIN, INTENSITY_MAX, true)]
        private Vector2 _intensityRange;

        private void OnValidate()
        {
            if (_light == null)
                _light = GetComponent<Light>();
            if (_lightData == null)
                _lightData = GetComponent<HDAdditionalLightData>();
        }

        private void Start()
        {
            if (_randomizeAtRuntime && _light.bakingOutput.lightmapBakeType is LightmapBakeType.Realtime or LightmapBakeType.Mixed)
                Randomize();
            Destroy(this);
        }

        [Button]
        private void Randomize()
        {
            RandomizeColor();
            RandomizeTemperature();
            RandomizeIntensity();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_light);
            UnityEditor.EditorUtility.SetDirty(_lightData);
#endif
        }

        private void RandomizeIntensity()
        {
            if (_randomizeIntensity)
            {
                _light.intensity = Random.Range(_intensityRange.x, _intensityRange.y);
            }
        }

        private void RandomizeTemperature()
        {
            if (_randomizeTemperature && _light.useColorTemperature)
            {
                _light.colorTemperature = Random.Range(_temperatureRange.x, _temperatureRange.y);
            }
        }

        private void RandomizeColor()
        {
            if (_randomizeColor)
            {
                _light.color = _colorRange.Evaluate(Random.Range(0, 1));
            }
        }
    }
}
