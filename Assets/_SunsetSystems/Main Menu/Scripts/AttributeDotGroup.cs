using SunsetSystems.Data;
using SunsetSystems.Entities.Characters;
using UnityEngine;

namespace SunsetSystems.MainMenu.UI
{
    public class AttributeDotGroup : ClickableDotGroup
    {
        [SerializeField]
        private AttributeType associatedAttribute = AttributeType.Invalid;
        private MainCharacterCreator gameInitializer;

        protected override void Start()
        {
            base.Start();
            if (!gameInitializer)
                gameInitializer = FindAnyObjectByType<MainCharacterCreator>(FindObjectsInactive.Exclude);
        }

        public override void Initialize()
        {
            base.Initialize();
            if (gameInitializer != null)
            {
                int attributeValue = gameInitializer.GetAttributeValue(associatedAttribute);
                OnClick(attributeValue);
            }
        }

        public override void OnClick(int fullCount)
        {
            base.OnClick(fullCount);
            if (gameInitializer)
                gameInitializer.SetAttribute(associatedAttribute, FullDots);
        }
    }
}