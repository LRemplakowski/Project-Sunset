using Sirenix.OdinInspector;
using UnityEngine;

namespace SunsetSystems.Abilities
{
    [CreateAssetMenu(fileName = "New Spell Ability", menuName = "Sunset Abilities/AOE Spell Ability")]
    public class AOESpell : SpellAbility, IAOEAbility
    {
        [BoxGroup("AOE Data")]
        [SerializeField]
        private int _aoeRadius;

        public int GetAOERadius(IAbilityContext context)
        {
            return _aoeRadius;
        }
    }
}
