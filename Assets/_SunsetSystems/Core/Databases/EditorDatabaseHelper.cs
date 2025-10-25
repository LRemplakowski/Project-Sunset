#if UNITY_EDITOR
using SunsetSystems.Journal;
using SunsetSystems.WorldMap;

namespace SunsetSystems.Core.Database
{
    public static class EditorDatabaseHelper
    {
        public static ItemDatabase ItemDB { get; set; }
        public static CreatureDatabase CreatureDB { get; set; }
        public static QuestDatabase QuestDB { get; set; }
        public static ObjectiveDatabase ObjectiveDB { get; set; }
        public static UMAWardrobeDatabase WardrobeDB { get; set; }
        public static WorldMapEntryDatabase WorldMapDB { get; set; }
        public static AbilityDatabase AbilityDB { get; set; }
    }
}
#endif
