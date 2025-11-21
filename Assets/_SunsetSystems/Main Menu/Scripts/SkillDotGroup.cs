using SunsetSystems.Data;
using SunsetSystems.Entities.Characters;
using UnityEngine;

namespace SunsetSystems.MainMenu.UI
{
    public class SkillDotGroup : ClickableDotGroup
    {
        [SerializeField]
        private SkillType associatedSkill = SkillType.Invalid;
        private MainCharacterCreator gameInitializer;

        protected override void Start()
        {
            if (!gameInitializer)
                gameInitializer = FindAnyObjectByType<MainCharacterCreator>(FindObjectsInactive.Exclude);
            base.Start();
        }

        public override void Initialize()
        {
            base.Initialize();
            if (gameInitializer != null)
            {
                int attributeValue = gameInitializer.GetSkillValue(associatedSkill);
                OnClick(attributeValue);
            }
        }

        public override void OnClick(int fullCount)
        {
            base.OnClick(fullCount);
            if (gameInitializer)
                gameInitializer.SetSkill(associatedSkill, FullDots);
        }
    }
}