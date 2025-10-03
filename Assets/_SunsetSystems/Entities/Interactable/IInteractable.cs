using SunsetSystems.ActionSystem;
using UnityEngine;

public interface IInteractable
{
    bool ForceHover { get; set; }
    bool IsHoveredOver { get; set; }
    float InteractionDistance { get; set; }
    IActionPerformer TargetedBy { get; set; }
    Transform InteractionTransform { get; set; }
    bool Interacted { get; set; }
    bool Interactable { get; set; }
    void Interact();
}
