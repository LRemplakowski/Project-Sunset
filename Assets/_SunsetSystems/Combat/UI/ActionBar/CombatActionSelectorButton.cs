using System;
using Sirenix.OdinInspector;
using SunsetSystems.Abilities;
using SunsetSystems.Equipment;
using UnityEngine;
using UnityEngine.UI;

namespace SunsetSystems.Combat.UI
{
    public interface IUITooltip
    {
        void Show();
        void Hide();
    }

    public interface IAbilityButtonTooltip : IUITooltip
    {

        void Initialize(IAbilityConfig ability);
    }

    public class CombatActionSelectorButton : SerializedMonoBehaviour, IAbilityButton
    {
        [Title("Config")]
        [SerializeField]
        private Image _iconDefault;
        [SerializeField]
        private Image _iconSelected;
        [SerializeField]
        private CompositeButton _buttonDefault;
        [SerializeField]
        private CompositeButton _buttonSelected;
        [SerializeField]
        private AmmoDisplay _ammoCounter;
        [SerializeField]
        private IAbilityButtonTooltip _buttonTooltip;
        [Title("Runtime")]
        [ShowInInspector, ReadOnly]
        private IAbilityConfig _cachedAbility;
        [ShowInInspector, ReadOnly]
        private IAbilitySource _cachedAbilitySource;
        private Action<IAbilityConfig> _selectionDelegate;

        private void Awake()
        {
            ActionBarUI.OnAbilitySelected += OnAbilitySelected;
        }

        private void OnDisable()
        {
            _buttonTooltip?.Hide();
        }

        private void OnDestroy()
        {
            ActionBarUI.OnAbilitySelected -= OnAbilitySelected;
        }

        public void OnClick()
        {
            _selectionDelegate?.Invoke(_cachedAbility);
        }

        public void OnPointerEnter()
        {
            _buttonTooltip?.Show();
        }

        public void OnPointerExit()
        {
            _buttonTooltip?.Hide();
        }

        public void Initialize(IAbilityConfig ability, IAbilitySource source, Action<IAbilityConfig> selectionDelegate)
        {
            CacheAbility(ability);
            _cachedAbilitySource = source;
            CacheSelectionDelegate(selectionDelegate);
            SetupButtonVisuals(ability);
            _buttonTooltip.Initialize(ability);
        }

        public void SetUpdateAmmoCounterEnabled(bool enabled)
        {
            _ammoCounter.SetAmmoCounterVisible(enabled);
        }

        public void UpdateAmmoData(in WeaponAmmoData ammoData)
        {
            if (ammoData.Weapon is IAbilitySource abilitySource && _cachedAbilitySource == abilitySource)
                _ammoCounter.UpdateAmmoData(in ammoData);
        }

        private void CacheAbility(IAbilityConfig ability) => _cachedAbility = ability;
        private void CacheSelectionDelegate(Action<IAbilityConfig> selectionDelegate) => _selectionDelegate = selectionDelegate;

        private void SetupButtonVisuals(IAbilityConfig ability)
        {
            var abilituGUIdata = ability.GetAbilityUIData();
            var defaultSprite = abilituGUIdata.GetAbilityIcon(IAbilityUIData.IconState.Default);
            var highlightedSprite = abilituGUIdata.GetAbilityIcon(IAbilityUIData.IconState.Highlighted);
            var pressedSprite = abilituGUIdata.GetAbilityIcon(IAbilityUIData.IconState.Pressed);
            var selectedSprite = abilituGUIdata.GetAbilityIcon(IAbilityUIData.IconState.Selected);
            var disabledSprite = abilituGUIdata.GetAbilityIcon(IAbilityUIData.IconState.Disabled);

            var defaultSpriteStates = new CustomSpriteState()
            {
                Image = _iconDefault,
                HighlightedSprite = highlightedSprite,
                PressedSprite = pressedSprite,
                SelectedSprite = selectedSprite,
                DisabledSprite = disabledSprite
            };
            var abilitySelectedSpriteStates = new CustomSpriteState()
            {
                Image = _iconSelected,
                HighlightedSprite = selectedSprite,
                PressedSprite = selectedSprite,
                SelectedSprite = selectedSprite,
                DisabledSprite = disabledSprite
            };
            UpdateButtonSprites(_iconDefault, _buttonDefault, defaultSprite, in defaultSpriteStates);
            UpdateButtonSprites(_iconSelected, _buttonSelected, selectedSprite, in abilitySelectedSpriteStates);

            UpdateAbilitySelected(false);
        }

        private void OnAbilitySelected(IAbilityConfig config)
        {
            UpdateAbilitySelected(config == _cachedAbility);
        }

        private void UpdateAbilitySelected(bool selected)
        {
            _buttonDefault.gameObject.SetActive(!selected);
            _buttonDefault.interactable = !selected;
            _buttonSelected.gameObject.SetActive(selected);
            _buttonSelected.interactable = selected;
        }

        private static void UpdateButtonSprites(Image icon, CompositeButton button, Sprite normal, in CustomSpriteState buttonSpriteStates)
        {
            icon.sprite = normal;
            button.ClearCompositeData();
            button.AddSpriteSwapComposite(buttonSpriteStates);
        }
    }
}
