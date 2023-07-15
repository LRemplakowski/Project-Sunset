using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SunsetSystems.Journal;
using NaughtyAttributes;
using System.Linq;

namespace SunsetSystems.Entities.Interactable
{
    public class InteractableObjectiveTrigger : InteractableEntity, IObjectiveTrigger
    {
        [SerializeReference, Required]
        private Quest _associatedQuest;
        [SerializeField]
        private Objective _objective;
        private bool ObjectiveActive = false;

        protected override void Start()
        {
            base.Start();
            if (_objective != null)
            {
                _objective.OnObjectiveActive -= OnObjectiveActive;
                _objective.OnObjectiveInactive -= OnObjectiveInactive;
                _objective.OnObjectiveActive += OnObjectiveActive;
                _objective.OnObjectiveInactive += OnObjectiveInactive;
            }
        }

        private void OnDestroy()
        {
            if (_objective != null)
            {
                _objective.OnObjectiveActive -= OnObjectiveActive;
                _objective.OnObjectiveInactive -= OnObjectiveInactive;
            }
        }

        private void OnObjectiveActive(Objective objective)
        {
            ObjectiveActive = true;
        }

        private void OnObjectiveInactive(Objective objective)
        {
            ObjectiveActive = false;
        }

        public bool CheckCompletion(Objective objective)
        {
            return ObjectiveActive;
        }

        protected override void HandleInteraction()
        {
            if (CheckCompletion(_objective))
            {
                Debug.Log($"Completed objective {_objective}!");
                _objective.Complete();
            }
            else
            {
                Debug.Log($"Objective {_objective} is not active!");
            }
        }
    }
}
