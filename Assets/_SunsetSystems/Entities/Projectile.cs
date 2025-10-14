using System;
using System.Collections;
using Sirenix.OdinInspector;
using SunsetSystems.Combat;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SunsetSystems.Entities
{
    public interface IProjectile
    {
        void Launch(ITargetable target, Action projectileImpactCallback = null);
    }

    public class Projectile : SerializedMonoBehaviour, IProjectile
    {
        [TabGroup("Behaviour")]
        [SerializeField]
        private float _maxLifetime = 100f;
        [TabGroup("Behaviour")]
        [SerializeField, MinValue("@this._trailFadeOutDuration")]
        private float _lifetimeAfterImpact = 2f;
        [TabGroup("Behaviour")]
        [SerializeField]
        private bool _isHoming = false;
        [TabGroup("Behaviour")]
        [SerializeField]
        private float _speed = 10f;
        [TabGroup("Behaviour"), Required]
        [SerializeField]
        private Rigidbody _rigidbody;
        [TabGroup("VFX")]
        [SerializeField]
        private ParticleSystem _trailParticleSystem;
        [TabGroup("VFX")]
        [SerializeField]
        private float _trailFadeOutDuration = 1f;
        [TabGroup("VFX")]
        [SerializeField]
        private ParticleSystem _impactParticleSystem;
        [TabGroup("SFX")]
        [SerializeField]
        private AudioSource _launchSFX;
        [TabGroup("SFX")]
        [SerializeField]
        private AudioSource _travelSFX;
        [TabGroup("SFX")]
        [SerializeField]
        private AudioSource _impactSFX;

        private ITargetable _target;
        private Action _projectileImpactCallback;
        private bool _impacted = false;

        private void FixedUpdate()
        {
            if (_isHoming && !_impacted && _target != null)
            {
                Vector3 direction = (_target.ProjectileTarget.position - transform.position).normalized;
                _rigidbody.linearVelocity = direction * _speed;
            }
        }

        [Button]
        public void Launch(ITargetable target, Action projectileImpactCallback = null)
        {
            _target = target;
            _rigidbody.isKinematic = false;
            _rigidbody.linearVelocity = (_target.ProjectileTarget.position - transform.position).normalized * _speed;
            _projectileImpactCallback = projectileImpactCallback;
            if (_launchSFX)
            {
                _launchSFX.Play();
            }
            if (_travelSFX)
            {
                _travelSFX.Play();
            }
            StartCoroutine(SelfDestructAfterLifetime());
        }

        private void OnTriggerEnter(Collider other)
        {
            var targetable = other.GetComponentInChildren<ITargetable>();
            if (targetable != null && targetable == _target)
            {
                Impact();
            }
        }

        private void Impact()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            if (_impactParticleSystem)
            {
                _impactParticleSystem.transform.position = _target.ProjectileTarget.position;
                _impactParticleSystem.Play();
            }
            if (_trailParticleSystem)
            {
                StartCoroutine(FadeOutTrail());
            }
            if (_impactSFX)
            {
                _impactSFX.Play();
            }
            _projectileImpactCallback?.Invoke();
            _impacted = true;
            _target = null;
        }

        private IEnumerator FadeOutTrail()
        {
            if (_trailParticleSystem == null) yield break;

            _trailParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            var main = _trailParticleSystem.main;

            // Cache original startColor; handle MinMaxGradient cases
            ParticleSystem.MinMaxGradient startGradient = main.startColor;
            Color baseColor = startGradient.mode == ParticleSystemGradientMode.Color
                ? startGradient.color
                : startGradient.gradient.Evaluate(0f); // fallback if gradient

            float elapsed = 0f;

            while (elapsed < _trailFadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _trailFadeOutDuration);
                float alpha = Mathf.Lerp(1f, 0f, t);

                // Apply new alpha while keeping RGB
                var faded = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                main.startColor = new ParticleSystem.MinMaxGradient(faded);
                yield return null;
            }

            // Ensure fully transparent at end
            var final = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
            main.startColor = new ParticleSystem.MinMaxGradient(final);
        }


        private IEnumerator SelfDestructAfterLifetime()
        {
            float lifeTime = 0f;
            while (lifeTime < _maxLifetime)
            {
                yield return null;
                lifeTime += Time.deltaTime;
                if (_impacted)
                {
                    yield return new WaitForSeconds(_lifetimeAfterImpact);
                    Addressables.ReleaseInstance(gameObject);
                    yield break;
                }
            }
            Addressables.ReleaseInstance(gameObject);
        }
    }
}
