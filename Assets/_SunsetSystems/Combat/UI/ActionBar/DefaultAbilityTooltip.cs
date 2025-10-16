using Sirenix.OdinInspector;
using SunsetSystems.Abilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SunsetSystems.Combat.UI
{
    public class DefaultAbilityTooltip : SerializedMonoBehaviour, IAbilityButtonTooltip
    {
        [SerializeField]
        private Image _icon;
        [SerializeField]
        private TextMeshProUGUI _title;
        [SerializeField]
        private TextMeshProUGUI _description;
        [SerializeField]
        private TextMeshProUGUI _apCost;
        [SerializeField]
        private TextMeshProUGUI _bloodCost;

        public void Initialize(IAbilityConfig ability)
        {
            var uiData = ability.GetAbilityUIData();
            _title.text = uiData.GetLocalizedName();
            _description.text = uiData.GetLocalizedDescription();
            _icon.sprite = uiData.GetAbilityIcon(IAbilityUIData.IconState.Default);
            var abilityContext = CombatManager.Instance.CurrentActiveActor.References.AbilityUser.GetCurrentAbilityContext();
            var apCost = uiData.GetAPCostText(abilityContext);
            _apCost.gameObject.SetActive(!string.IsNullOrWhiteSpace(apCost));
            _apCost.text = uiData.GetAPCostText(abilityContext);
            var bloodCost = uiData.GetBloodCostText(abilityContext);
            _bloodCost.gameObject.SetActive(!string.IsNullOrWhiteSpace(bloodCost));
            _bloodCost.text = uiData.GetBloodCostText(abilityContext);
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);
    }
}
