using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using SunsetSystems.Persistence;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;

namespace SunsetSystems.Utils
{
    public class UMAGroupVisibility : SerializedMonoBehaviour, IPersistentComponent
    {
        [SerializeField]
        private List<DynamicCharacterAvatar> _childrenUMA = new();
        [SerializeField]
        private bool _umaVisible = true;

        public string ComponentID => "UMA_VISIBILITY_GROUP";

        private void OnValidate()
        {
            RefreshUMAs();
        }

        public void RefreshUMAs()
        {
            _childrenUMA = GetComponentsInChildren<DynamicCharacterAvatar>(true).ToList();
        }

        private void Start()
        {
            foreach (var uma in _childrenUMA)
            {
                uma.CharacterCreated.AddListener(UpdateUMAVisibility);
            }
            RefreshUMAVisibility();
        }

        public void SetUMAVisibility(bool visible)
        {
            _umaVisible = visible;
            RefreshUMAVisibility();
        }

        private void RefreshUMAVisibility()
        {
            foreach (var uma in _childrenUMA)
            {
                if (uma.umaData != null && uma.UpdatePending() is false)
                {
                    uma.ToggleHide(!_umaVisible);
                }
                else
                {
                    StartCoroutine(LateRefreshVisibility(uma));
                }
            }

            IEnumerator LateRefreshVisibility(DynamicCharacterAvatar uma)
            {
                yield return new WaitUntil(() => uma.umaData != null && uma.UpdatePending() is false); 
                uma.ToggleHide(!_umaVisible);
            }
        }
        
        private void UpdateUMAVisibility(UMAData umaData)
        {
            if (_umaVisible)
                umaData.Show();
            else
                umaData.Hide();
        }

        public object GetComponentPersistenceData()
        {
            return new UMAGroupVisibilityPersistenceData(this);
        }

        public void InjectComponentPersistenceData(object data)
        {
            if (data is not UMAGroupVisibilityPersistenceData umaGroupData)
                return;
            SetUMAVisibility(umaGroupData.UMAsVisible);
        }

        [SerializeField]
        public class UMAGroupVisibilityPersistenceData 
        {
            public bool UMAsVisible;

            public UMAGroupVisibilityPersistenceData(UMAGroupVisibility umaGroup)
            {
                UMAsVisible = umaGroup._umaVisible;
            }

            public UMAGroupVisibilityPersistenceData()
            {

            }
        }
    }
}
