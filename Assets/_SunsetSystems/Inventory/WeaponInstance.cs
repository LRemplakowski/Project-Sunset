using Sirenix.OdinInspector;
using SunsetSystems.Animation;
using UnityEngine;
using UnityEngine.VFX;
using UMA;
using SunsetSystems.Combat;
using SunsetSystems.Entities;

namespace SunsetSystems.Equipment
{
    [RequireComponent(typeof(AudioSource))]
    public class WeaponInstance : MonoBehaviour, IWeaponInstance
    {
        [Title("Config")]
        [SerializeField, Min(0)]
        private float bulletVelocity = 3f;
        [SerializeField, Min(0)]
        private float bulletLifetime = 5f;
        [Title("References")]
        [SerializeField]
        private Transform projectileOrigin;
        [SerializeField]
        private VisualEffect muzzleFlash;
        [SerializeField]
        private GunBullet bulletPrefab;
        [SerializeField]
        private AudioClip shotSFX;
        [SerializeField]
        private AudioClip reloadSFX;
        [Title("Components")]
        [SerializeField]
        private AudioSource _weaponAudioSource;
        [SerializeField]
        private UMAMountedItem _umaMount;
        [field: SerializeField]
        public WeaponAnimationDataProvider WeaponAnimationData { get; private set; }

        public GameObject GameObject => this.gameObject;

        private ITargetable _target;

        private void OnValidate()
        {
            if (_weaponAudioSource == null)
                _weaponAudioSource = GetComponent<AudioSource>();
            if (_umaMount == null)
                _umaMount = GetComponent<UMAMountedItem>();
        }

        private void Awake()
        {
            if (_umaMount == null)
                _umaMount = GetComponent<UMAMountedItem>();
        }

        private void Start()
        {
            if (_umaMount != null)
                _umaMount.MountItem();
        }

        [Title("Editor Utility")]
        [Button]
        public void PlayFireWeaponFX()
        {
            if (muzzleFlash != null)
                muzzleFlash.Play();
            if (bulletPrefab != null && _target != null)
            {
                IProjectile bulletInstance = Instantiate(bulletPrefab, projectileOrigin.position, Quaternion.identity);
                bulletInstance.Launch(_target);
            }
            if (shotSFX != null)
            {
                _weaponAudioSource.clip = shotSFX;
                _weaponAudioSource.Play();
            }
        }

        [Button]
        public void PlayReloadSFX()
        {
            if (reloadSFX != null)
                _weaponAudioSource.PlayOneShot(reloadSFX);
        }

        public void SetWeaponTarget(ITargetable target) => _target = target;
    }
}
