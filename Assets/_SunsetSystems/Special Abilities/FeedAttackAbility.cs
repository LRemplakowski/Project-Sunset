using Sirenix.OdinInspector;
using SunsetSystems.Animation;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SunsetSystems.Abilities
{
    [CreateAssetMenu(fileName = "New Feed Ability", menuName = "Sunset Abilities/Feed Attack")]
    public class FeedAttackAbility : WeaponAttackAbility, IBloodAbility, IAnimatedAbility, ISFXAbility
    {
        [BoxGroup("Feed Ability")]
        [SerializeField]
        private int _bloodGainedOnSuccess = 0;
        [BoxGroup("Feed Ability")]
        [SerializeField]
        private WeaponAnimationType _weaponAnimationTypeOverride;
        [BoxGroup("Feed Ability")]
        [SerializeField]
        private string _castAnimationTrigger;
        [BoxGroup("Feed Ability")]
        [SerializeField]
        private AssetReferenceAudioClip _preparationSFX;
        [BoxGroup("Feed Ability")]
        [SerializeField]
        private AssetReferenceAudioClip _executionSFX;

        public WeaponAnimationType PreCastAnimationType => _weaponAnimationTypeOverride;
        public int CastAnimationHash => Animator.StringToHash(_castAnimationTrigger);
        public int GetBloodGained() => _bloodGainedOnSuccess;

        public AssetReferenceAudioClip PreparationSFX => _preparationSFX;

        public AssetReferenceAudioClip ExecutionSFX => _executionSFX;
    }
}
