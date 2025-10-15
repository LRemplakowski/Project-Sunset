using System;
using SunsetSystems.Audio;
using SunsetSystems.Core.SceneLoading;
using UnityEngine;

namespace SunsetSystems.Persistence
{
    [Serializable]
    public struct SaveMetaData
    {
        public string SaveID;
        public string SaveName;
        public string SaveDate;
        public string ActiveQuestName;
        public double PlayTime;
        public readonly string SaveFileName => $"{SaveID}.sav";
        public LevelLoadingData LevelLoadingData;
        public ScenePlaylistData PlaylistData;
        public Texture2D SaveScreenShot;

        public readonly bool IsValid() => !string.IsNullOrEmpty(SaveID);
    }
}
