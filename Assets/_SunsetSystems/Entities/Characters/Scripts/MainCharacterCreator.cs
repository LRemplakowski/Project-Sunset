using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using SunsetSystems.Entities.Data;
using SunsetSystems.Equipment;
using SunsetSystems.MainMenu.UI;
using UnityEngine;

namespace SunsetSystems.Entities.Characters
{
    public class MainCharacterCreator : SerializedMonoBehaviour, ICreatureTemplateProvider
    {
        [SerializeField]
        private CreatureConfig _templateAsset;
        [SerializeField]
        private Transform _dotGroupsParent;

        private MainCharacterTemplate _template;
        private ClickableDotGroup[] _skillDotGroups;

        public ICreatureTemplate CreatureTemplate => GetTemplate();

        private void OnEnable()
        {
            ResetTemplate();
        }

        private void Start()
        {
            _skillDotGroups = _dotGroupsParent.GetComponentsInChildren<ClickableDotGroup>(true);
            ResetTemplate();
        }

        public void ResetTemplate()
        {
            InitializeTemplate();
            _skillDotGroups?.ForEach(group => group.Initialize());
        }

        private ICreatureTemplate GetTemplate()
        {
            if (_template == null)
                InitializeTemplate();
            return _template;
        }

        public void SetAttribute(AttributeType attribute, int value)
        {
            if (_template == null) return;

            var statsData = _template.StatsData;
            var existing = statsData.Attributes.GetAttribute(attribute);
            existing.SetValue(value);
            _template.StatsData = statsData;
        }

        public void SetSkill(SkillType skill, int value)
        {
            if (_template == null) return;

            var statsData = _template.StatsData;
            var existing = statsData.Skills.GetSkill(skill);
            existing.SetValue(value);
            _template.StatsData = statsData;
        }

        public int GetSkillValue(SkillType skill)
        {
            if (_template == null) return 0;

            var existing = _template.StatsData.Skills.GetSkill(skill);
            return existing.GetValue();
        }

        public int GetAttributeValue(AttributeType attribute)
        {
            if (_template == null) return 0;

            var existing = _template.StatsData.Attributes.GetAttribute(attribute);
            return existing.GetValue();
        }

        private void InitializeTemplate()
        {
            _template = new()
            {
                DatabaseID = _templateAsset.DatabaseID,
                ReadableID = _templateAsset.ReadableID,
                FirstName = _templateAsset.FirstName,
                LastName = _templateAsset.LastName,
                Faction = _templateAsset.Faction,
                BodyType = _templateAsset.BodyType,
                CreatureType = _templateAsset.CreatureType
            };
            if (_templateAsset.BaseLookWardrobeCollection != null)
                _template.BaseLookWardrobeReadableID = _templateAsset.BaseLookWardrobeCollection.ReadableID;
            _template.EquipmentSlotsData = new();
            foreach (var item in _templateAsset.EquipmentSlotsData)
            {
                if (item.Value.GetEquippedItem() == null)
                    _template.EquipmentSlotsData[item.Key] = item.Value.DefaultItemID;
                else
                    _template.EquipmentSlotsData[item.Key] = item.Value.GetEquippedItem().ReadableID;
            }
            _template.StatsData = new(_templateAsset.StatsData);
            _template.KnownPowers = _templateAsset.StartingPowers.Select(power => power.ScriptName).ToList();
        }

        [Serializable]
        public class MainCharacterTemplate : ICreatureTemplate
        {
            [OdinSerialize, ES3Serializable]
            public string DatabaseID { get; set; }
            [OdinSerialize, ES3Serializable]
            public string ReadableID { get; set; }
            public string FullName => $"{FirstName} {LastName}";
            [OdinSerialize, ES3Serializable]
            public string FirstName { get; set; }
            [OdinSerialize, ES3Serializable]
            public string LastName { get; set; }
            [OdinSerialize, ES3Serializable]
            public Faction Faction { get; set; }
            [OdinSerialize, ES3Serializable]
            public BodyType BodyType { get; set; }
            [OdinSerialize, ES3Serializable]
            public CreatureType CreatureType { get; set; }
            [OdinSerialize, ES3Serializable]
            public string BaseLookWardrobeReadableID { get; set; }
            [OdinSerialize, ES3Serializable]
            public Dictionary<EquipmentSlotID, string> EquipmentSlotsData { get; set; }
            [OdinSerialize, ES3Serializable]
            public StatsData StatsData { get; set; }
            [OdinSerialize, ES3Serializable]
            public List<string> KnownPowers { get; set; }

            public MainCharacterTemplate(CreatureConfig asset)
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
                this.KnownPowers = asset.StartingPowers.Select(power => power.ScriptName).ToList();
            }

            public MainCharacterTemplate()
            {

            }
        }
    }
}
