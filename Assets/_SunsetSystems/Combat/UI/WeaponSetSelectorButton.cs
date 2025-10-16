using System;
using Sirenix.OdinInspector;
using SunsetSystems.Equipment;
using UnityEngine;
using UnityEngine.Serialization;

namespace SunsetSystems.Combat.UI
{
    public interface IWeaponTooltip : IUITooltip
    {
        void Initialize(SelectedWeapon weapon);
    }

    public class WeaponSetSelectorButton : SerializedMonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("associatedWeapon")]
        private SelectedWeapon _associatedWeapon;
        [SerializeField]
        private IWeaponTooltip _weaponTooltip;

        public static event Action<SelectedWeapon> OnWeaponSelected;

        private void Start()
        {
            _weaponTooltip.Initialize(_associatedWeapon);
        }

        public void OnClick()
        {
            OnWeaponSelected?.Invoke(_associatedWeapon);
        }
    }
}
