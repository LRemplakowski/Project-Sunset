using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SunsetSystems.Abilities;
using SunsetSystems.Core.Database;
using SunsetSystems.Entities.Data;
using SunsetSystems.Equipment;
using SunsetSystems.UMA;
using UnityEngine;

namespace SunsetSystems.Entities.Characters
{
    [CreateAssetMenu(fileName = "New Creature Config", menuName = "Character/Creature Config")]
    public class CreatureConfig : AbstractDatabaseEntry<CreatureConfig>, ICreatureTemplateProvider
    {
        [field: SerializeField, ReadOnly]
        public override string DatabaseID { get; protected set; } = "";
        [field: SerializeField]
        public override string ReadableID { get; protected set; }
        [SerializeField]
        private string _name = "New";
        public string FirstName { get => _name; }
        [SerializeField]
        private string _lastName = "Creature";
        public string LastName { get => _lastName; }
        public string FullName { get => $"{FirstName} {LastName}".Trim(); }
        [field: SerializeField]
        public Sprite Portrait { get; private set; }
        [SerializeField]
        private StatsConfig _statsConfig;
        public StatsData StatsData => new(_statsConfig);
        [SerializeField]
        private List<IDisciplinePower> _startingPowers = new();
        public IReadOnlyCollection<IDisciplinePower> StartingPowers => _startingPowers;
        [field: SerializeField]
        public InventoryConfig EquipmentConfig { get; private set; }
        [field: SerializeField]
        public IUMAWardrobeDatabaseItem BaseLookWardrobeCollection { get; private set; }
        [SerializeField]
        private Faction _creatureFaction;
        public Faction Faction { get => _creatureFaction; }
        [SerializeField]
        private BodyType _bodyType;
        public BodyType BodyType { get => _bodyType; }
        [SerializeField]
        private CreatureType _creatureType;
        public CreatureType CreatureType { get => _creatureType; }
        [field: SerializeField, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout, IsReadOnly = true)]
        public Dictionary<EquipmentSlotID, IEquipmentSlot> EquipmentSlotsData { get; private set; }

        public ICreatureTemplate CreatureTemplate => new TemplateFromCreatureAsset(this);

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (EquipmentSlotsData == null || EquipmentSlotsData.Count < Enum.GetValues(typeof(EquipmentSlotID)).Length - 1)
            {
                EquipmentSlotsData = new()
                {
                    { EquipmentSlotID.PrimaryWeapon, new EquipmentSlot(EquipmentSlotID.PrimaryWeapon) },
                    { EquipmentSlotID.SecondaryWeapon, new EquipmentSlot(EquipmentSlotID.SecondaryWeapon) },
                    { EquipmentSlotID.Chest, new EquipmentSlot(EquipmentSlotID.Chest) },
                    { EquipmentSlotID.Boots, new EquipmentSlot(EquipmentSlotID.Boots) },
                    { EquipmentSlotID.Hands, new EquipmentSlot(EquipmentSlotID.Hands) },
                    { EquipmentSlotID.Trinket, new EquipmentSlot(EquipmentSlotID.Trinket) }
                };
            }
        }
#endif

        protected override void RegisterToDatabase()
        {
            CreatureDatabase.Instance?.Register(this);
        }

        protected override void UnregisterFromDatabase()
        {
            CreatureDatabase.Instance?.Unregister(this);
        }

        [Serializable]
        public class TemplateFromCreatureAsset : ICreatureTemplate
        {
            public string DatabaseID { get; private set; }

            public string ReadableID { get; private set; }

            public string FullName => $"{FirstName} {LastName}".Trim();

            public string FirstName { get; private set; }

            public string LastName { get; private set; }

            public Faction Faction { get; private set; }

            public BodyType BodyType { get; private set; }

            public CreatureType CreatureType { get; private set; }

            public string BaseLookWardrobeReadableID { get; private set; }

            public Dictionary<EquipmentSlotID, string> EquipmentSlotsData { get; private set; }

            public StatsData StatsData { get; private set; }

            public IReadOnlyCollection<IDisciplinePower> KnownPowers { get; private set; }

            public TemplateFromCreatureAsset(CreatureConfig asset)
            {
                this.DatabaseID = asset.DatabaseID;
                this.ReadableID = asset.ReadableID;
                this.FirstName = asset.FirstName;
                this.LastName = asset.LastName;
                this.Faction = asset.Faction;
                this.BodyType = asset.BodyType;
                this.CreatureType = asset.CreatureType;
                if (asset.BaseLookWardrobeCollection != null)
                    this.BaseLookWardrobeReadableID = asset.BaseLookWardrobeCollection.ReadableID;
                this.EquipmentSlotsData = new();
                foreach (var item in asset.EquipmentSlotsData)
                {
                    if (item.Value.GetEquippedItem() == null)
                        this.EquipmentSlotsData[item.Key] = item.Value.DefaultItemID;
                    else
                        this.EquipmentSlotsData[item.Key] = item.Value.GetEquippedItem().ReadableID;
                }
                this.StatsData = new(asset.StatsData);
                this.KnownPowers = asset.StartingPowers;
            }

            public TemplateFromCreatureAsset()
            {

            }
        }
    }
}
