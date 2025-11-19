using System.Collections;
using Sirenix.OdinInspector;
using SunsetSystems.Entities.Characters.Navigation;
using UnityEngine;

namespace SunsetSystems.ActionSystem
{
    [System.Serializable]
    public class Interact : EntityAction
    {
        private readonly IInteractable target;
        private readonly INavigationManager navMeshAgent;
        [ShowInInspector, ReadOnly]
        private Vector3 destination;
        private IEnumerator delayedInteractionCoroutine;

        public Interact(IActionPerformer owner, IInteractable target) : base(owner, false)
        {
            this.target = target;
            this.navMeshAgent = owner.References.NavigationManager;
            this.destination = target.InteractionTransform.position;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            target.Interacted = false;
            if (delayedInteractionCoroutine != null)
                Owner.CoroutineRunner.StopCoroutine(delayedInteractionCoroutine);
        }

        public override void Begin()
        {
            float distance = Vector3.Distance(target.InteractionTransform.position, Owner.References.Transform.position);
            conditions.Add(new InteractionComplete(target));
            if (distance > target.InteractionDistance)
            {
                if (navMeshAgent.SetNavigationTarget(destination))
                {
                    //conditions.Add(new Destination(navMeshAgent));
                    delayedInteractionCoroutine = InteractWhenCloseEnough();
                    Owner.CoroutineRunner.StartCoroutine(delayedInteractionCoroutine);
                }
                else
                {
                    Abort();
                }
            }
            else
            {
                target.TargetedBy = Owner;
                target.Interact();
            }
        }

        private IEnumerator InteractWhenCloseEnough()
        {
            while (Vector3.Distance(target.InteractionTransform.position, Owner.References.Transform.position) > target.InteractionDistance)
                yield return null;
            target.TargetedBy = Owner;
            target.Interact();
            delayedInteractionCoroutine = null;
        }
    }
}
