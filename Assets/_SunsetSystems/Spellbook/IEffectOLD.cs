namespace SunsetSystems.Abilities
{
    public interface IEffectOLD
    {
        AffectedHandler AffectedEffectHandler { get; }

        bool ApplyEffect(IEffectHandlerOLD handler);
        bool ValidateTarget(EffectHandlerSceneContext context);
    }

    public enum AffectedHandler
    {
        Caster,
        Target
    }
}
