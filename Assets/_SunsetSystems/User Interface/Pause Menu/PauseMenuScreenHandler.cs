using System;
using Sirenix.OdinInspector;
using SunsetSystems.Core.SceneLoading;
using SunsetSystems.Game;
using UltEvents;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SunsetSystems.UI
{
    public class PauseMenuScreenHandler : SerializedMonoBehaviour
    {
        [SerializeField]
        private GameObject inventory, journal, settings, characterSheet;
        [SerializeField]
        private CharacterSelector _characterSelector;
        [SerializeField]
        private GameObject _saveLoadScreen;

        [SerializeField]
        private UltEvent _onReturnToMenu;
        
        public PauseMenuScreen CurrentActiveScreen { get; private set; }
        private GameObject _lastSelectedScreen;

        public void QuitToMenu()
        {
            _onReturnToMenu?.Invoke();
            LevelLoader.Instance.BackToMainMenu();
        }

        public void OpenPauseMenu() => OpenMenuScreen(PauseMenuScreen.Settings);
        public void ClosePauseMenu() => OpenMenuScreen(PauseMenuScreen.None);

        public void OpenMenuScreen(PauseMenuScreen screen)
        {
            if (GameManager.Instance.CachedGameState == GameState.Dialogue)
                return;
            if (_lastSelectedScreen != null)
                _lastSelectedScreen.SetActive(false);
            switch (screen)
            {
                case PauseMenuScreen.Settings:
                    gameObject.SetActive(true);
                    _characterSelector.SetActive(false);
                    settings.SetActive(true);
                    _lastSelectedScreen = settings;
                    break;
                case PauseMenuScreen.Inventory:
                    gameObject.SetActive(true);
                    _characterSelector.SetActive(true);
                    inventory.SetActive(true);
                    _lastSelectedScreen = inventory;
                    break;
                case PauseMenuScreen.CharacterSheet:
                    gameObject.SetActive(true);
                    _characterSelector.SetActive(true);
                    characterSheet.SetActive(true);
                    _lastSelectedScreen = characterSheet;
                    break;
                case PauseMenuScreen.Journal:
                    gameObject.SetActive(true);
                    _characterSelector.SetActive(false);
                    journal.SetActive(true);
                    _lastSelectedScreen = journal;
                    break;
                case PauseMenuScreen.Map:
                    break;
                case PauseMenuScreen.None:
                    gameObject.SetActive(false);
                    _lastSelectedScreen = null;
                    break;
            }
            CurrentActiveScreen = screen;
        }

        public void OnCancelAction(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (gameObject.activeInHierarchy is true && _saveLoadScreen.activeInHierarchy is false)
                {
                    OpenMenuScreen(PauseMenuScreen.None);
                }
                else
                {
                    _saveLoadScreen.SetActive(false);
                    OpenMenuScreen(PauseMenuScreen.Settings);
                }
            }
        }

        public void OnCharacterSheetAction(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (CurrentActiveScreen == PauseMenuScreen.CharacterSheet)
                {
                    OpenMenuScreen(PauseMenuScreen.None);
                }
                else
                {
                    OpenMenuScreen(PauseMenuScreen.CharacterSheet);
                }
            }
        }

        public void OnInventoryAction(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (CurrentActiveScreen == PauseMenuScreen.Inventory)
                {
                    OpenMenuScreen(PauseMenuScreen.None);
                }
                else
                {
                    OpenMenuScreen(PauseMenuScreen.Inventory);
                }
            }
        }

        public void OnJournalAction(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (CurrentActiveScreen == PauseMenuScreen.Journal)
                {
                    OpenMenuScreen(PauseMenuScreen.None);
                }
                else
                {
                    OpenMenuScreen(PauseMenuScreen.Journal);
                }
            }
        }

        public void SetSelectedCharacter(string characterID) => _characterSelector.SelectCharacter(characterID);
    }

    public enum PauseMenuScreen
    {
        Settings,
        Inventory,
        CharacterSheet,
        Journal,
        Map,
        None
    }
}