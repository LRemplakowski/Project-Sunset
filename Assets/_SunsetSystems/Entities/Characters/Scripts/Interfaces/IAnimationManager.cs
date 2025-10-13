namespace SunsetSystems.Animation
{
    public interface IAnimationManager
    {
        void ClearCombatAnimationTypeOverride();
        void PlayFireWeaponAnimation();
        void PlayTakeHitAnimation();
        void SetCombatAnimationTypeOverride(WeaponAnimationType preCastAnimationType);
        void SetCoverAnimationsEnabled(bool enabled);
        void TriggerDeathAnimation();
    }
}