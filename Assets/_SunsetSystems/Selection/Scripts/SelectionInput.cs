using InsaneSystems.RTSSelection.UI;
using SunsetSystems.Utils;
using SunsetSystems.Utils.Input;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace InsaneSystems.RTSSelection
{
    /// <summary> In this class handled all player selection input. </summary>
    [RequireComponent(typeof(Selection))]
    public class SelectionInput : Singleton<SelectionInput>
    {
        SelectionRect SelectionRect => throw new NotImplementedException();

        Vector2 startMousePosition = new();
        Vector2 mousePosition = new();

        int selectionButton;

        private void OnEnable()
        {
            SunsetInputHandler.OnPrimaryAction += OnPrimaryAction;
            SunsetInputHandler.OnPointerPositionAction += OnPointerPosition;
        }

        private void OnDisable()
        {
            SunsetInputHandler.OnPrimaryAction -= OnPrimaryAction;
            SunsetInputHandler.OnPointerPositionAction -= OnPointerPosition;
        }

        private void Start()
        {
            Selection selection = null;
            if (selection == null)
                selection = GetComponent<Selection>();
        }

        public void OnPrimaryAction(InputAction.CallbackContext context)
        {
            if (context.performed && !InputHelper.IsRaycastHittingUIObject(mousePosition, out List<RaycastResult> _))
            {
                HandleClick();
            }
            else if (context.canceled)
            {
                HandleClickRelease();
            }
        }

        private void HandleClick()
        {
            Selection selection = null;
            startMousePosition = new Vector2(mousePosition.x, mousePosition.y);
            selection.StartSelection();
            SelectionRect.EnableRect(startMousePosition);
        }

        private void HandleClickRelease()
        {
            Selection selection = null;
            selection.FinishSelection(startMousePosition, mousePosition);
            SelectionRect.DisableRect();
        }

        public void OnPointerPosition(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            mousePosition = context.ReadValue<Vector2>();
        }
    }
}