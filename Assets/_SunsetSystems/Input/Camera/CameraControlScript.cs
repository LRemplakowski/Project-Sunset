using SunsetSystems.Core;
using SunsetSystems.Game;
using SunsetSystems.Persistence;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SunsetSystems.Input.CameraControl
{
    public class CameraControlScript : MonoBehaviour, ISaveable
    {
        public static CameraControlScript Instance { get; private set; }

        [SerializeField]
        private CinemachineCamera mainVirtualCam;
        [SerializeField]
        private LayerMask _groundRaycastMask;
        [SerializeField]
        private float _cameraMoveSpeed = 4;
        [SerializeField]
        private BoundingBox _currentBoundingBox;
        public BoundingBox CurrentBoundingBox { set => _currentBoundingBox = value; }

        //Save/Load variables
        public string DataKey => DataKeyConstants.CAMERA_CONTROL_SCRIPT_DATA_KEY;
        private bool _movedToSavedPosition = false;

        // Runtime
        private float _internalMoveTargetSpeed => _cameraMoveSpeed + 2;
        private Vector3 _moveTarget;
        private Vector3 _moveDirection;
        private float _rotationDirection;
        private Vector2 _zoomDirection;
        private static Vector2 mousePos;
        private static Vector3 mousePosVector;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                ISaveable.RegisterSaveable(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _moveTarget = transform.position;
        }

        private void OnDestroy()
        {
            ISaveable.UnregisterSaveable(this);
        }

        public void MoveToLevelStartPosition()
        {
            if (_movedToSavedPosition is false)
                ForceToPosition(WaypointManager.Instance.GetSceneDefaultEntryWaypoint().transform);
        }

        public void ForceToPosition(Vector3 position) => _moveTarget = position;

        public void ForceToPosition(Transform positionSource) => ForceToPosition(positionSource.position);

        public void ForceRotation(Vector3 eulerAngles)
        {
            Debug.Log("Forcing camera to specified rotation");
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            if (!(context.canceled || context.performed)){
                return;
            }
            Vector2 value = context.ReadValue<Vector2>();
            _moveDirection = new Vector3(value.x, 0, value.y);
            if (GameManager.Instance.CachedGameState == GameState.Dialogue){
                _moveDirection = Vector3.zero;
            }
        }

        private void FixedUpdate()
        {
            _moveTarget += Quaternion.AngleAxis(Camera.main.transform.localRotation.eulerAngles.y, Vector3.up) * _moveDirection * _internalMoveTargetSpeed * Time.fixedDeltaTime;
            Ray ray = new(transform.position + Vector3.up, -Vector3.up);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, _groundRaycastMask, QueryTriggerInteraction.Collide)) {
                _moveTarget = new(_moveTarget.x, hitInfo.point.y, _moveTarget.z);
            }
            if (_currentBoundingBox) {
                _moveTarget = _currentBoundingBox.IsPositionWithinBounds(_moveTarget) ? _moveTarget : _currentBoundingBox.ClampPositionToBounds(_moveTarget);
            }
        }

        public Vector3 GetPosition() {
            mousePos = new Vector2(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue());
            mousePosVector = new Vector3(mousePos.x, 0, mousePos.y);
            Ray ray = Camera.main.ScreenPointToRay(mousePosVector);
            Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, _groundRaycastMask);
            return raycastHit.point;
        }

        private void LateUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, _moveTarget, Time.deltaTime * _cameraMoveSpeed);
        }

        public object GetSaveData()
        {
            CameraSaveData saveData = new()
            {
                CurrentBoundingBox = _currentBoundingBox.GetID(),
                RigPosition = _moveTarget
            };
            return saveData;
        }

        public bool InjectSaveData(object data)
        {
            if (data is not CameraSaveData saveData)
                return false;
            if (string.IsNullOrWhiteSpace(saveData.CurrentBoundingBox))
            {
                Debug.LogWarning($"{nameof(CameraControlScript)} >>> Invalid Bounding Box ID in Save Data! Finding Bounding Box by saved camera position!");
                _currentBoundingBox = BoundingBox.FindFirstContainingPoint(saveData.RigPosition);
            }
            else
            {
                _currentBoundingBox = UniqueUtility.FindFirstWithID<BoundingBox>(saveData.CurrentBoundingBox);
            }
            ForceToPosition(saveData.RigPosition);
            _movedToSavedPosition = true;
            return true;
        }
    }

    public class CameraSaveData : SaveData
    {
        public string CurrentBoundingBox;
        public Vector3 RigPosition;
        public Vector3 CameraRotationTarget;
    }
}
