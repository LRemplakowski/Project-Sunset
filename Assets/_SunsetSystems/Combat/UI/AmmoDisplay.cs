using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SunsetSystems.Equipment;
using TMPro;
using UnityEngine;

namespace SunsetSystems.Combat.UI
{
    public class AmmoDisplay : SerializedMonoBehaviour
    {
        [SerializeField, Required]
        private TextMeshProUGUI _ammoText;

        public void SetAmmoCounterVisible(bool visible)
        {
            if (_ammoText)
            {
                _ammoText.enabled = visible;
                _ammoText.gameObject.SetActive(visible);
            }
        }

        public void UpdateAmmoData(in WeaponAmmoData ammoData)
        {
            if (_ammoText)
                _ammoText.text = $"{ammoData.CurrentAmmo}/{ammoData.MaxAmmo}";
        }
    }
}
