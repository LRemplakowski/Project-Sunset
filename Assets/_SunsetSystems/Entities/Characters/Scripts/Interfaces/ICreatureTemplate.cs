using System.Collections.Generic;
using SunsetSystems.Abilities;
using SunsetSystems.Entities.Data;
using SunsetSystems.Equipment;

namespace SunsetSystems.Entities.Characters
{
    public interface ICreatureTemplate
    {
        string DatabaseID { get; }
        string ReadableID { get; }
        string FullName { get; }
        string FirstName { get; }
        string LastName { get; }

        Faction Faction { get; }
        BodyType BodyType { get; }
        CreatureType CreatureType { get; }

        string BaseLookWardrobeReadableID { get; }

        Dictionary<EquipmentSlotID, string> EquipmentSlotsData { get; }
        StatsData StatsData { get; }
        IReadOnlyCollection<IDisciplinePower> KnownPowers { get; }
    }
}
