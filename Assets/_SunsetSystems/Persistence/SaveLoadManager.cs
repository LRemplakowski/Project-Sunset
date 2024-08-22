using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SunsetSystems.Core.SceneLoading;
using UnityEngine;

namespace SunsetSystems.Persistence
{
    
    public class SaveLoadManager : SerializedMonoBehaviour
    {
        private static SaveLoadManager _instance;

        private const string SAVE_PATH = "Saves/";
        private const string META_DATA = "SAVE_META";
        private const string GAME_DATA = "SAVE_GAME";

        [ShowInInspector, ReadOnly]
        private GlobalPersistenceData _gameData = new();
        private static GlobalPersistenceData GameData => _instance._gameData;

        [RuntimeInitializeOnLoadMethod]
        public static void InitializeSaveSystem()
        {
            if (_instance != null)
                return;
            GameObject saveLoadGO = new("Save System Manager");
            DontDestroyOnLoad(saveLoadGO);
            _instance = saveLoadGO.AddComponent<SaveLoadManager>();
        }

        public static void ForceCreateNewSaveData()
        {
            _instance._gameData = new();
        }

        public static void CreateNewSaveFile(string saveName)
        {
            string date = $"{DateTime.Now:yyyy-M-dd--HH-mm-ss}";
            string saveID = Guid.NewGuid().ToString();
            string filename = SaveIDToFilePath(saveID);
            SaveMetaData metaData = new()
            {
                SaveName = saveName,
                SaveID = saveID,
                SaveDate = date,
                LevelLoadingData = LevelLoader.Instance.CurrentLoadedLevel,
            };
            UpdateRuntimeDataCache();
            ES3.Save(META_DATA, metaData, filename);
            ES3.Save(GAME_DATA, GameData, filename);
        }

        public static void UpdateRuntimeDataCache()
        {
            foreach (ISaveable saveable in ISaveable.Saveables)
            {
                GameData.UpdateSaveData(saveable);
            }
        }

        public static void LoadSavedDataIntoRuntime(string saveID)
        {
            //_gameData.ClearSaveData();
            ES3.LoadInto(GAME_DATA, SaveIDToFilePath(saveID), GameData);
        }

        public static IEnumerable<SaveMetaData> GetAllSaveMetaData()
        {
            List<SaveMetaData> result = new();
            string[] saveFiles = null;
            if (ES3.DirectoryExists(SAVE_PATH))
                saveFiles = ES3.GetFiles(SAVE_PATH);
            if (saveFiles == null)
                return result;
            foreach (var saveFile in saveFiles)
            {
                try
                {
                    result.Add(ES3.Load<SaveMetaData>(META_DATA, SAVE_PATH + saveFile));
                }
                catch (FormatException exception)
                {
                    UnityEngine.Debug.LogError($"Exception occured while loading a save file! File {saveFile} may be corrupted!");
                    UnityEngine.Debug.LogException(exception);
                    continue;
                }
            }
            return result;
        }

        public static SaveMetaData GetSaveMetaData(string saveID)
        {
            var metaData = ES3.Load<SaveMetaData>(META_DATA, SaveIDToFilePath(saveID));
            return metaData;
        }

        public static void InjectRuntimeDataIntoSaveables()
        {
            foreach (ISaveable saveable in ISaveable.Saveables)
            {
                try
                {
                    if (GameData.TryGetData(saveable.DataKey, out object data))
                        saveable.InjectSaveData(data);
                    else
                        UnityEngine.Debug.Log($"There is no saved data for object {saveable}!");
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogException(exception, saveable as UnityEngine.Object);
                }
                finally
                {
                    UnityEngine.Debug.Log($"Data injection into {saveable} successful!");
                }
            }
        }

        public static LevelLoadingData GetSavedLevelAsset(string saveID)
        {
            var metaData = ES3.Load<SaveMetaData>(META_DATA, SaveIDToFilePath(saveID));
            if (metaData.SaveID == saveID)
                return metaData.LevelLoadingData;
            return new();
        }

        public static void DeleteSaveFile(string saveID)
        {
            string saveFileName = SaveIDToFilePath(saveID);
            ES3.DeleteFile(saveFileName);
        }

        private static string SaveIDToFilePath(string saveID)
        {
            return $"{SAVE_PATH}{saveID}.sav";
        }

        private static string SaveFileNameToSaveID(string fileName)
        {
            return fileName.Split('.')[0];
        }

        public static bool HasExistingSaves()
        {
            bool result = false;
            if (ES3.DirectoryExists(SAVE_PATH))
            {
                result = ES3.GetFiles(SAVE_PATH).Length > 0;
            }
            return result;
        }
    }
}
