using SunsetSystems.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevGUIManager : ExposableMonobehaviour
{
    private DevTurnCombatGUI devTurnCombatGUI;

    private void Start()
    {
        devTurnCombatGUI = GetComponentInChildren<DevTurnCombatGUI>();
        devTurnCombatGUI.gameObject.SetActive(GameManager.Instance.IsCurrentState(GameState.Combat));
    }
}
