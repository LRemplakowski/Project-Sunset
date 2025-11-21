using UnityEngine;

namespace SunsetSystems.MainMenu.UI
{
    public class DotGroup : MonoBehaviour
    {

        [SerializeField]
        protected GameObject dotFullPrefab, dotEmptyPrefab;
        [SerializeField]
        private int _emptyDots;
        internal int EmptyDots
        {
            get => _emptyDots;
            set
            {
                _emptyDots = value;
                isDirty = true;
            }
        }
        [SerializeField]
        private int _fullDots;
        internal int FullDots
        {
            get => _fullDots;
            set
            {
                _fullDots = value;
                isDirty = true;
            }
        }

        protected bool isDirty = false;

        protected virtual void Awake()
        {
            RebuildDotGroup();
        }

        protected virtual void Update()
        {
            if (isDirty)
                RebuildDotGroup();
        }

        internal void RebuildDotGroup()
        {
            ClearDotGroup();
            InitDotGroup();
            isDirty = false;
        }

        [ContextMenu("Rebuild group")]
        protected void ContextRebuildGroup()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            InitDotGroup();
            isDirty = false;
        }

        protected virtual void InitDotGroup()
        {
            for (int i = 0; i < FullDots; i++)
                Instantiate(dotFullPrefab, transform);
            for (int i = 0; i < EmptyDots; i++)
                Instantiate(dotEmptyPrefab, transform);
        }

        protected virtual void ClearDotGroup()
        {
            transform.DestroyChildren();
        }
    }
}
