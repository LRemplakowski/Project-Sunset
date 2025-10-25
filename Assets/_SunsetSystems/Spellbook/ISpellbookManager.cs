using System.Collections.Generic;
using SunsetSystems.Entities.Characters;

namespace SunsetSystems.Abilities
{
    public interface ISpellbookManager
    {
        IReadOnlyCollection<string> KnownPowerIDs { get; }
        IReadOnlyCollection<IDisciplinePower> KnownPowers { get; }
        IReadOnlyCollection<IDisciplineInfo> KnownDisciplines { get; }

        void CopyFromTemplate(ICreatureTemplate template);
        bool IsPowerKnown(IDisciplinePower power);

        bool TryLearnPower(IDisciplinePower power);
    }
}
