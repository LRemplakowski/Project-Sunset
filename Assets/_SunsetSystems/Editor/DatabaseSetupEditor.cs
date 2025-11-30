using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using SunsetSystems.Abilities;
using SunsetSystems.Core.Database;
using SunsetSystems.Journal;
using SunsetSystems.WorldMap;
using UnityEditor;
using UnityEngine;

namespace SunsetSystems.Editor
{
    public class DatabaseSetupEditor : OdinEditorWindow
    {
        static DatabaseSetupEditor()
        {
            var window = GetWindow<DatabaseSetupEditor>();
            window.LoadDatabaseReferences();
            window.UpdateStaticReferences();
            window.Close();
        }

        [AssetsOnly]
        public ItemDatabase ItemDatabase;
        [AssetsOnly]
        public CreatureDatabase CreatureDatabase;
        [AssetsOnly]
        public QuestDatabase QuestDatabase;
        [AssetsOnly]
        public ObjectiveDatabase ObjectiveDatabase;
        [AssetsOnly]
        public UMAWardrobeDatabase WardrobeDatabase;
        [AssetsOnly]
        public WorldMapEntryDatabase WorldMapDatabase;
        [AssetsOnly]
        public AbilityDatabase AbilityDatabase;
        [AssetsOnly]
        public DisciplineDatabase DisciplineDatabase;

        [MenuItem("Tools/Database Setup")]
        public static void OpenWindow()
        {
            var window = GetWindow<DatabaseSetupEditor>();
            window.OnClose += window.SaveChanges;
            window.Show();
        }

        public override void SaveChanges()
        {
            base.SaveChanges();
            SaveDatabaseReferences();
            UpdateStaticReferences();
        }

        protected override void Initialize()
        {
            base.Initialize();
            LoadDatabaseReferences();
            UpdateStaticReferences();
        }

        private void LoadDatabaseReferences()
        {
            string itemPath = EditorPrefs.GetString("ItemDatabase");
            ItemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabase>(itemPath);
            string creaturePath = EditorPrefs.GetString("CreatureDatabase");
            CreatureDatabase = AssetDatabase.LoadAssetAtPath<CreatureDatabase>(creaturePath);
            string questPath = EditorPrefs.GetString("QuestDatabase");
            QuestDatabase = AssetDatabase.LoadAssetAtPath<QuestDatabase>(questPath);
            string objectivePath = EditorPrefs.GetString("ObjectiveDatabase");
            ObjectiveDatabase = AssetDatabase.LoadAssetAtPath<ObjectiveDatabase>(objectivePath);
            string wardrobePath = EditorPrefs.GetString("WardrobeDatabase");
            WardrobeDatabase = AssetDatabase.LoadAssetAtPath<UMAWardrobeDatabase>(wardrobePath);
            string worldMapPath = EditorPrefs.GetString("WorldMapDatabase");
            WorldMapDatabase = AssetDatabase.LoadAssetAtPath<WorldMapEntryDatabase>(worldMapPath);
            string abilityPath = EditorPrefs.GetString("AbilityDatabase");
            AbilityDatabase = AssetDatabase.LoadAssetAtPath<AbilityDatabase>(abilityPath);
            string disciplinePath = EditorPrefs.GetString("DisciplineDatabase");
            DisciplineDatabase = AssetDatabase.LoadAssetAtPath<DisciplineDatabase>(disciplinePath);
        }

        private void SaveDatabaseReferences()
        {
            string itemPath = AssetDatabase.GetAssetPath(ItemDatabase);
            EditorPrefs.SetString("ItemDatabase", itemPath);
            string creatureParth = AssetDatabase.GetAssetPath(CreatureDatabase);
            EditorPrefs.SetString("CreatureDatabase", creatureParth);
            string questPath = AssetDatabase.GetAssetPath(QuestDatabase);
            EditorPrefs.SetString("QuestDatabase", questPath);
            string objectivePath = AssetDatabase.GetAssetPath(ObjectiveDatabase);
            EditorPrefs.SetString("ObjectiveDatabase", objectivePath);
            string wardrobePath = AssetDatabase.GetAssetPath(WardrobeDatabase);
            EditorPrefs.SetString("WardrobeDatabase", wardrobePath);
            string worldMapPath = AssetDatabase.GetAssetPath(WorldMapDatabase);
            EditorPrefs.SetString("WorldMapDatabase", worldMapPath);
            string abilityPath = AssetDatabase.GetAssetPath(AbilityDatabase);
            EditorPrefs.SetString("AbilityDatabase", abilityPath);
            string disciplinePath = AssetDatabase.GetAssetPath(DisciplineDatabase);
            EditorPrefs.SetString("DisciplineDatabase", disciplinePath);
        }

        private void UpdateStaticReferences()
        {
            Debug.Log("Editor Database references updated!");
            EditorDatabaseHelper.ItemDB = ItemDatabase;
            EditorDatabaseHelper.CreatureDB = CreatureDatabase;
            EditorDatabaseHelper.QuestDB = QuestDatabase;
            EditorDatabaseHelper.ObjectiveDB = ObjectiveDatabase;
            EditorDatabaseHelper.WardrobeDB = WardrobeDatabase;
            EditorDatabaseHelper.WorldMapDB = WorldMapDatabase;
            EditorDatabaseHelper.AbilityDB = AbilityDatabase;
            EditorDatabaseHelper.DisciplineDB = DisciplineDatabase;
        }
    }
}
