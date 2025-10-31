using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using SunsetSystems.Game;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public interface IGlobalInput
{
    void OverrideInput(UnityEngine.Object source, params string[] maps);
    void ClearInputOverride(UnityEngine.Object source);
}

public class SunsetInputHandler : SerializedMonoBehaviour, IGlobalInput
{
    public static IGlobalInput Instance { get; private set; }

    public const string PLAYER_MAP = "Player";
    public const string UI_MAP = "UI";
    public const string SHORTCUTS_MAP = "Shortcuts";
    public const string SHORTCUTS_POPUP_MAP = "Shortcuts Popup";

    [SerializeField]
    private PlayerInput _playerInput;

    private readonly Dictionary<string, InputActionMap> _inputMaps = new();
    private readonly List<InputOverride> _inputOverrides = new();

    private readonly struct InputOverride
    {
        public readonly UnityEngine.Object Source;
        public readonly string[] InputMaps;

        public InputOverride(UnityEngine.Object source, string[] inputMaps)
        {
            Source = source;
            InputMaps = inputMaps;
        }
    }

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
    public static event Action<InputAction.CallbackContext> OnHighlightInteractables;
    public static event Action<InputAction.CallbackContext> OnHelp;

    public static event Action<InputAction.CallbackContext> OnPopupCancel;

    private bool _inputDirty = false;

    [Button]
    private void PrintActionMaps()
    {
        _playerInput.actions.actionMaps.ForEach(map => Debug.Log($"Input map: {map.name}"));
    }

    [Button]
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        _playerInput.actions.Disable();
        _inputMaps[PLAYER_MAP] = _playerInput.actions.FindActionMap(PLAYER_MAP, true);
        _inputMaps[UI_MAP] = _playerInput.actions.FindActionMap(UI_MAP, true);
        _inputMaps[SHORTCUTS_MAP] = _playerInput.actions.FindActionMap(SHORTCUTS_MAP, true);
        _inputMaps[SHORTCUTS_POPUP_MAP] = _playerInput.actions.FindActionMap(SHORTCUTS_POPUP_MAP, true);
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void Start()
    {
        UpdateInputMaps();
    }

    private void Update()
    {
        if (_inputDirty)
            UpdateInputMaps();
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
        if (this == (Instance as UnityEngine.Object))
        {
            Instance = null;
        }
    }

    private void OnGameStateChanged(GameState state)
    {
        SetInputDirty();
    }

    private string[] GetInputMapsByGameState(in GameState gameState)
    {
        return gameState switch
        {
            GameState.Exploration => new string[] { PLAYER_MAP, UI_MAP, SHORTCUTS_MAP },
            GameState.Combat => new string[] { PLAYER_MAP, UI_MAP, SHORTCUTS_MAP },
            GameState.Dialogue => new string[] { UI_MAP },
            GameState.MainMenu => new string[] { UI_MAP, SHORTCUTS_MAP },
            GameState.GamePaused => new string[] { UI_MAP, SHORTCUTS_MAP },
            GameState.WorldMap => new string[] { PLAYER_MAP, UI_MAP },
            _ => throw new NotImplementedException(),
        };
    }

    public void OverrideInput(UnityEngine.Object source, params string[] maps)
    {
        InputOverride inputOverride = new(source, maps);
        _inputOverrides.RemoveAll(input => input.Source == source);
        _inputOverrides.Add(inputOverride);
        SetInputDirty();
    }

    public void ClearInputOverride(UnityEngine.Object source)
    {
        int removed = _inputOverrides.RemoveAll(input => input.Source == source);
        if (removed > 0)
            SetInputDirty();
    }

    private void SetInputDirty() => _inputDirty = true;

    private void UpdateInputMaps()
    {
        _playerInput.actions.Disable();
        if (_inputOverrides.Count > 0)
        {
            var inputOverride = _inputOverrides.Last();
            ToggleActionMaps(true, inputOverride.InputMaps);
        }
        else
        {
            var maps = GetInputMapsByGameState(GameManager.Instance.CachedGameState);
            ToggleActionMaps(true, maps);
        }
        _inputDirty = false;
    }

    public void ToggleActionMaps(bool active, params string[] mapNames)
    {
        foreach (var mapName in mapNames)
        {
            if (_inputMaps.TryGetValue(mapName, out var map))
            {
                ToggleMap(active, map);
            }
        }

        static void ToggleMap(bool active, InputActionMap map)
        {
            if (active)
            {
                map.Enable();
            }
            else
            {
                map.Disable();
            }
        }
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

    public void HighlightInteractablesAction(InputAction.CallbackContext context)
    {
        OnHighlightInteractables?.Invoke(context);
    }

    public void Help(InputAction.CallbackContext context)
    {
        OnHelp?.Invoke(context);
    }

    public void PopupCancel(InputAction.CallbackContext context)
    {
        OnPopupCancel?.Invoke(context);
    }
}
