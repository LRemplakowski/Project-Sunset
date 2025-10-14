using System.Collections.Generic;

namespace SunsetSystems.Abilities
{
    public interface IDisciplineInfo
    {
        public IDiscipline Discipline { get; }
        public int CurrentLevel { get; }
        public IReadOnlyCollection<IDisciplinePower> KnownPowers { get; }
    }
}
