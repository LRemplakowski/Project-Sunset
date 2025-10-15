using SunsetSystems.Abilities;
using SunsetSystems.Inventory.Data;
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace SunsetSystems.Inventory
{
    public interface IWeapon : IEquipableItem
    {
        AbilityRange WeaponType { get; }
        int MaxAmmo { get; }

        bool GetWeaponUsesAmmo();
        RangeData GetRangeData();
        DamageData GetDamageData();

        AssetReferenceGameObject EquippedInstanceAsset { get; }
    }

    public struct RangeData
    {
        public float ShortRange, OptimalRange, MaxRange;

        public RangeData(int shortRange, int optimalRange, int maxRange)
        {
            ShortRange = shortRange;
            OptimalRange = optimalRange;
            MaxRange = maxRange;
        }

        public RangeData(float shortRange, float optimalRange, float maxRange)
        {
            ShortRange = shortRange;
            OptimalRange = optimalRange;
            MaxRange = maxRange;
        }
    }

    public struct DamageData
    {
        public int DamageModifier;
        public DamageType DamageType;
    }

    public enum DamageType
    {
        Slashing, Piercing, Bludgeoning, Supernatural, Fire
    }

    [Flags]
    public enum WeaponFireMode
    {
        Single = 1 << 0, 
        Burst = 1 << 1, 
        Auto = 1 << 2
    }

    public enum AbilityRange
    {
        Melee,
        Ranged,
    }
}
