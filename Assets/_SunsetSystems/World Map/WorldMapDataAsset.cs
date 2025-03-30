using Sirenix.OdinInspector;
using SunsetSystems.Core.Database;
using SunsetSystems.Core.SceneLoading;
using UnityEngine;
using UnityEngine.Localization;

namespace SunsetSystems.WorldMap
{
    [CreateAssetMenu(fileName = "New World Map Asset", menuName = "Sunset World Map/World Map Asset")]
    public class WorldMapDataAsset : AbstractDatabaseEntry<IWorldMapData>, IWorldMapData
    {
        [field: SerializeField, ReadOnly]
        public override string DatabaseID { get; protected set; }
        [field: SerializeField]
        public override string ReadableID { get; protected set; }

        [SerializeField, BoxGroup("Localization")]
        private LocalizedString _localizedName;
        [SerializeField, BoxGroup("Localization")]
        private LocalizedString _localizedDescription;
        [SerializeField, BoxGroup("Localization")]
        private string _fallbackName;
        [SerializeField, MultiLineProperty, BoxGroup("Localization")]
        private string _fallbackDescription;
        [SerializeField]
        private Sprite _areaIcon;

        [field: SerializeField]
        public SceneLoadingDataAsset LevelData { get; private set; }

        public string GetAreaName()
        {
            if (TryGetLocalizedName(out string localized))
                return localized;
            else
                return _fallbackName;
        }

        public string GetDescription()
        {
            if (TryGetLocalizedDescription(out string localized))
                return localized;
            else
                return _fallbackDescription;
        }

        public Sprite GetIcon()
        {
            return _areaIcon;
        }

        private bool TryGetLocalizedName(out string localizedName)
        {
            if (_localizedName.IsEmpty)
            {
                localizedName = "";
                return false;
            }
            else
            {
                localizedName = _localizedName.GetLocalizedString();
                return true;
            }
        }

        private bool TryGetLocalizedDescription(out string localizedDesc)
        {
            if (_localizedDescription.IsEmpty)
            {
                localizedDesc = "";
                return false;
            }
            else
            {
                localizedDesc = _localizedDescription.GetLocalizedString();
                return true;
            }
        }

        #region Database
        protected override void RegisterToDatabase()
        {
            WorldMapEntryDatabase.Instance.Register(this);
        }

        protected override void UnregisterFromDatabase()
        {
            WorldMapEntryDatabase.Instance.Unregister(this);
        }
        #endregion
    }
}
