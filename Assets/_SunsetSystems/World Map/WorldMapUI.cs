using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SunsetSystems.WorldMap
{
    public class WorldMapUI : SerializedMonoBehaviour
    {
        [Title("References")]
        [SerializeField, Required]
        private IWorldMapManager _worldMapManager;
        [SerializeField]
        private Transform _mapTokenParent;
        [SerializeField, Required]
        private CanvasGroup _mapCanvasGroup;
        [SerializeField, Required]
        private AreaDescription _areaDescriptionWindow;
        [SerializeField, Required]
        private WorldMapTravelConfirmation _areaConfirmatonScreen;

        [Title("Data")]
        [SerializeField]
        private List<IWorldMapToken> _mapTokens = new();

        [Title("Runtime")]
        [ShowInInspector]
        private IWorldMapData _selectedMap;
        [ShowInInspector]
        private readonly Dictionary<string, IWorldMapToken> _idTokenDictionary = new();

        [Title("Editor Utility")]
        [SerializeField]
        private bool _autoCacheTokensInParent = true;

        private void OnValidate()
        {
            EnsureTokenParent();
            if (_autoCacheTokensInParent)
                CacheMapTokens();
            else
                _mapTokens = _mapTokens.Distinct().ToList();
        }

        [Button]
        private void CacheMapTokens()
        {
            _mapTokens = _mapTokenParent.GetComponentsInChildren<IWorldMapToken>(true).ToList();
        }

        private void EnsureTokenParent() => _mapTokenParent = _mapTokenParent != null ? _mapTokenParent : transform;

        private void Awake()
        {
            EnsureTokenParent();
            _mapTokens.ForEach(token => _idTokenDictionary[token.GetData().DatabaseID] = token);
        }

        private void Start()
        {
            _mapTokens.ForEach(token => token.InjectTokenManager(this));
        }

        private void OnEnable()
        {
            var unlockedMaps = _worldMapManager.GetUnlockedMaps();
            _mapTokens.ForEach(token => token.SetVisible(false));
            foreach (IWorldMapData map in unlockedMaps)
            {
                if (_idTokenDictionary.TryGetValue(map.DatabaseID, out IWorldMapToken token))
                    token.SetVisible(true);
            }
        }

        public void ShowAreaDescription(IWorldMapData tokenData)
        {
            _selectedMap = tokenData;
            _ = _areaDescriptionWindow.ShowDescription(tokenData);
        }

        public void ToogleTravelConfirmationPopup(bool show)
        {
            if (show)
                _areaConfirmatonScreen.ShowConfirmationWindow();
            else
                _areaConfirmatonScreen.HideConfirmationWindow();
        }

        public void ConfirmTravelToSelectedArea()
        {
            _worldMapManager.TravelToMap(_selectedMap);
        }
    }
}
