using System;
using SunsetSystems.Combat;
using SunsetSystems.Combat.Grid;
using UnityEngine;

namespace SunsetSystems.Abilities
{
    public interface ITargetingContext
    {
        Action<ITargetable> TargetUpdateDelegate();
        Action<bool> TargetingLineUpdateDelegate();
        Action<bool> TargetLockSetDelegate();

        IAbilityConfig GetSelectedAbility();
        Collider GetLastRaycastCollider();
        LineRenderer GetTargetingLineRenderer();
        IAbilityContext GetAbilityContext();
        ICombatant GetCurrentCombatant();
        ITargetable GetCurrentTarget();
        GridManager GetCurrentGrid();
        IExecutionConfirmationUI GetExecutionUI();
        AudioSource GetSFXAudioSource();

        bool IsPointerOverUI();
        bool IsTargetLocked();
        bool CanExecuteAbility(IAbilityConfig ability);
    }
}
