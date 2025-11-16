using System;
using UnityEngine;

namespace SunsetSystems.Animation
{
    public interface IAnimationManager
    {
        event Action<string> OnAnimationEvent;

        void ClearCombatAnimationTypeOverride();
        void PlayFireWeaponAnimation();
        void PlayTakeHitAnimation();
        void SetCombatAnimationTypeOverride(WeaponAnimationType preCastAnimationType);
        void SetCoverAnimationsEnabled(bool enabled);
        void SetIsDead(bool dead);
        void SetTrigger(int hash);
        Vector3 GetBonePosition(HumanBodyBones bone);
    }
}