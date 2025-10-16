using System.Collections;
using UnityEngine;

namespace SunsetSystems.ActionSystem
{
    [System.Serializable]
    public class FaceTarget : EntityAction
    {
        private const float MARGIN_OF_ERROR = 0.01f;

        [SerializeField]
        private Transform ownerTransform;
        [SerializeField]
        private float rotationSpeed;
        [SerializeField]
        private Vector3 lookDirection;

        private IEnumerator lookAtRoutine;

        public FaceTarget(IActionPerformer owner, Transform lookAtTarget, float rotationSpeed = 90f) : this(owner, (lookAtTarget.position - owner.Transform.position).normalized, rotationSpeed)
        {

        }

        public FaceTarget(IActionPerformer owner, Vector3 lookDirection, float rotationSpeed = 90f) : base(owner)
        {
            ownerTransform = owner.Transform;
            this.lookDirection = lookDirection;
            this.rotationSpeed = rotationSpeed;
            conditions.Add(new FaceTargetCondition(ownerTransform, lookDirection, MARGIN_OF_ERROR));
        }

        public override void Cleanup()
        {
            base.Cleanup();
            if (lookAtRoutine != null)
                (Owner as MonoBehaviour).StopCoroutine(lookAtRoutine);
        }

        public override void Begin()
        {
            lookAtRoutine = LookAtTargetOverTime();
            (Owner as MonoBehaviour).StartCoroutine(lookAtRoutine);
        }

        private IEnumerator LookAtTargetOverTime()
        {
            if (lookDirection == Vector3.zero)
            {
                Abort();
                yield break;
            }
            //Owner.References.NavigationManager.FaceDirectionAfterMovementFinished(Owner.Transform.position + lookDirection);
            float dotProduct = Vector3.Dot(ownerTransform.forward, lookDirection);
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            while (dotProduct < 1f - MARGIN_OF_ERROR)
            {
                ownerTransform.rotation = Quaternion.RotateTowards(ownerTransform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
                yield return null;
            }
            lookAtRoutine = null;
        }
    }
}
