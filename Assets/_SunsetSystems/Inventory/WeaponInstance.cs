using Sirenix.OdinInspector;
using SunsetSystems.Animation;
using UnityEngine;
using UnityEngine.VFX;

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
        private Rigidbody bulletPrefab;
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
            if (bulletPrefab)
            {
                Rigidbody bulletInstance = Instantiate(bulletPrefab, projectileOrigin.position, Quaternion.identity);
                bulletInstance.AddForce(projectileOrigin.forward * bulletVelocity);
                Destroy(bulletInstance.gameObject, bulletLifetime);
            }
            if (shotSFX != null)
                _weaponAudioSource.PlayOneShot(shotSFX);
        }

        [Button]
        public void PlayReloadSFX()
        {
            if (reloadSFX != null)
                _weaponAudioSource.PlayOneShot(reloadSFX);
        }
    }
}
