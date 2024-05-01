using System.Collections.Generic;
using SunsetSystems.Entities.Data;
using SunsetSystems.Equipment;
using UMA.CharacterSystem;

namespace SunsetSystems.Entities.Characters.Interfaces
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

        short BaseLookWardrobeCollectionID { get; }
        UMAWardrobeCollection BaseLookWardrobeCollectionAsset { get; }

        Dictionary<EquipmentSlotID, string> EquipmentSlotsData { get; }
        StatsData StatsData { get; }
    }
}
