using SunsetSystems.UMA;
using UnityEngine;

namespace SunsetSystems.Core.Database
{
    [CreateAssetMenu(fileName = "Wardrobe Database", menuName = "UMA/Data/Database")]
    public class UMAWardrobeDatabase : AbstractDatabase<IUMAWardrobeDatabaseItem>
    {
        public static UMAWardrobeDatabase Instance
        {
            get
            {
#if UNITY_EDITOR
                return EditorDatabaseHelper.WardrobeDB;
#else
                return DatabaseHolder.Instance.GetDatabase<UMAWardrobeDatabase>();
#endif
            }
        }

#if UNITY_EDITOR
        protected override AbstractDatabase<IUMAWardrobeDatabaseItem> GetEditorInstance()
        {
            return Instance;
        }

        protected override void SetEditorInstance(AbstractDatabase<IUMAWardrobeDatabaseItem> instance)
        {
            EditorDatabaseHelper.WardrobeDB = instance as UMAWardrobeDatabase;
        }
#endif
    }
}
