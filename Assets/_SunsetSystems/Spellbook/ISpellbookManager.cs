using System.Collections.Generic;

namespace SunsetSystems.Abilities
{
    public interface ISpellbookManager
    {
        IReadOnlyCollection<IDisciplinePower> KnownPowers { get; }
        IReadOnlyCollection<DisciplineData> KnownDisciplines { get; }

        bool IsPowerKnown(IDisciplinePower power);
        bool IsPowerKnown(string powerID);

        bool TryLearnPower(IDisciplinePower power);
    }
}
