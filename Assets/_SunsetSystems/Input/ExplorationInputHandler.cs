using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime;
using InsaneSystems.RTSSelection;
using Sirenix.OdinInspector;
using SunsetSystems.Entities.Characters;
using SunsetSystems.ActionSystem;
using SunsetSystems.Input.CameraControl;
using SunsetSystems.Party;
using SunsetSystems.Utils.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SunsetSystems.Input
{
    public class ExplorationInputHandler : SerializedMonoBehaviour, IGameplayInputHandler
    {
        private const int RAYCAST_RANGE = 100;

        [Title("References")]
        [SerializeField]
        private Vector2 mousePosition;
        private Collider lastHit;
        [SerializeField]
        private LayerMask _raycastTargetMask;
        [SerializeField]
        private float _companionFollowDistance = 1.0f;
        [SerializeField]
        List<ISelectable> selectables;

        public void HandlePointerPosition(InputAction.CallbackContext context)
        {
            if (context.performed is false)
                return;
            mousePosition = context.ReadValue<Vector2>();
            if (IsOverUI())
                return;

            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, RAYCAST_RANGE, _raycastTargetMask, QueryTriggerInteraction.Collide))
            {
                if (lastHit == null)
                {
                    lastHit = hit.collider;
                }
                HandleExplorationPointerPosition(hit);
            }

            void HandleExplorationPointerPosition(RaycastHit hit)
            {
                IInteractable interactable;
                if (lastHit != hit.collider)
                {
                    if (lastHit.TryGetComponent(out interactable))
                    {
                        interactable.IsHoveredOver = false;
                    }
                    lastHit = hit.collider;
                }
                if (hit.collider.TryGetComponent(out interactable))
                {
                    interactable.IsHoveredOver = true;
                }
            }
        }

        private bool IsOverUI()
        {
            return InputHelper.IsRaycastHittingUIObject(mousePosition, out var results) && results.Select(r => r.gameObject.GetComponentInParent<CanvasGroup>())
                                                                                                  .Any(cg => cg == null || cg.blocksRaycasts);
        }

        public void HandlePrimaryAction(InputAction.CallbackContext context)
        {
            if (context.performed is false)
                return;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, RAYCAST_RANGE, _raycastTargetMask))
            {
                MoveToPositionOrInteract();
            }

            void MoveToPositionOrInteract()
            {
                ICreature mainCharacter = PartyManager.Instance.MainCharacter;
                if (mainCharacter == null)
                {
                    Debug.LogError("MAIN CHARACTER IS NULL!!! WHAT THE FUCK?!");
                    return;
                }
                // Main Character should always take the lead since it's a first entry in ActiveParty list
                List<ICreature> creatures = new();
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    _ = mainCharacter.PerformAction(new Interact(mainCharacter, interactable), true);
                }
                else
                {
                    _ = mainCharacter.PerformAction(new Move(mainCharacter, hit.point), true);
                }
                if (PartyManager.Instance.ActiveParty.Count > 1)
                    creatures.AddRange(PartyManager.Instance.Companions);
                MoveCreaturesToPosition(creatures);
            }
        }

        public void HandleSecondaryAction(InputAction.CallbackContext context)
        {
            Debug.LogWarning("Secondary action not implemented!");
        }


        public void HandleCameraMoveAction(InputAction.CallbackContext context)
        {
            CameraControlScript.Instance.OnMove(context);
        }

        private void MoveCreaturesToPosition(List<ICreature> creatures)
        {
            for (int i = 0; i < creatures.Count; i++)
            {
                ICreature creature = creatures[i];
                if (creature != null && creature.PeekCurrentAction is not Follow)
                {
                    creature.PerformAction(new Follow(creature, PartyManager.Instance.MainCharacter, _companionFollowDistance));
                }
            }
        }
    }
}
