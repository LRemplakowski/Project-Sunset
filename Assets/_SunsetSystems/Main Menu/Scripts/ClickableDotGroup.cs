using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace SunsetSystems.MainMenu.UI
{
    public class ClickableDotGroup : DotGroup
    {
        [SerializeField]
        private int maxDots = 5;
        [SerializeField]
        private int minDots = 0;
        [SerializeField]
        private UnassignedDots unassignedDots;
        [SerializeField, Tooltip("Used only when Unassigned Dots is null.")]
        private string unassginedDotsTag;
        [SerializeField]
        private Button removeDotsPrefab;
        [SerializeField, ReadOnly]
        private Button cachedRemoveDotsButton;
        [SerializeField]
        private bool useRemoveDotsButton;

        private void OnEnable()
        {
            if (unassignedDots == null)
                unassignedDots = GameObject.FindGameObjectWithTag(unassginedDotsTag).GetComponent<UnassignedDots>();
            unassignedDots.OnDotGroupDataChanged += this.RemoveDots;
        }

        private void OnDisable()
        {
            unassignedDots.OnDotGroupDataChanged -= this.RemoveDots;
        }

        protected virtual void Start()
        {
            FullDots = minDots;
            EmptyDots = maxDots - minDots;
        }

        public virtual void Initialize()
        {

        }

        public virtual void OnClick(int fullCount)
        {
            if (unassignedDots.HasEnabledDotGroupWithCount(fullCount - minDots) || fullCount == minDots)
            {
                unassignedDots.EnableDotGroup(FullDots - minDots);
            }
            if (unassignedDots.DisableDotGroup(fullCount - minDots) || fullCount == minDots)
            {
                FullDots = fullCount;
                EmptyDots = maxDots - fullCount;
            }
        }

        protected override void InitDotGroup()
        {
            for (int i = 1; i <= FullDots; i++)
            {
                Button dot = Instantiate(dotFullPrefab, transform).GetComponent<Button>();
                int arg = i;
                dot.onClick.AddListener(() => OnClick(arg));
            }

            for (int i = 1; i <= EmptyDots; i++)
            {
                Button dot = Instantiate(dotEmptyPrefab, transform).GetComponent<Button>();
                int arg = i + FullDots;
                dot.onClick.AddListener(() => OnClick(arg));
            }

            if (removeDotsPrefab != null && useRemoveDotsButton && FullDots > 0 && cachedRemoveDotsButton == null)
            {
                cachedRemoveDotsButton = Instantiate(removeDotsPrefab, transform.parent);
                cachedRemoveDotsButton.onClick.AddListener(RemoveDots);
            }
        }

        private void RemoveDots()
        {
            OnClick(minDots);
            if (cachedRemoveDotsButton)
            {
                cachedRemoveDotsButton.onClick.RemoveListener(RemoveDots);
                Destroy(cachedRemoveDotsButton.gameObject);
                cachedRemoveDotsButton = null;
            }
        }

        protected override void ClearDotGroup()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                child.GetComponent<Button>().onClick.RemoveAllListeners();
                Destroy(child.gameObject);
            }
        }
    }
}
