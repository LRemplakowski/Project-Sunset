using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using SunsetSystems.Core.SceneLoading;
using SunsetSystems.Input.CameraControl;
using SunsetSystems.Persistence;
using UltEvents;
using UnityEngine;
using UnityEngine.Serialization;

namespace SunsetSystems.Game
{
    public class GameManager : SerializedMonoBehaviour, ISaveable
    {
        public static GameManager Instance { get; private set; }

        public static event Action<GameState> OnGameStateChanged;

        [field: Title("References")]
        [field: SerializeField]
        public CameraControlScript GameCamera { get; private set; }

        [Title("Runtime")]
        [SerializeField, FormerlySerializedAs("_gameState")]
        private GameState _sceneDefaultState = GameState.Exploration;

        [ShowInInspector, ReadOnly, LabelText("Current Cached State")]
        private GameState _gameState;
        public GameState CachedGameState
        {
            get
            {
                return _gameState;
            }
        }

        [Title("Events")]
        // Called when level is done loading but before injecting persistence data in ISaveable objects
        public UltEvent OnBeforePersistentDataLoad = new();
        // Called when level is done loading & all the persistent data already is injected
        public UltEvent OnLevelStart = new();
        // Called before unloading current level and before caching the persistent data in ISaveable objects
        public UltEvent OnBeforePersistentDataCache = new();
        // Called when we start to unload the current level and all the persistent data has been cached
        public UltEvent OnLevelExit = new();

        public string DataKey => DataKeyConstants.GAME_MANAGER_DATA_KEY;

        private readonly List<IGameStateRequest> _stateRequests = new();

        protected void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            ISaveable.RegisterSaveable(this);
            LevelLoader.OnBeforePersistentDataCache += BeforePersistentDataCache;
            LevelLoader.OnLevelLoadEnd += GameLevelStart;
            LevelLoader.OnLevelLoadStart += GameLevelEnd;
            LevelLoader.OnBeforePersistentDataLoad += BeforePersistentDataLoad;

            _gameState = GetCurrentState();
        }

//        private void Start()
//        {
//#if UNITY_EDITOR
//            BeforePersistentDataLoad();
//            GameLevelStart();
//#endif
//        }

        private void OnDestroy()
        {
            ISaveable.UnregisterSaveable(this);
            LevelLoader.OnBeforePersistentDataCache -= BeforePersistentDataCache;
            LevelLoader.OnLevelLoadEnd -= GameLevelStart;
            LevelLoader.OnLevelLoadStart -= GameLevelEnd;
            LevelLoader.OnBeforePersistentDataLoad -= BeforePersistentDataLoad;
        }

        private void BeforePersistentDataCache()
        {
            OnBeforePersistentDataCache?.InvokeSafe();
        }

        private void BeforePersistentDataLoad()
        {
            OnBeforePersistentDataLoad?.InvokeSafe();
        }

        private void GameLevelStart()
        {
            OnLevelStart?.InvokeSafe();
        }

        private void GameLevelEnd()
        {
            OnLevelExit?.InvokeSafe();
        }

        public string GetLanguage()
        {
            return "EN";
        }

        public void RequestState(IGameStateRequest request)
        {
            ReleaseState(request);
            _stateRequests.Add(request);
            QueueStateUpdate();
        }

        public void ReleaseState(IGameStateRequest request)
        {
            _stateRequests.RemoveAll(existing => existing.SourceID == request.SourceID);
            QueueStateUpdate();
        }

        private void QueueStateUpdate()
        {
            _gameState = GetCurrentState();
            OnGameStateChanged?.Invoke(_gameState);
        }

        private GameState GetCurrentState()
        {
            if (_stateRequests.Count > 0)
            {
                return _stateRequests.Last().State;
            }
            else
            {
                return _sceneDefaultState;
            }
        }

        public bool IsCurrentState(GameState state)
        {
            return CachedGameState.Equals(state);
        }

        public object GetSaveData()
        {
            GameManagerSaveData saveData = new()
            {
                CurrentState = CachedGameState
            };
            return saveData;
        }

        public bool InjectSaveData(object data)
        {
            if (data is not GameManagerSaveData savedData)
                return false;
            _gameState = savedData.CurrentState;
            return true;
        }

        private class GameManagerSaveData : SaveData
        {
            public GameState CurrentState;
        }
    }

    public enum GameState
    {
        Exploration,
        Combat,
        Dialogue,
        MainMenu,
        GamePaused, 
        WorldMap
    }

    public interface IGameStateRequest
    {
        public string SourceID { get; }
        public GameState State { get; }
    }

    public class StateChangeRequest : IGameStateRequest
    {
        public string SourceID { get; }

        public GameState State { get; }

        public StateChangeRequest(string sourceID, GameState state)
        {
            SourceID = sourceID;
            State = state;
        }
    }
}
