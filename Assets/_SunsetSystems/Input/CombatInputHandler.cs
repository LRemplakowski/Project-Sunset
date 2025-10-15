using System;
using Sirenix.OdinInspector;
using SunsetSystems.Abilities;
using SunsetSystems.Combat;
using SunsetSystems.Combat.Grid;
using SunsetSystems.Game;
using SunsetSystems.Input.CameraControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SunsetSystems.Input
{
    public class CombatInputHandler : SerializedMonoBehaviour, IGameplayInputHandler
    {
        [Title("References")]
        [SerializeField, Required]
        private PlayerCombatActionManager _selectedActionManager;
        [SerializeField]
        private LineRenderer _targetingLineRenderer;
        [SerializeField]
        private IExecutionConfirmationUI _executionUI;
        [Title("Config")]
        [SerializeField]
        private LayerMask _targetableLayerMask;

        private const float RAYCAST_RANGE = 100;
        private Vector2 _pointerPosition;

        private bool _pointerOverGameObject;
        private bool _showTargetingLine;

        private Collider _lastHitCollider;
        private ITargetable _targetableComponent;
        private bool _targetLocked;

        public static event Action<ITargetable> OnTargetingDataUpdate;
        public static event Action<bool> OnTargetLockUpdate;

        private ITargetingContext _targetingContext;

        private void Awake()
        {
            SetTargetingContext(new AbilityTargetingContext(this));
        }

        private void Start()
        {
            SetShowTargetingLine(false);
            UpdateTargetingLineActive();
            _selectedActionManager.Initialize(GetTargetingContext());
        }

        private void Update()
        {
            if (CombatManager.Instance.IsActiveActorPlayerControlled() is false || GameManager.Instance.IsCurrentState(GameState.Combat) is false)
                SetShowTargetingLine(false);
            SetPointerOverGameObject(EventSystem.current.IsPointerOverGameObject());
            UpdateTargetingLineActive();
        }

        private void UpdateTargetingLineActive()
        {
            GetTargetingLineRenderer().enabled = GetShowTargetingLine();
        }

        public void HandlePointerPosition(InputAction.CallbackContext context)
        {
            if (context.performed is false)
                return;
            if (CombatManager.Instance.IsActiveActorPlayerControlled() is false)
                return;
            SetPointerPosition(context.ReadValue<Vector2>());
            Ray ray = Camera.main.ScreenPointToRay(GetPointerPosition());
            if (Physics.Raycast(ray, out var hit, RAYCAST_RANGE, GetRaycastLayerMask()))
            {
                SetLastHitCollider(hit.collider);
            }
            else
            {
                SetLastHitCollider(null);
            }
            if (GetIsTargetLocked())
                return;
            _selectedActionManager.GetSelectedAbility().GetTargetingStrategy().ExecutePointerPosition(GetTargetingContext());
        }

        public void HandlePrimaryAction(InputAction.CallbackContext context)
        {
            if (context.started is false)
                return;
            if (CombatManager.Instance.IsActiveActorPlayerControlled() is false)
                return;
            _selectedActionManager.GetSelectedAbility().GetTargetingStrategy().ExecuteSetTargetLock(GetTargetingContext());
        }

        public void HandleSecondaryAction(InputAction.CallbackContext context)
        {
            if (context.started is false)
                return;
            if (CombatManager.Instance.IsActiveActorPlayerControlled() is false)
                return;
            _selectedActionManager.GetSelectedAbility().GetTargetingStrategy().ExecuteClearTargetLock(GetTargetingContext());
        }

        public void HandleCameraMoveAction(InputAction.CallbackContext context)
        {
            CameraControlScript.Instance.OnMove(context);
        }

        private ITargetingContext GetTargetingContext() => _targetingContext;
        private void SetTargetingContext(ITargetingContext context) => _targetingContext = context;

        private Vector2 GetPointerPosition() => _pointerPosition;
        private void SetPointerPosition(Vector2 position) => _pointerPosition = position;

        private ITargetable GetCurrentTarget() => _targetableComponent;
        private void SetCurrentTarget(ITargetable target)
        {
            _targetableComponent = target;
            OnTargetingDataUpdate?.Invoke(target);
        }

        private bool GetShowTargetingLine() => _showTargetingLine;
        private void SetShowTargetingLine(bool show) => _showTargetingLine = show;

        private LineRenderer GetTargetingLineRenderer() => _targetingLineRenderer;

        private Collider GetLastHitCollider() => _lastHitCollider;
        private void SetLastHitCollider(Collider collider) => _lastHitCollider = collider;

        private bool GetIsTargetLocked() => _targetLocked;
        private void SetIsTargetLocked(bool locked)
        {
            _targetLocked = locked;
            OnTargetLockUpdate?.Invoke(locked);
        }

        private LayerMask GetRaycastLayerMask() => _targetableLayerMask;

        private void SetPointerOverGameObject(bool pointerOverGO)
        {
            _pointerOverGameObject = pointerOverGO;
        }
        private bool GetIsPointerOverGameObject() => _pointerOverGameObject;

        private IExecutionConfirmationUI GetExecutionUI() => _executionUI;

        private class AbilityTargetingContext : ITargetingContext
        {
            private readonly CombatInputHandler _inputHandler;

            public AbilityTargetingContext(CombatInputHandler inputHandler)
            {
                _inputHandler = inputHandler;
            }

            public IAbilityContext GetAbilityContext() => GetSelf().GetContext().AbilityUser.GetCurrentAbilityContext();
            public ICombatant GetSelf() => CombatManager.Instance.CurrentActiveActor;
            public ITargetable GetSelfTarget() => GetSelf() as ITargetable;
            public ICombatContext GetSelfContext() => GetSelf().GetContext();
            public ITargetable GetCurrentTarget() => _inputHandler.GetCurrentTarget();
            public ITargetableContext GetTargetContext() => GetCurrentTarget()?.GetContext();
            public GridManager GetCurrentGrid() => CombatManager.Instance.CurrentEncounter.GridManager;
            public Collider GetLastRaycastCollider() => _inputHandler.GetLastHitCollider();
            public IAbilityConfig GetSelectedAbility() => _inputHandler._selectedActionManager.GetSelectedAbility();
            public LineRenderer GetTargetingLineRenderer() => _inputHandler.GetTargetingLineRenderer();
            public IExecutionConfirmationUI GetExecutionUI() => _inputHandler.GetExecutionUI();
            public AudioSource GetSFXAudioSource() => GetSelf().References.GetCachedComponent<AudioSource>();

            public bool IsPointerOverUI() => _inputHandler.GetIsPointerOverGameObject();
            public bool IsTargetLocked() => _inputHandler.GetIsTargetLocked();
            public bool CanExecuteAbility(IAbilityConfig ability)
            {
                var abilityUser = GetSelf().GetContext().AbilityUser;
                return abilityUser.GetHasValidAbilityContext(ability) && abilityUser.GetCanAffordAbility(ability);
            }

            public Action<bool> TargetLockSetDelegate() => _inputHandler.SetIsTargetLocked;
            public Action<ITargetable> TargetUpdateDelegate() => _inputHandler.SetCurrentTarget;
            public Action<bool> TargetingLineUpdateDelegate() => _inputHandler.SetShowTargetingLine;
        }
    }
}
