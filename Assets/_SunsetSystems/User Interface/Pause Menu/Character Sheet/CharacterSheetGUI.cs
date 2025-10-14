using System.Collections.Generic;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Entities.Data;
using SunsetSystems.Party;
using SunsetSystems.Abilities;
using SunsetSystems.UI.Utils;
using UnityEngine;
using System.Linq;

namespace SunsetSystems.UI
{
    public class CharacterSheetGUI : MonoBehaviour
    {
        [SerializeField]
        private List<AttributeGroupUpdateReciever> _attributes;
        [SerializeField]
        private List<SkillGroupUpdateReciever> _skills;
        [SerializeField]
        private DisciplineGroupUpdateReciever _disciplines;

        private void OnEnable()
        {
            UpdateCharacterSheet();
        }

        public void UpdateCharacterSheet()
        {
            string selectedCharacterKey = CharacterSelector.SelectedCharacterKey;
            ICreature creature = PartyManager.Instance.GetPartyMemberByID(selectedCharacterKey);
            StatsData data = creature.References.StatsManager.Stats;
            List<IUserInfertaceDataProvider<BaseStat>> attributes = new();
            attributes.AddRange(data.Attributes?.GetAttributeList() ?? new());
            _attributes.ForEach(attributeGroup => attributeGroup.UpdateViews(attributes));
            List<IUserInfertaceDataProvider<BaseStat>> skills = new();
            skills.AddRange(data.Skills?.GetSkillList() ?? new());
            _skills.ForEach(skillGroup => skillGroup.UpdateViews(skills));
            ISpellbookManager spellbook = creature.References.SpellbookManager;
            _disciplines.UpdateViews(spellbook.KnownDisciplines.Cast<IUserInfertaceDataProvider<IDisciplineInfo>>().ToList());
        }
    }
}
