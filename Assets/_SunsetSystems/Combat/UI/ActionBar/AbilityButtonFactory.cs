using System;
using Sirenix.OdinInspector;
using SunsetSystems.Abilities;
using SunsetSystems.Equipment;
using UnityEngine;

namespace SunsetSystems.Combat.UI
{
    public interface IAbilityButton
    {
        void Initialize(IAbilityConfig ability, Action<IAbilityConfig> selectionDelegate);
        void SetUpdateAmmoCounterEnabled(bool enabled);
        void OnUpdateAmmoData(WeaponAmmoData ammoData);
    }

    public interface IAbilityButtonFactory
    {
        IAbilityButton Create(Transform parent, IAbilityConfig data, Action<IAbilityConfig> selectionDelegate, out Action<WeaponAmmoData> onUpdateAmmoData);
    }

    [CreateAssetMenu(fileName = "New Ability Button Factory", menuName = "Factories/UI/Ability Button Factory")]
    public class AbilityButtonFactory : SerializedScriptableObject, IAbilityButtonFactory
    {
        [SerializeField, AssetsOnly]
        private IAbilityButton _buttonPrefab;

        public IAbilityButton Create(Transform parent, IAbilityConfig data, Action<IAbilityConfig> selectionDelegate, out Action<WeaponAmmoData> onUpdateAmmoData)
        {
            var buttonObject = Instantiate(_buttonPrefab as UnityEngine.Object, parent);
            var buttonBehaviour = buttonObject as IAbilityButton;
            buttonBehaviour.Initialize(data, selectionDelegate);
            buttonBehaviour.SetUpdateAmmoCounterEnabled(data is IAmmoAbility ammoAbility && ammoAbility.UsesAmmo);
            onUpdateAmmoData = buttonBehaviour.OnUpdateAmmoData;
            return buttonBehaviour;
        }
    }
}