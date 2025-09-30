using Sirenix.OdinInspector;
using SunsetSystems.Core.Database;
using UMA.CharacterSystem;
using UnityEngine;

namespace SunsetSystems.UMA
{
    [CreateAssetMenu(fileName = "Wardrobe Data", menuName = "UMA/Base Look Wardrobe")]
    public class WardrobeDatabaseItem : AbstractDatabaseEntry<IUMAWardrobeDatabaseItem>, IUMAWardrobeDatabaseItem
    {
        [field: SerializeField, ReadOnly]
        public override string DatabaseID { get; protected set; }
        [field: SerializeField]
        public override string ReadableID { get; protected set; }

        [field: SerializeField]
        public UMAWardrobeCollection Data { get; private set; }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (Data == null)
            {
                Data = CreateInstance<UMAWardrobeCollection>();
                Data.name = $"{name} Wardrobe Data";
            }
            if (UnityEditor.AssetDatabase.IsSubAsset(Data) is false)
            {
                if (UnityEditor.AssetDatabase.IsMainAsset(Data))
                {
                    var tempData = Instantiate(Data);
                    UnityEditor.AssetDatabase.DeleteAsset(UnityEditor.AssetDatabase.GetAssetPath(Data));
                    Data = tempData;
                    Data.name = $"{name} Wardrobe Data";
                }
                UnityEditor.AssetDatabase.AddObjectToAsset(Data, this);  //Database is also a ScriptableObject asset.
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }
        }
#endif

        protected override void RegisterToDatabase()
        {
            UMAWardrobeDatabase.Instance.Register(this);
        }

        protected override void UnregisterFromDatabase()
        {
            UMAWardrobeDatabase.Instance.Unregister(this);
        }
    }
}
