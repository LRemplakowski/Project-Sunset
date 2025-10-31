using System;

namespace SunsetSystems.Abilities
{
    public interface IDiscipline : IComparable<IDiscipline>
    {
        string ID { get; }
        string Name { get; }
        string Description { get; }
    }
}
