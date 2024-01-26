using Sirenix.OdinInspector;
using SunsetSystems.Dialogue;
using SunsetSystems.Entities;
using SunsetSystems.Entities.Interactable;
using SunsetSystems.Game;
using SunsetSystems.Inventory.UI;
using SunsetSystems.UI;
using SunsetSystems.Utils;
using SunsetSystems.Utils.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SunsetSystems.Input
{
    public class UIBehaviourController : SerializedMonoBehaviour
    {
        [SerializeField]
        private GameplayUIManager gameplayUIParent;
        [SerializeField]
        private GameManager gameManager;
        [SerializeField]
        private LayerMask _raycastTargetMask;

        private Vector2 pointerPosition;

        private void OnEnable()
        {
            SunsetInputHandler.OnInventory += OnInventory;
            SunsetInputHandler.OnCharacterSheet += OnCharacterSheet;
            SunsetInputHandler.OnEscape += OnEscape;
            SunsetInputHandler.OnPointerPosition += OnPointerPosition;
            SunsetInputHandler.OnJournal += OnJournal;
            SunsetInputHandler.OnHighlightInteractables += OnHighlightInteractables;
            SunsetInputHandler.OnHelp += OnShowHelp;
        }

        private void OnDisable()
        {
            SunsetInputHandler.OnInventory -= OnInventory;
            SunsetInputHandler.OnCharacterSheet -= OnCharacterSheet;
            SunsetInputHandler.OnEscape -= OnEscape;
            SunsetInputHandler.OnPointerPosition -= OnPointerPosition;
            SunsetInputHandler.OnJournal -= OnJournal;
            SunsetInputHandler.OnHighlightInteractables -= OnHighlightInteractables;
            SunsetInputHandler.OnHelp -= OnShowHelp;
        }

        private void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
            if (gameplayUIParent != null && Physics.Raycast(ray, out RaycastHit hit, 100f, _raycastTargetMask))
            {
                if (InputHelper.IsRaycastHittingUIObject(pointerPosition, out List<RaycastResult> hits))
                {
                    if (hits.Any(hit => hit.gameObject.GetComponentInParent<CanvasGroup>()?.blocksRaycasts ?? false))
                    {
                        gameplayUIParent.DisableNameplate();
                        return;
                    }
                }
                INameplateReciever nameplateReciever = hit.collider.GetComponent<INameplateReciever>();
                if (nameplateReciever is not null && (nameplateReciever as MonoBehaviour).enabled)
                {
                    if (nameplateReciever is IInteractable interactable && interactable.IsHoveredOver == false)
                    {
                        gameplayUIParent.DisableNameplate();
                    }
                    else
                    {
                        gameplayUIParent.HandleNameplateHover(nameplateReciever);
                    }
                }
                else
                {
                    gameplayUIParent.DisableNameplate();
                }
            }
        }

        private void OnSecondaryAction(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            if (InputHelper.IsRaycastHittingUIObject(pointerPosition, out List<RaycastResult> hits))
            {
                IContextMenuTarget contextMenuTarget;
                if (hits.Any(h => (contextMenuTarget = h.gameObject.GetComponent<IContextMenuTarget>()) is not null))
                {
                    Debug.Log("Secondary action in UI!");
                }
            }
        }

        private void OnEscape(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            if (gameplayUIParent.ContainerGUI.gameObject.activeInHierarchy)
            {
                gameplayUIParent.ContainerGUI.CloseContainerGUI();
                return;
            }
            SwitchPauseAndOpenScreen(PauseMenuScreen.Settings);
        }

        private void OnCharacterSheet(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            SwitchPauseAndOpenScreen(PauseMenuScreen.CharacterSheet);
        }

        private void OnInventory(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            SwitchPauseAndOpenScreen(PauseMenuScreen.Inventory);
        }

        private void OnJournal(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            SwitchPauseAndOpenScreen(PauseMenuScreen.Journal);
        }

        private void OnShowHelp(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                gameplayUIParent.HelpOverlay.SetActive(true);
            }
            else if (context.canceled)
            {
                gameplayUIParent.HelpOverlay.SetActive(false);
            }
        }

        private void SwitchPauseAndOpenScreen(PauseMenuScreen screen)
        {
            if (GameManager.Instance.CurrentState == GameState.Menu)
                return;
            PauseMenuController pauseUI = gameplayUIParent.PauseMenuUI;
            if (GameManager.Instance.IsCurrentState(GameState.GamePaused) && pauseUI.CurrentActiveScreen == screen)
            {
                Debug.Log("Resuming game");
                GameManager.Instance.CurrentState = GameState.Exploration;
                pauseUI.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("Pausing game");
                GameManager.Instance.CurrentState = GameState.GamePaused;
                pauseUI.gameObject.SetActive(true);
                pauseUI.OpenMenuScreen(screen);
            }
        }

        private void OnPointerPosition(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            pointerPosition = context.ReadValue<Vector2>();
        }

        private void OnHighlightInteractables(InputAction.CallbackContext context)
        {
            if (context.performed)
                InteractableEntity.InteractablesInScene.ForEach(interactable => interactable.IsHoveredOver = true);
            else if (context.canceled)
                InteractableEntity.InteractablesInScene.ForEach(interactable => interactable.IsHoveredOver = false);
        }
    }
}
