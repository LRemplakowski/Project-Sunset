using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SunsetSystems.Abilities.Targeting
{
    public abstract class AbstractTargetingStrategy : IAbilityTargetingStrategy
    {
        public event Action OnExecutionTriggered;

        protected readonly IAbilityConfig _ability;

        private GameObject _vfxInstance;
        private AsyncOperationHandle<AudioClip> _sfxLoading;

        public AbstractTargetingStrategy(IAbilityConfig abilityConfig)
        {
            _ability = abilityConfig;
        }

        public abstract void ExecuteSetTargetLock(ITargetingContext context);
        public abstract void ExecuteClearTargetLock(ITargetingContext context);
        public abstract void ExecutePointerPosition(ITargetingContext context);
        public virtual void ExecuteTargetingBegin(ITargetingContext context)
        {
            HandleAnimatedAbility(context);
            HandleSFXAbility(context);
            HandleVFXAbility(context);

            void HandleAnimatedAbility(ITargetingContext context)
            {
                if (_ability is IAnimatedAbility animatedAbility)
                {
                    context.GetSelf().References.AnimationManager.SetCombatAnimationTypeOverride(animatedAbility.PreCastAnimationType);
                }
            }

            async void HandleSFXAbility(ITargetingContext context)
            {
                if (_ability is ISFXAbility sfxAbility && (sfxAbility.PreparationSFX?.RuntimeKeyIsValid() ?? false))
                {
                    var audioSource = context.GetSFXAudioSource();
                    _sfxLoading = Addressables.LoadAssetAsync<AudioClip>(sfxAbility.PreparationSFX);
                    await _sfxLoading.Task;
                    audioSource.clip = _sfxLoading.Result;
                    audioSource.Play();
                }
            }

            async void HandleVFXAbility(ITargetingContext context)
            {
                if (_vfxInstance != null)
                {
                    Addressables.ReleaseInstance(_vfxInstance);
                }
                if (_ability is IVFXAbility vfxAbility && (vfxAbility.PreCastVfxPrefab?.RuntimeKeyIsValid() ?? false))
                {

                    if (vfxAbility.PreCastVfxPrefab != null)
                    {
                        var body = context.GetSelf().References.Body;
                        var position = context.GetSelf().References.AnimationManager.GetBonePosition(HumanBodyBones.LeftHand);
                        var loadingOp = Addressables.InstantiateAsync(vfxAbility.PreCastVfxPrefab, position, Quaternion.identity, body);
                        await loadingOp.Task;
                        _vfxInstance = loadingOp.Result;
                    }
                }
            }
        }
        public virtual void ExecuteTargetingEnd(ITargetingContext context)
        {
            HandleAnimatedAbility(context);
            HandleVFXAbility(context);
            HandleSFXAbility(context);

            void HandleAnimatedAbility(ITargetingContext context)
            {
                if (_ability is IAnimatedAbility)
                {
                    context.GetSelf().References.AnimationManager.ClearCombatAnimationTypeOverride();
                }
            }

            void HandleVFXAbility(ITargetingContext context)
            {
                if (_vfxInstance != null)
                {
                    if (_vfxInstance.TryGetComponent(out ParticleSystem particleSystem))
                    {
                        particleSystem.Stop();
                        context.GetSelf().CoroutineRunner.StartCoroutine(ReleaseWithDelay(particleSystem.main.startLifetime.constantMax));
                    }
                    else
                    {
                        Addressables.ReleaseInstance(_vfxInstance);
                        _vfxInstance = null;
                    }
                }
            }

            void HandleSFXAbility(ITargetingContext context)
            {
                if (_ability is ISFXAbility)
                {
                    if (_sfxLoading.IsValid())
                    {
                        Addressables.Release(_sfxLoading);
                    }
                }
            }

            IEnumerator ReleaseWithDelay(float delay)
            {
                yield return new WaitForSeconds(delay);
                Addressables.ReleaseInstance(_vfxInstance);
                _vfxInstance = null;
            }
        }

        protected void TriggerExecution()
        {
            OnExecutionTriggered?.Invoke();
        }
    }
}
