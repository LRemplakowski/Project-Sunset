using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace SunsetSystems.Animation
{
    [RequireComponent(typeof(RigTransform))]
    public class WeaponAnimationDataProvider : MonoBehaviour
    {
        [field: SerializeField]
        public Transform RightHandIK { get; private set; }
        [field: SerializeField]
        public Transform LeftHandIK { get; private set; }
        [field: SerializeField]
        public Vector3 LeftHintLocalPosition { get; private set; }
        [field: SerializeField]
        public Vector3 RightHintLocalPosition { get; private set; }
        [field: SerializeField]
        public WeaponAnimationType AnimationType { get; private set; }
    }
}
