using System.Collections;
using SunsetSystems.Entities.Characters.Navigation;
using SunsetSystems.Entities.Interfaces;
using UnityEngine;

namespace SunsetSystems.ActionSystem
{
    [System.Serializable]
    public class Follow : EntityAction
    {
        private readonly INavigationManager followTarget;
        private readonly INavigationManager myAgent;
        private readonly float followDistance;

        private IEnumerator followCoroutine;
        private bool following;

        public Follow(IActionPerformer owner, IEntity followTarget, float followDistance) : base(owner)
        {
            this.followTarget = followTarget.References.GetCachedComponent<INavigationManager>();
            myAgent = owner.References.NavigationManager;
            following = false;
            this.followDistance = followDistance;
            conditions.Add(new KeepWithinStoppingDistanceOfFollowTarget(this.followTarget, myAgent, followDistance * 1.01f));
        }

        public override void Cleanup()
        {
            base.Cleanup();
            if (followCoroutine != null)
                Owner.CoroutineRunner.StopCoroutine(followCoroutine);
            following = false;
        }

        public override void Begin()
        {
            following = true;
            followCoroutine = FollowCoroutine();
            Owner.CoroutineRunner.StartCoroutine(followCoroutine);
        }

        private IEnumerator FollowCoroutine()
        {
            while (following)
            {
                myAgent.SetNavigationTarget(followTarget.Position - ((followTarget.Position - myAgent.Position).normalized * followDistance));
                yield return new WaitForSeconds(.1f);
            }
        }
    }
}
