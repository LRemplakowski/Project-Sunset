using System;
using Sirenix.OdinInspector;
using SunsetSystems.Abilities;
using SunsetSystems.Equipment;
using UnityEngine;

namespace SunsetSystems.Combat.UI
{
    public interface IAbilityButton
    {
        void Initialize(IAbilityConfig ability, IAbilitySource source, Action<IAbilityConfig> selectionDelegate);
        void SetUpdateAmmoCounterEnabled(bool enabled);
        void UpdateAmmoData(in WeaponAmmoData ammoData);
    }

    public interface IAbilityButtonFactory
    {
        IAbilityButton Create(Transform parent, IAbilityConfig data, IAbilitySource source, Action<IAbilityConfig> selectionDelegate);
    }

    [CreateAssetMenu(fileName = "New Ability Button Factory", menuName = "Factories/UI/Ability Button Factory")]
    public class AbilityButtonFactory : SerializedScriptableObject, IAbilityButtonFactory
    {
        [SerializeField, AssetsOnly]
        private IAbilityButton _buttonPrefab;

        public IAbilityButton Create(Transform parent, IAbilityConfig data, IAbilitySource source, Action<IAbilityConfig> selectionDelegate)
        {
            var buttonObject = Instantiate(_buttonPrefab as UnityEngine.Object, parent);
            var buttonBehaviour = buttonObject as IAbilityButton;
            buttonBehaviour.Initialize(data, source, selectionDelegate);
            buttonBehaviour.SetUpdateAmmoCounterEnabled(data is IAmmoAbility ammoAbility && ammoAbility.UsesAmmo);
            return buttonBehaviour;
        }
    }
}