using System;

namespace SunsetSystems.Abilities
{
    public interface IDisciplinePower
    {
        string ID { get; }
        string Name { get; }
        string Description { get; }
        IDiscipline Discipline { get; }
        int Level { get; }
    }
}