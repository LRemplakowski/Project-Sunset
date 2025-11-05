using SunsetSystems.Animation;
using SunsetSystems.Combat;
using UnityEngine;

namespace SunsetSystems.Equipment
{
    public interface IWeaponInstance
    {
        GameObject GameObject { get; }
        WeaponAnimationDataProvider WeaponAnimationData { get; }

        void SetWeaponTarget(ITargetable target);
        void PlayFireWeaponFX();
        void PlayReloadSFX();
    }
}
