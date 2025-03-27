using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using SunsetSystems.Core.SceneLoading;
using SunsetSystems.Core.TimeFlow;
using SunsetSystems.Persistence;
using UnityEngine;

namespace SunsetSystems.WorldMap
{
    public class WorldMapManager : SerializedMonoBehaviour, IWorldMapManager, ISaveable
    {
        [Title("References")]
        [SerializeField]
        private IWorldMapData _currentPlayerHaven;
        [SerializeField]
        private List<IWorldMapData> _defaultUnlockedMaps = new();

        [Title("Runtime")]
        [SerializeField]
        private IWorldMapData _currentTraveledToMap;
        [ShowInInspector, ReadOnly]
        private IWorldMapData _currentSelectedMap;
        [ShowInInspector, ReadOnly]
        private HashSet<IWorldMapData> _unlockedMaps = new();

        public string DataKey => DataKeyConstants.WORLD_MAP_MANAGER_DATA_KEY;

        public static event Action<IWorldMapData> OnCurrentMapChanged;
        public static event Action OnUnlockedMapsUpdated;

        private void Awake()
        {
            ISaveable.RegisterSaveable(this);
            _unlockedMaps.AddRange(_defaultUnlockedMaps);
        }

        private void OnDestroy()
        {
            ISaveable.UnregisterSaveable(this);
        }

        public void TravelToSelectedMap()
        {
            TravelTimeData travelData = GetTravelTime(GetCurrentMap(), GetSelectedMap());
            TimeManager.Instance.AddTimeSpan(travelData.TravelDuration);
            _currentTraveledToMap = GetSelectedMap();
            OnCurrentMapChanged?.Invoke(GetSelectedMap());
        }

        public bool EnterCurrentMap()
        {
            if (_currentTraveledToMap == null)
                return false;
            _ = LevelLoader.Instance.LoadNewScene(_currentTraveledToMap.LevelData);
            return true;
        }

        public void SetMapUnlocked(IWorldMapData mapData, bool unlocked)
        {
            if (mapData == null)
            {
                Debug.LogError("World Map Manager was instructed to unlock NULL map! Aborting...");
                return;
            }

            if (unlocked)
                _unlockedMaps.Add(mapData);
            else
                _unlockedMaps.Remove(mapData);
            OnUnlockedMapsUpdated?.Invoke();
        }

        public void SetSelectedMap(IWorldMapData mapData) => _currentSelectedMap = mapData;

        public IEnumerable<IWorldMapData> GetUnlockedMaps() => _unlockedMaps.AsEnumerable();

        public bool IsMapUnlocked(IWorldMapData mapData)
        {
            return _unlockedMaps.Contains(mapData) || _unlockedMaps.Any(map => map.DatabaseID == mapData.DatabaseID);
        }

        public bool IsCurrentMap(IWorldMapData mapData)
        {
            if (mapData != null && _currentTraveledToMap != null)
                return mapData.DatabaseID == _currentTraveledToMap.DatabaseID;
            return false;
        }

        public IWorldMapData GetCurrentMap() => _currentTraveledToMap;

        public void SetCurrentMap(WorldMapDataAsset map) => _currentTraveledToMap = map;

        public IWorldMapData GetPlayerHavenMap() => _currentPlayerHaven;

        public IWorldMapData GetSelectedMap() => _currentSelectedMap;

        public TravelTimeData GetTravelTime(IWorldMapData start, IWorldMapData end)
        {
            WorldMapTravelTimeCalculator.CalculateTravelTime(new(this, start, end), out var travelData);
            return travelData;
        }

        public float GetDistanceBetweenAreas(IWorldMapData start, IWorldMapData end)
        {
            return 1f;
        }

        public float GetPlayerTravelSpeed()
        {
            return 1f;
        }

        public object GetSaveData()
        {
            return new WorldMapSaveData(this);
        }

        public bool InjectSaveData(object data)
        {
            if (data is not WorldMapSaveData mapData)
                return false;
            _unlockedMaps = new();
            foreach (var mapID in mapData.UnlockedMapsIDs)
            {
                if (WorldMapEntryDatabase.Instance.TryGetEntry(mapID, out IWorldMapData entry))
                    _unlockedMaps.Add(entry);
            }
            WorldMapEntryDatabase.Instance.TryGetEntry(mapData.LastTraveledToMap, out _currentTraveledToMap);
            OnUnlockedMapsUpdated?.Invoke();
            return true;
        }

        [Serializable]
        public class WorldMapSaveData : SaveData
        {
            public string LastTraveledToMap;
            public List<string> UnlockedMapsIDs;

            public WorldMapSaveData(WorldMapManager manager) : base()
            {
                LastTraveledToMap = manager._currentTraveledToMap?.DatabaseID ?? "";
                UnlockedMapsIDs = manager.GetUnlockedMaps().Select(map => map.DatabaseID).ToList();
            }
        }
    }
}
