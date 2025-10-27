using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class SunsetInputHandler : SerializedMonoBehaviour
{
    [SerializeField]
    private PlayerInput _playerInput;
    [SerializeField]
    private InputSystemUIInputModule _uiInputModule;

    private PlayerInputMapping _input;

    // Mouse input
    public static event Action<InputAction.CallbackContext> OnPrimaryAction;
    public static event Action<InputAction.CallbackContext> OnSecondaryAction;
    public static event Action<InputAction.CallbackContext> OnPointerPositionAction;
    public static event Action<InputAction.CallbackContext> OnCameraMoveAction;
    // Keyboard input
    public static event Action<InputAction.CallbackContext> OnInventory;
    public static event Action<InputAction.CallbackContext> OnJournal;
    public static event Action<InputAction.CallbackContext> OnEscape;
    public static event Action<InputAction.CallbackContext> OnCharacterSheet;
    public static event Action<InputAction.CallbackContext> OnSkipDialogue;
    public static event Action<InputAction.CallbackContext> OnHighlightInteractables;
    public static event Action<InputAction.CallbackContext> OnHelp;

    [Button]
    private void Awake()
    {
        _input ??= new();
        //SubscribePlayerActions();
        //_input.Enable();
        //_input.Player.Enable();
        //_input.UI.Enable();
        _playerInput.actions.actionMaps.ForEach(map => map.Enable());
        //_uiInputModule.actionsAsset = _input.asset;
    }

    private void SubscribePlayerActions()
    {
        _input.Player.LeftClick.started += PrimaryAction;
        _input.Player.LeftClick.performed += PrimaryAction;
        _input.Player.LeftClick.canceled += PrimaryAction;
        _input.Player.RightClick.started += SecondaryAction;
        _input.Player.RightClick.performed += SecondaryAction;
        _input.Player.RightClick.canceled += SecondaryAction;
        _input.Player.MousePosition.started += PointerPosition;
        _input.Player.MousePosition.performed += PointerPosition;
        _input.Player.MousePosition.canceled += PointerPosition;
    }

    public void PrimaryAction(InputAction.CallbackContext context)
    {
        OnPrimaryAction?.Invoke(context);
    }

    public void SecondaryAction(InputAction.CallbackContext context)
    {
        OnSecondaryAction?.Invoke(context);
    }

    public void PointerPosition(InputAction.CallbackContext context)
    {
        OnPointerPositionAction?.Invoke(context);
    }

    public void CameraMoveAction(InputAction.CallbackContext context)
    {
        OnCameraMoveAction?.Invoke(context);
    }

    public void InventoryAction(InputAction.CallbackContext context)
    {
        OnInventory?.Invoke(context);
    }

    public void JournalAction(InputAction.CallbackContext context)
    {
        OnJournal?.Invoke(context);
    }

    public void EscapeAction(InputAction.CallbackContext context)
    {
        OnEscape?.Invoke(context);
    }

    public void CharacterSheetAction(InputAction.CallbackContext context)
    {
        OnCharacterSheet?.Invoke(context);
    }

    public void SkipDialogueAction(InputAction.CallbackContext context)
    {
        OnSkipDialogue?.Invoke(context);
    }

    public void HighlightInteractablesAction(InputAction.CallbackContext context)
    {
        OnHighlightInteractables?.Invoke(context);
    }

    public void Help(InputAction.CallbackContext context)
    {
        OnHelp?.Invoke(context);
    }
}
