using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace SunsetSystems.ActionSystem
{
    [System.Serializable]
    public abstract class EntityAction
    {
        /// <summary>
        /// Priority actions clear action queue upon assignment.
        /// </summary>
        [ShowInInspector]
        private string name => this.ToString();
        [field: SerializeField]
        public bool IsPriority { get; protected set; }
        [field: SerializeField]
        public bool ActionFinished { get; protected set; } = false;
        [field: SerializeField]
        public bool ActionCanceled { get; protected set; } = false;
        [field: OdinSerialize]
        protected IActionPerformer Owner { get; private set; }
        [SerializeField]
        protected List<Condition> conditions = new List<Condition>();

        public EntityAction(IActionPerformer owner)
        {
            Owner = owner;
            IsPriority = false;
        }

        public EntityAction(IActionPerformer owner, bool isPriority) : this(owner)
        {
            IsPriority = isPriority;
        }

        /// <summary>
        /// Start executing action logic.
        /// </summary>
        public abstract void Begin();

        /// <summary>
        /// Do any relevant cleanup.
        /// </summary>
        public virtual void Cleanup()
        {
            conditions.Clear();
        }

        public virtual void Abort()
        {
            ActionCanceled = true;
        }

        public virtual bool EvaluateAction()
        {
            if (conditions.Count == 0)
            {
                Debug.LogError("Aborting action, no conditions present");
                Abort();
            }
            else
            {
                ActionFinished = conditions.All(c => c.IsMet());
            }
            if (ActionFinished || ActionCanceled)
                Cleanup();
            return ActionFinished || ActionCanceled;
        }

        public override string ToString()
        {
            string action = "EntityAction: " + this.GetType() + "\nConditions:";
            foreach (Condition c in conditions)
            {
                action += "\n" + c;
            }
            return action;
        }
    } 
}
