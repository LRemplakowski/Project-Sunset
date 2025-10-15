using SunsetSystems.ActionSystem;
using SunsetSystems.Entities;
using UltEvents;
using UnityEngine;

namespace SunsetSystems.Combat
{
    public interface ICombatant : IActionPerformer, IContextProvider<ICombatContext>
    {
        UltEvent<ICombatant> OnDamageTaken { get; set; }

        Vector3 AimingOrigin { get; }
        Vector3 NameplatePosition { get; }

        void SignalEndTurn();
    }
}
