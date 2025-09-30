using SunsetSystems.Core.Database;
using UMA.CharacterSystem;

namespace SunsetSystems.UMA
{
    public interface IUMAWardrobeDatabaseItem : IDatabaseEntry<IUMAWardrobeDatabaseItem>
    {
        UMAWardrobeCollection Data { get; }
    }
}
