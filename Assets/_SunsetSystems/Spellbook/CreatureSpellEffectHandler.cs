using Sirenix.OdinInspector;

namespace SunsetSystems.Abilities
{
    public class CreatureSpellEffectHandler : SerializedMonoBehaviour, IEffectHandlerOLD
    {
        public void HandleEffect(IEffectOLD effect, ISpellbookManager caster)
        {
            effect.ApplyEffect(this);
        }

        public EffectHandlerSceneContext GetContext()
        {
            return new(this);
        }
    }
}
