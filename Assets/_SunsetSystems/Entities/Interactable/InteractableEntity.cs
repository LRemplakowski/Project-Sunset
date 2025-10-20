using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SunsetSystems.Core.Localization;
using SunsetSystems.ActionSystem;
using SunsetSystems.Entities.Interfaces;
using UltEvents;
using UnityEngine;
using SunsetSystems.Dialogue;

namespace SunsetSystems.Entities.Interactable
{
    public class InteractableEntity : PersistentEntity, IInteractable, IHoverNameplateSource, ILocalizationTarget
    {
        public static readonly HashSet<IInteractable> InteractablesInScene = new();

        public GameObject NameplateSource => gameObject;
        public Vector3 NameplateWorldPosition => transform.position + _nameplateOffset;

        [field: Title("References")]
        [field: SerializeField]
        public List<IInteractionHandler> InteractionHandlers { get; set; }
        [SerializeField]
        private Collider _interactionCollider;
        [SerializeField]
        private IHighlightHandler _highlightHandler;
        [SerializeField]
        private GameObject _linkedGameObject;

        [Title("Config")]
        [SerializeField]
        protected float _interactionDistance = 2.0f;
        public float InteractionDistance
        {
            get => _interactionDistance;
            set => _interactionDistance = value;
        }
        public IActionPerformer TargetedBy { get; set; }

        private bool _interacted;
        public bool Interacted
        {
            get
            {
                return _interacted;
            }
            set
            {
                _interacted = value;
            }
        }

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
                return _interactable && (!_interactableOnce || !_interacted) && gameObject.activeSelf;
            }
            set
            {
                _interactable = value;
                if (_interactable)
                    InteractablesInScene.Add(this);
                else
                    InteractablesInScene.Remove(this);
                if (_interactionCollider != null)
                    _interactionCollider.enabled = value;
            }
        }

        [SerializeField]
        private string _nameplateName;
        public string NameplateText => _nameplateName;
        [SerializeField]
        private Vector3 _nameplateOffset = new (0, 3, 0);
        [SerializeField]
        private bool _interactableOnce = false;

        [Title("Events")]
        public UltEvent<bool> OnInteractionTriggered;

        [Title("Runtime")]
        [ShowInInspector]
        private bool _isHoveredOver;
        [ShowInInspector]
        private bool _forceHover = false;

        public bool IsHoveredOver
        {
            get => _isHoveredOver || ForceHover;
            set
            {
                _isHoveredOver = value;
                HandleHoverHiglight();
                HandleNameplate();
            }
        }

        public bool ForceHover
        {
            get
            {
                return _forceHover;
            }

            set
            {
                _forceHover = value;
                HandleHoverHiglight();
                HandleNameplate();
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (InteractionTransform == null)
            {
                InteractionTransform = this.transform;
            }
            if (_interactionCollider == null)
                _interactionCollider = GetComponentInChildren<Collider>();
            if (_interactionCollider != null)
                _interactionCollider.enabled = Interactable;
            if (_references == null)
                _references = GetComponent<IEntityReferences>();
            if (InteractionHandlers == null)
                InteractionHandlers = new();
            if (_highlightHandler == null)
                _highlightHandler = GetComponent<IHighlightHandler>();

        }

        protected virtual void Awake()
        {
            if (InteractionTransform == null)
                InteractionTransform = this.transform;
            if (Interactable)
                InteractablesInScene.Add(this);
            if (_highlightHandler == null)
                _highlightHandler = GetComponent<IHighlightHandler>();

        }

        private void OnEnable()
        {
            if (Interactable)
                InteractablesInScene.Add(this);
            if (_linkedGameObject != null)
                _linkedGameObject.SetActive(true);
            if (_interactionCollider != null)
                _interactionCollider.enabled = Interactable;
            DialogueManager.Instance.OnDialogueStarted.AddListener(OnDialogueStarted);
        }

        private void OnDisable()
        {
            InteractablesInScene.Remove(this);
            if (_linkedGameObject != null)
                _linkedGameObject.SetActive(false);
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.OnDialogueStarted.RemoveListener(OnDialogueStarted);
        }

        protected virtual void Start()
        {
            if (_interactionCollider == null)
                _interactionCollider = GetComponentInChildren<Collider>();
            if (_interactionCollider != null)
                _interactionCollider.enabled = Interactable;
        }

        private void OnDialogueStarted()
        {
            IsHoveredOver = false;
        }

        [Button]
        public virtual void Interact()
        {
            if (!Interactable)
                return;
            IsHoveredOver = false;
            Debug.Log(TargetedBy + " interacted with object " + gameObject);
            Interacted = true;
            if (_interactableOnce)
                this.Interactable = false;
            bool result = false;
            foreach (IInteractionHandler handler in InteractionHandlers)
            {
                if (handler.HandleInteraction(TargetedBy))
                    result = true;
            }
            OnInteractionTriggered?.Invoke(result);
            TargetedBy = null;
        }

        private void HandleHoverHiglight()
        {
            if (_highlightHandler != null && Interactable)
                _highlightHandler.SetHighlightActive(IsHoveredOver);
        }

        private void HandleNameplate()
        {
            IHoverNameplateSource.OnHoverStatusChange?.Invoke(this, Interactable && IsHoveredOver);   
        }

        public void ResetInteraction()
        {
            _interacted = false;
            Interactable = true;
        }

        public void SetLocalizedText(string text)
        {
            _nameplateName = text;
        }

        public override object GetPersistenceData()
        {
            InteractableEntityPersistenceData persistenceData = new(this);
            return persistenceData;
        }

        public override void InjectPersistenceData(object data)
        {
            base.InjectPersistenceData(data);
            if (data is not InteractableEntityPersistenceData interactableData)
                return;
            Interactable = interactableData.Interactable;
            _interacted = interactableData.Interacted;
            _interactableOnce = interactableData.InteractableOnce;
            //InteractionHandlers = new();
            //foreach (var key in interactableData.InteractionHandlers)
            //{
            //    InteractionHandlers.Add(ES3ReferenceMgr.Current.Get(key) as IInteractionHandler);
            //}
            if (_linkedGameObject)
                _linkedGameObject.SetActive(interactableData.GameObjectActive);
        }

        [Serializable]
        public class InteractableEntityPersistenceData : PersistenceData
        {
            public bool Interactable;
            public bool Interacted;
            public bool InteractableOnce;
            public List<long> InteractionHandlers;

            public InteractableEntityPersistenceData(InteractableEntity interactableEntity) : base(interactableEntity)
            {
                Interactable = interactableEntity.Interactable;
                Interacted = interactableEntity.Interacted;
                InteractableOnce = interactableEntity._interactableOnce;
            }

            public InteractableEntityPersistenceData() : base()
            {

            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(InteractionTransform.position, _interactionDistance);
        }
    }
}
