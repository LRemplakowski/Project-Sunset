using SunsetSystems.Core.Database;
using SunsetSystems.Entities.Characters;

namespace SunsetSystems.Abilities
{
    public interface IDisciplinePower : IDatabaseEntry<IDisciplinePower>
    {
        string ID { get; }
        string ScriptName { get; }
        string Name { get; }
        string Description { get; }
        IDiscipline Discipline { get; }
        int Level { get; }
    }
}