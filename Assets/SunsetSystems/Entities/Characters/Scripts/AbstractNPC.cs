using SunsetSystems.Entities.Characters.Actions;
using UnityEngine;

namespace SunsetSystems.Entities.Characters
{
    public abstract class AbstractNPC : Creature, INameplateReciever
    {
        [SerializeField]
        private Transform _lineTarget;

        public string NameplateText { get => Data.FullName; }
        public Vector3 NameplateWorldPosition => new(transform.position.x, transform.position.y + _nameplateOffset, transform.position.z);
        [SerializeField]
        private float _nameplateOffset = 2.5f;

        public Transform LineTarget
        {
            get => _lineTarget;
            private set => _lineTarget = value;
        }

        public override void Move(Vector3 moveTarget, float stoppingDistance)
        {
            Agent.stoppingDistance = stoppingDistance;
            AddActionToQueue(new Move(this, moveTarget));
        }

        public override void Move(Vector3 moveTarget)
        {
            Move(moveTarget, 0f);
        }

        public override void Move(GridElement moveTarget)
        {
            CurrentGridPosition = moveTarget;
            Move(moveTarget.transform.position);
        }

        public override void Attack(Creature target)
        {
            AddActionToQueue(new Attack(target, this));
        }

        protected override void Start()
        {
            base.Start();
            if (!LineTarget)
                LineTarget = CreateDefaultLineTarget();
        }

        private Transform CreateDefaultLineTarget()
        {
            GameObject lt = new("Default Line Target");
            lt.transform.parent = this.transform;
            lt.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 1.5f, this.transform.position.z);
            return lt.transform;
        }
    }
}
