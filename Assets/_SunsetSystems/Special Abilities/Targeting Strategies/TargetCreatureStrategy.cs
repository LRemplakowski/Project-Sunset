using System;
using System.Collections;
using SunsetSystems.Combat;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Inventory;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SunsetSystems.Abilities.Targeting
{
    public class TargetCreatureStrategy : IAbilityTargetingStrategy
    {
        private const float TARGETING_RANGE_MARGIN = .5f;
        private static readonly Color TargetInRangeColor = Color.red;
        private static readonly Color TargetOutOfRangeColor = Color.gray;

        private readonly IAbilityConfig _ability;

        private GameObject _vfxInstance;
        private AsyncOperationHandle<AudioClip> _sfxLoading;

        public event Action OnExecutionTriggered;

        public TargetCreatureStrategy(IAbilityConfig ability)
        {
            _ability = ability;
        }

        public void ExecuteSetTargetLock(ITargetingContext context)
        {
            if (ValidateTarget(_ability, context, out ITargetable target) is false)
            {
                ClearTargetingDelegates(context);
                DisableExecutionUI(context);
                return;
            }
            ICombatant current = context.GetSelf();
            context.TargetUpdateDelegate().Invoke(target);
            var abilityRange = _ability.GetTargetingData(context.GetAbilityContext()).GetRangeData();
            bool isTargetInRange = IsTargetInRange(current, target, in abilityRange);
            current.References.NavigationManager.FaceDirectionAfterMovementFinished(target.GetContext().Transform.position);
            context.TargetLockSetDelegate().Invoke(true);
            var executionUI = context.GetExecutionUI();
            if (isTargetInRange)
            {
                if (CanShowTargetingLine(in abilityRange))
                    ShowTargetingLine(context, target.ProjectileTarget.position, TargetInRangeColor);
                executionUI.RegisterConfirmationCallback(TriggerExecution);
                executionUI.UpdateShowInterface(true, () => context.CanExecuteAbility(_ability));
            }
            else
            {
                if (CanShowTargetingLine(in abilityRange))
                    ShowTargetingLine(context, target.ProjectileTarget.position, TargetOutOfRangeColor);
                executionUI.UnregisterConfirmationCallback(TriggerExecution);
                executionUI.UpdateShowInterface(true, () => false);
            }
        }

        public void ExecuteClearTargetLock(ITargetingContext context)
        {
            ClearTargetingDelegates(context);
            DisableExecutionUI(context);
        }

        public void ExecutePointerPosition(ITargetingContext context)
        {
            if (ValidateTarget(_ability, context, out ITargetable target) is false)
            {
                ClearTargetingDelegates(context);
                return;
            }
            context.TargetUpdateDelegate().Invoke(target);
            var abilityRange = _ability.GetTargetingData(context.GetAbilityContext()).GetRangeData();
            ICombatant current = context.GetSelf();
            current.References.NavigationManager.FaceDirectionAfterMovementFinished(target.GetContext().Transform.position);
            if (CanShowTargetingLine(in abilityRange))
            {
                var targetingLineColor = IsTargetInRange(current, target, in abilityRange) ? TargetInRangeColor : TargetOutOfRangeColor;
                ShowTargetingLine(context, target.ProjectileTarget.position, in targetingLineColor);
            }
        }

        public void ExecuteTargetingBegin(ITargetingContext context)
        {
            ClearTargetingDelegates(context);
            DisableExecutionUI(context);
            context.GetTargetingLineRenderer().SetPosition(0, context.GetSelf().AimingOrigin);
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

        public void ExecuteTargetingEnd(ITargetingContext context)
        {
            ClearTargetingDelegates(context);
            DisableExecutionUI(context);
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

        private void ShowTargetingLine(ITargetingContext context, in Vector3 target, in Color lineColor)
        {
            var targetingLineRenderer = context.GetTargetingLineRenderer();
            targetingLineRenderer.SetPosition(1, target);
            targetingLineRenderer.startColor = lineColor;
            targetingLineRenderer.endColor = lineColor;
            context.TargetingLineUpdateDelegate().Invoke(true);
        }

        private void ClearTargetingDelegates(ITargetingContext context)
        {
            context.TargetUpdateDelegate().Invoke(null);
            context.TargetingLineUpdateDelegate().Invoke(false);
            context.TargetLockSetDelegate().Invoke(false);
        }

        private void DisableExecutionUI(ITargetingContext context)
        {
            var executionUI = context.GetExecutionUI();
            executionUI.UnregisterConfirmationCallback(TriggerExecution);
            executionUI.UpdateShowInterface(false, () => false);
        }

        private void TriggerExecution()
        {
            OnExecutionTriggered?.Invoke();
        }

        private static bool ValidateTarget(IAbilityConfig ability, ITargetingContext context, out ITargetable target)
        {
            target = default;
            var collider = context.GetLastRaycastCollider();
            if (collider == null)
                return false;
            if (collider.TryGetComponent(out ICreature targetCreature) is false)
                return false;
            target = targetCreature.References.Targetable;
            return target.IsValidTarget(ability.GetTargetingData(context.GetAbilityContext()).GetValidEntityTypesFlag());
        }

        private static bool IsTargetInRange(ICombatant attacker, ITargetable target, in RangeData abilityRange)
        {
            Vector3Int attackerPosition = attacker.GetContext().GridPosition;
            Vector3Int targetPosition = target.GetContext().GridPosition;
            var gridDistance = Vector3Int.Distance(attackerPosition, targetPosition);
            bool result = gridDistance <= abilityRange.MaxRange + TARGETING_RANGE_MARGIN;
            return result;
        }

        private static bool CanShowTargetingLine(in RangeData abilityRange) => abilityRange.MaxRange > 1;
    }
}
