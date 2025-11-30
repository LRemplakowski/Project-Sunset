using SunsetSystems.Core.Database;
using UnityEngine;

namespace SunsetSystems.Abilities
{
    [CreateAssetMenu(fileName = "New Discipline Database", menuName = "Sunset Abilities/Discipline Database")]
    public class DisciplineDatabase : AbstractDatabase<IDisciplinePower>
    {
        public static DisciplineDatabase Instance
        {
            get
            {
#if UNITY_EDITOR
                return EditorDatabaseHelper.DisciplineDB;
#else
                return DatabaseHolder.Instance.GetDatabase<DisciplineDatabase>();
#endif
            }
        }

#if UNITY_EDITOR
        protected override AbstractDatabase<IDisciplinePower> GetEditorInstance()
        {
            return Instance;
        }

        protected override void SetEditorInstance(AbstractDatabase<IDisciplinePower> instance)
        {
            EditorDatabaseHelper.DisciplineDB = instance as DisciplineDatabase;
        }
#endif
    }
}