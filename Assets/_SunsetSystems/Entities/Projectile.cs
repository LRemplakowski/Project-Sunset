using System;
using System.Collections;
using Sirenix.OdinInspector;
using SunsetSystems.Combat;
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
        private bool _immediateImpact;
        [TabGroup("Behaviour")]
        [SerializeField]
        private float _maxLifetime = 30f;
        [TabGroup("Behaviour")]
        [SerializeField, HideIf("@this._immediateImpact")]
        private bool _isHoming = false;
        [TabGroup("Behaviour")]
        [SerializeField, HideIf("@this._immediateImpact")]
        private float _speed = 10f;
        [TabGroup("Behaviour"), Required]
        [SerializeField]
        private Rigidbody _rigidbody;
        [TabGroup("VFX")]
        [SerializeField]
        private ParticleSystem _trailParticleSystem;
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

        private void Start()
        {
            if (_launchSFX) _launchSFX.loop = false;
            if (_impactSFX) _impactSFX.loop = false;
        }

        private void FixedUpdate()
        {
            if (IsHomingProjectile())
            {
                Vector3 direction = (_target.ProjectileTarget.position - transform.position).normalized;
                _rigidbody.linearVelocity = direction * _speed;
            }
        }

        private bool IsHomingProjectile()
        {
            return !_immediateImpact && _isHoming && !_impacted && _target != null;
        }

        [Button]
        public void Launch(ITargetable target, Action projectileImpactCallback = null)
        {
            _target = target;
            _projectileImpactCallback = projectileImpactCallback;
            if (_immediateImpact)
            {
                LaunchImmediateImpact();
            }
            else
            {
                LaunchAsProjectile();
            }
            StartCoroutine(SelfDestructAfterLifetime());
        }

        private void LaunchImmediateImpact()
        {
            _rigidbody.isKinematic = true;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rigidbody.MovePosition(_target.ProjectileTarget.position);
            if (_launchSFX)
            {
                _launchSFX.Play();
            }
            if (_travelSFX)
            {
                _travelSFX.Play();
            }
        }

        private void LaunchAsProjectile()
        {
            _rigidbody.isKinematic = false;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.linearVelocity = (_target.ProjectileTarget.position - transform.position).normalized * _speed;
            if (_launchSFX)
            {
                _launchSFX.Play();
            }
            if (_travelSFX)
            {
                _travelSFX.Play();
            }
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
                _trailParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            if (_travelSFX)
            {
                _travelSFX.Stop();
            }
            if (_impactSFX)
            {
                _impactSFX.Play();
            }
            _projectileImpactCallback?.Invoke();
            _impacted = true;
            _target = null;
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
                    yield return new WaitWhile(() => IsTrailAlive() || IsImpactAlive() || IsImpactSoundPlaying());
                    Addressables.ReleaseInstance(gameObject);
                    yield break;
                }
            }
            Addressables.ReleaseInstance(gameObject);
        }

        private bool IsTrailAlive()
        {
            return _trailParticleSystem != null && _trailParticleSystem.IsAlive(true);
        }

        private bool IsImpactAlive()
        {
            return _impactParticleSystem != null && _impactParticleSystem.IsAlive(true);
        }

        private bool IsImpactSoundPlaying()
        {
            return _impactSFX != null && _impactSFX.isPlaying;
        }
    }
}
