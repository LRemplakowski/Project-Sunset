namespace SunsetSystems.Abilities
{
    public interface IEffectHandlerOLD
    {
        EffectHandlerSceneContext GetContext();
        void HandleEffect(IEffectOLD effect, ISpellbookManager caster);
    }
}
