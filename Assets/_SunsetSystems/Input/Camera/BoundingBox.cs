using System.Linq;
using CleverCrow.Fluid.UniqueIds;
using SunsetSystems.Core;
using SunsetSystems.Utils;
using UnityEngine;

namespace SunsetSystems.Input.CameraControl
{
    [RequireComponent(typeof(UniqueId))]
    public class BoundingBox : MonoBehaviour, IUnique
    {
        [SerializeField]
        private Vector3 _boundingBoxSize = new Vector3(1, 1, 1);
        [SerializeField]
        private Vector3 _offset;
        [SerializeField]
        private Color _gizmoColor = Color.red;

        [SerializeField, HideInInspector]
        private UniqueId _uniqueID;

        private Bounds _boxBounds;

        private void Awake()
        {
            _boxBounds = new(transform.position + _offset, _boundingBoxSize);
        }

        private void Start()
        {
            EnsureID();
        }

        private void OnValidate()
        {
            EnsureID();
        }

        private void EnsureID()
        {
            if (_uniqueID == null)
            {
                _uniqueID = GetComponent<UniqueId>();
            }
        }

        private ref Bounds GetBounds() => ref _boxBounds;

        public bool IsPositionWithinBounds(Vector3 position)
        {
            return GetBounds().Contains(position);
        }

        public Vector3 ClampPositionToBounds(Vector3 position)
        {
            return GetBounds().ClosestPoint(position);
        }

        private void OnDrawGizmos()
        {
            Vector3 boundingBoxCenter = transform.position + _offset;
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireCube(boundingBoxCenter, _boundingBoxSize);
        }

        public string GetID()
        {
            return _uniqueID.Id;
        }

        public static BoundingBox FindFirstContainingPoint(Vector3 point)
        {
            return FindObjectsByType<BoundingBox>(FindObjectsSortMode.None).FirstOrDefault(box => box.IsPositionWithinBounds(point));
        }
    }
}
