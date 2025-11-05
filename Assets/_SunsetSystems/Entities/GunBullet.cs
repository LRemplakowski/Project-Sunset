using System;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using SunsetSystems.Combat;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SunsetSystems.Entities
{
    public enum TrailType
    {
        ParticleSystem, TrailRenderer
    }

    public class GunBullet : SerializedMonoBehaviour, IProjectile
    {
        [TabGroup("Behaviour")]
        [SerializeField]
        private float _maxLifetime = 5f;
        [TabGroup("Behaviour")]
        [SerializeField]
        private float _speed = 100f;
        [TabGroup("Behaviour")]
        [SerializeField, PropertyRange(0d, 90d)]
        private float _maxDeviation = 5f;
        [TabGroup("Behaviour")]
        [SerializeField, Required]
        private Rigidbody _rigidbody;
        [TabGroup("VFX")]
        [SerializeField]
        private TrailType _trailType = TrailType.ParticleSystem;
        [TabGroup("VFX"), ShowIf("@this._trailType == TrailType.ParticleSystem")]
        [SerializeField]
        private ParticleSystem _trailParticleSystem;
        [TabGroup("VFX"), ShowIf("@this._trailType == TrailType.TrailRenderer")]
        [SerializeField]
        private TrailRenderer _trailRenderer;
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

        private Vector3 _moveVelocity;
        private Collider[] _ignoreColliders;
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
            if (_impacted) return;
            if (_moveVelocity == Vector3.zero) return;

            _rigidbody.MovePosition(_rigidbody.position + (_moveVelocity * Time.fixedDeltaTime));
        }

        [Button]
        public void Launch(ITargetable target, Action projectileImpactCallback = null, params Collider[] ignoreColliders)
        {
            _target = target;
            _projectileImpactCallback = projectileImpactCallback;
            _ignoreColliders = ignoreColliders;
            LaunchAsProjectile();
            StartCoroutine(SelfDestructAfterLifetime());
        }

        private void LaunchAsProjectile()
        {
            _rigidbody.isKinematic = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            Vector3 direction = (_target.ProjectileTarget.position - transform.position).normalized;
            _moveVelocity = DeviateDirection(direction, _maxDeviation) * _speed;
            if (_launchSFX)
            {
                _launchSFX.Play();
            }
            if (_travelSFX)
            {
                _travelSFX.Play();
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (_ignoreColliders != null && _ignoreColliders.Contains(other.collider)) return;

            var contact = other.GetContact(0);
            Impact(contact.point, other.collider.GetComponent<ITargetable>());
        }

        private void Impact(Vector3 impactPoint, ITargetable targetable)
        {
            if (_impactParticleSystem)
            {
                _impactParticleSystem.transform.position = impactPoint;
                _impactParticleSystem.Play();
            }
            if (UseParticleTrail())
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
            if (targetable != null && targetable == _target)
            {
                _projectileImpactCallback?.Invoke();
            }

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

        private bool UseParticleTrail() => _trailType == TrailType.ParticleSystem && _trailParticleSystem != null;
        private bool UseTrailRenderer() => _trailType == TrailType.TrailRenderer && _trailRenderer != null;

        private bool IsTrailAlive()
        {
            return _trailType switch
            {
                TrailType.ParticleSystem => _trailParticleSystem != null && _trailParticleSystem.IsAlive(true),
                TrailType.TrailRenderer => _trailRenderer != null && _trailRenderer.positionCount > 0,
                _ => throw new NotImplementedException(),
            };
        }

        private bool IsImpactAlive()
        {
            return _impactParticleSystem != null && _impactParticleSystem.IsAlive(true);
        }

        private bool IsImpactSoundPlaying()
        {
            return _impactSFX != null && _impactSFX.isPlaying;
        }

        private static Vector3 DeviateDirection(Vector3 direction, float angleDegrees)
        {
            Vector3 randomOffset = UnityEngine.Random.insideUnitSphere;
            Vector3 perpendicular = Vector3.ProjectOnPlane(randomOffset, direction).normalized;
            Quaternion rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, angleDegrees), perpendicular);
            return rotation * direction;
        }
    }
}
