﻿using SunsetSystems.Entities.Characters;
using SunsetSystems.Resources;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace SunsetSystems.Entities.Interactable
{
    public abstract class InteractableEntity : Entity, IInteractable
    {
        [field:SerializeField]
        public GameObject HoverHighlight { get; set; }

        private bool _isHoveredOver;
        public bool IsHoveredOver
        {
            get => _isHoveredOver;
            set
            {
                _isHoveredOver = value;
                HandleHoverHiglight();
            }
        }

        [SerializeField]
        protected float _interactionDistance = 2.0f;
        public float InteractionDistance
        {
            get => _interactionDistance;
            set => _interactionDistance = value;
        }
        public Creature TargetedBy { get; set; }
        public bool Interacted { get; set; }

        [SerializeField]
        protected Transform _interactionTransform;
        public Transform InteractionTransform
        {
            get => _interactionTransform;
            set => _interactionTransform = value;
        }

        [SerializeField]
        private bool _interactable = true;
        public bool Interactable
        {
            get
            {
                return _interactable;
            }
            set
            {
                _interactable = value;
                this.enabled = value;
            }
        }
        [SerializeField]
        private bool _interactableOnce = false;

        public UnityEvent OnInteractionTriggered;

        protected virtual void OnValidate()
        {
            if (InteractionTransform == null)
            {
                InteractionTransform = this.transform;
            }
        }

        protected virtual void Awake()
        {
            if (InteractionTransform == null)
            {
                InteractionTransform = this.transform;
            }
        }

        protected virtual void Start()
        {
            enabled = Interactable;
        }

        public void Interact()
        {
            if (!Interactable)
                return;
            Debug.Log(TargetedBy + " interacted with object " + gameObject);
            HandleInteraction();
            OnInteractionTriggered?.Invoke();
            Interacted = true;
            TargetedBy = null;
            if (_interactableOnce)
                this.enabled = false;
        }

        protected abstract void HandleInteraction();

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(InteractionTransform.position, _interactionDistance);
        }

        private void HandleHoverHiglight()
        {
            if (HoverHighlight != null)
                HoverHighlight.SetActive(IsHoveredOver);
        }
    }
}
