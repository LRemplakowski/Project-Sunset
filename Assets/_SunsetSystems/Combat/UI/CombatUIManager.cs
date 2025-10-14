using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SunsetSystems.Equipment;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SunsetSystems.Combat.UI
{
    public class CombatUIManager : SerializedMonoBehaviour
    {
        [Title("References")]
        [SerializeField, Required]
        private Image currentActorPortrait;
        [SerializeField, Required]
        private ActionBarUI _actionBarUI;
        [SerializeField, Required]
        private PlayerHealthDisplay currentActorHealth;
        [SerializeField, Required]
        private ResourceBarDisplay apBar, bpBar;
        [SerializeField, Required]
        private CanvasGroup _combatCanvasGroup;

        private void Awake()
        {
            CombatManager.OnCombatStart += OnCombatBegin;
            CombatManager.OnCombatEnd += OnCombatEnd;
            CombatManager.OnCombatRoundBegin += OnCombatRoundBegin;
            CombatManager.OnCombatRoundEnd += OnCombatRoundEnd;
        }

        private void OnEnable()
        {
            currentActorPortrait.color = Color.clear;
        }

        private void OnDestroy()
        {
            CombatManager.OnCombatStart -= OnCombatBegin;
            CombatManager.OnCombatEnd -= OnCombatEnd;
            CombatManager.OnCombatRoundBegin -= OnCombatRoundBegin;
            CombatManager.OnCombatRoundEnd -= OnCombatRoundEnd;
        }

        public void OnCombatBegin(IEnumerable<ICombatant> _)
        {
            _combatCanvasGroup.interactable = false;
            _combatCanvasGroup.blocksRaycasts = true;
            _combatCanvasGroup.alpha = 1f;
        }

        public void OnCombatEnd(IEnumerable<ICombatant> _)
        {
            _combatCanvasGroup.interactable = false;
            _combatCanvasGroup.blocksRaycasts = false;
            _combatCanvasGroup.alpha = 0f;
        }

        public void OnCombatRoundBegin(ICombatant combatant)
        {
            EventSystem.current.SetSelectedGameObject(null);
            var context = combatant.GetContext();
            _combatCanvasGroup.interactable = context.IsPlayerControlled;
            if (context.IsPlayerControlled)
            {
                InitialUpdateUserInterface(combatant);
                SubscribeInterfaceUpdateEvents(combatant);
            }
        }

        private void SubscribeInterfaceUpdateEvents(ICombatant combatant)
        {
            var context = combatant.GetContext();
            var apManager = context.ActionPointManager;
            var bpManager = context.BloodPointManager;
            var weaponManager = context.WeaponManager;
            apManager.OnActionPointUpdate += OnActionUsed;
            bpManager.OnBloodPointUpdate += OnBloodPointSpent;
            weaponManager.OnWeaponChanged += OnWeaponChanged;
            weaponManager.OnAmmoChanged += OnAmmoChanged;
        }

        private void InitialUpdateUserInterface(ICombatant dataSource)
        {
            var context = dataSource.GetContext();
            var weaponManager = context.WeaponManager;
            var apManager = context.ActionPointManager;
            var bpManager = context.BloodPointManager;
            UpdateCurrentActorPortrait(dataSource);
            currentActorHealth.UpdateHealthDisplay(dataSource);
            apBar.UpdateActiveChunks(apManager.GetCurrentActionPoints());
            bpBar.UpdateActiveChunks(bpManager.GetCurrentBloodPoints());
            _actionBarUI.RefreshAvailableActions();
            _actionBarUI.UpdateAmmoCounter(weaponManager.GetSelectedWeaponAmmoData());
        }

        private void OnWeaponChanged(ICombatant combatant)
        {
            _actionBarUI.RefreshAvailableActions();
            var weaponManager = combatant.GetContext().WeaponManager;
            _actionBarUI.UpdateAmmoCounter(weaponManager.GetSelectedWeaponAmmoData());
        }

        private void OnActionUsed(int currentAP)
        {
            apBar.UpdateActiveChunks(currentAP);
        }

        private void OnBloodPointSpent(int currentBP)
        {
            bpBar.UpdateActiveChunks(currentBP);
        }

        private void OnAmmoChanged(ICombatant combatant, WeaponAmmoData ammoData)
        {
            _actionBarUI.UpdateAmmoCounter(ammoData);
        }

        private void UpdateCurrentActorPortrait(ICombatant actor)
        {
            currentActorPortrait.sprite = actor.References.CreatureData.Portrait;
            if (currentActorPortrait.color == Color.clear)
                StartCoroutine(ShowPortrait());
        }

        private IEnumerator ShowPortrait()
        {
            float time = 0f;
            while (time < 1f)
            {
                currentActorPortrait.color = Color.Lerp(Color.clear, Color.white, time);
                time += Time.deltaTime;
                yield return null;
            }
        }

        public void OnCombatRoundEnd(ICombatant combatant)
        {
            var context = combatant.GetContext();
            context.ActionPointManager.OnActionPointUpdate -= OnActionUsed;
            context.BloodPointManager.OnBloodPointUpdate -= OnBloodPointSpent;
            var weaponManager = context.WeaponManager;
            weaponManager.OnWeaponChanged -= OnWeaponChanged;
            weaponManager.OnAmmoChanged -= OnAmmoChanged;
        }
    }
}
