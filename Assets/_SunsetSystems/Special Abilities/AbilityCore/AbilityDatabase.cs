using SunsetSystems.Abilities;
using SunsetSystems.Core.Database;
using SunsetSystems.Entities.Characters;
using UnityEngine;

namespace SunsetSystems
{
    [CreateAssetMenu(fileName = "New Ability Database", menuName = "Sunset Abilities/Ability Database")]
    public class AbilityDatabase : AbstractDatabase<IAbilityConfig>
    {
        public static AbilityDatabase Instance
        {
            get
            {
#if UNITY_EDITOR
                return EditorDatabaseHelper.AbilityDB;
#else
                return DatabaseHolder.Instance.GetDatabase<AbilityDatabase>();
#endif
            }
        }

#if UNITY_EDITOR
        protected override AbstractDatabase<IAbilityConfig> GetEditorInstance()
        {
            return Instance;
        }

        protected override void SetEditorInstance(AbstractDatabase<IAbilityConfig> instance)
        {
            EditorDatabaseHelper.AbilityDB = instance as AbilityDatabase;
        }
#endif
    }
}
