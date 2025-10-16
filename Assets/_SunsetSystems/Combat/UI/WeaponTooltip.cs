using SunsetSystems.Equipment;
using SunsetSystems.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SunsetSystems.Combat.UI
{
    public class WeaponTooltip : SimpleTooltip, IWeaponTooltip
    {
        [SerializeField]
        private Image _weaponIcon;
        [SerializeField]
        private TextMeshProUGUI _weaponName;
        [SerializeField]
        private TextMeshProUGUI _weaponDescription;
        [SerializeField]
        private TextMeshProUGUI _damageBonus;
        [SerializeField]
        private TextMeshProUGUI _maxAmmo;
        [SerializeField]
        private TextMeshProUGUI _maxRange;
        [SerializeField]
        private TextMeshProUGUI _optimalRange;

        private SelectedWeapon _weaponSlot;

        public void Initialize(SelectedWeapon weapon)
        {
            _weaponSlot = weapon;
        }

        public override void Show()
        {
            base.Show();
            SetTooltipData();
        }

        private void SetTooltipData()
        {
            var weaponManager = CombatManager.Instance.CurrentActiveActor.References.WeaponManager;
            var weapon = GetWeapon(weaponManager);
            if (weapon != null)
            {
                _weaponIcon.sprite = weapon.Icon;
                _weaponName.text = weapon.TooltipName;
                _weaponDescription.text = weapon.ItemDescription;
                _damageBonus.text = $"Damage Bonus: {weapon.GetDamageData().DamageModifier}";
                bool useAmmo = weapon.GetWeaponUsesAmmo();
                bool isRanged = weapon.WeaponType == AbilityRange.Ranged;
                _maxAmmo.gameObject.SetActive(useAmmo);
                _maxAmmo.text = $"Max Ammo: {weapon.MaxAmmo}";
                var rangeData = weapon.GetRangeData();
                _optimalRange.gameObject.SetActive(isRanged);
                _maxRange.gameObject.SetActive(isRanged);
                _optimalRange.text = $"Optimal Range: {rangeData.OptimalRange}";
                _maxRange.text = $"Max Range: {rangeData.MaxRange}";
            }
        }

        private IWeapon GetWeapon(IWeaponManager weaponManager)
        {
            return _weaponSlot switch 
            { 
                SelectedWeapon.Primary => weaponManager.GetPrimaryWeapon(), 
                SelectedWeapon.Secondary => weaponManager.GetSecondaryWeapon(), 
                _ => weaponManager.GetPrimaryWeapon() };
        }
    }
}
