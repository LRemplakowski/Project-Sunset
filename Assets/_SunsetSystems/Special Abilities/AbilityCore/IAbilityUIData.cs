using UnityEngine;

namespace SunsetSystems.Abilities
{
    public interface IAbilityUIData
    {
        Sprite GetAbilityIcon(IconState iconState);
        string GetLocalizedName();
        string GetLocalizedDescription();
        string GetAPCostText(IAbilityContext context);
        string GetBloodCostText(IAbilityContext context);

        public enum IconState
        {
            Default, Highlighted, Pressed, Selected, Disabled
        }
    }
}
