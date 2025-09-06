using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SunsetSystems.Utils;
using UnityEngine;

namespace SunsetSystems.Cinematics
{
    [ExecuteInEditMode]
    public class FadeScreenManager : MonoBehaviour
    {
        public static FadeScreenManager Instance { get; private set; }

        [Title("References")]
        [SerializeField, Required]
        private CanvasGroup _fadeScreenCanvasGroup;

        [Title("Config")]
        [SerializeField]
        private float _defaultFadeTime = .5f;

        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void CycleFade(Action afterFadeOut = null)
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _fadeScreenCanvasGroup.alpha = 0f;
            _fadeScreenCanvasGroup.blocksRaycasts = true;
            _fadeCoroutine = StartCoroutine(CoroutineUtility.LerpAlphaOverTime(_fadeScreenCanvasGroup, _defaultFadeTime, 1f, FadeInWhenCompleted));

            void FadeInWhenCompleted()
            {
                afterFadeOut?.Invoke();
                _fadeCoroutine = StartCoroutine(CoroutineUtility.LerpAlphaOverTime(_fadeScreenCanvasGroup, _defaultFadeTime, 0f, () => _fadeScreenCanvasGroup.blocksRaycasts = false));
            }
        }

        public void FadeOut(float fadeTime, Action onAfterFadeOut = null)
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _fadeScreenCanvasGroup.blocksRaycasts = true;
            _fadeCoroutine = StartCoroutine(CoroutineUtility.LerpAlphaOverTime(_fadeScreenCanvasGroup, fadeTime, 1f, onAfterFadeOut));
        }

        public void FadeOut()
        {
            FadeOut(_defaultFadeTime);
        }

        public void FadeOut(float fadeTime)
        {
            FadeOut(fadeTime, null);
        }

        public void FadeOut(Action onAfterFadeOut = null)
        {
            FadeOut(_defaultFadeTime, onAfterFadeOut);
        }

        public void FadeIn(float fadeTime, Action onAfterFadeIn = null)
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(CoroutineUtility.LerpAlphaOverTime(_fadeScreenCanvasGroup, fadeTime, 0f, AfterFade));

            void AfterFade()
            {
                onAfterFadeIn?.Invoke();
                _fadeScreenCanvasGroup.blocksRaycasts = false;
            }
        }

        public void FadeIn()
        {
            FadeIn(_defaultFadeTime);
        }

        public void FadeIn(float fadeTime)
        {
            FadeIn(fadeTime, null);
        }

        public void FadeIn(Action onAfterFadeIn = null)
        {
            FadeIn(_defaultFadeTime, onAfterFadeIn);
        }
    }
}
