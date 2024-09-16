﻿using System;
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
        public readonly string SaveFileName => $"{SaveID}.sav";
        public LevelLoadingData LevelLoadingData;
        public ScenePlaylistData PlaylistData;
        public Texture2D SaveScreenShot;
    }
}
