using System;

namespace SunsetSystems.ActorResources
{
    public interface IBloodPointUser
    {
        event Action<int> OnBloodPointUpdate;

        int GetCurrentBloodPoints();
        int GetMaxBloodPoints();

        bool GetCanUseBloodPoints();
        bool UseBloodPoints(int amount);
        void GainBloodPoints(int amount);

        void AddBloodPointUseBlocker(string sourceID);
        void RemoveBloodPointUseBlocker(string sourceID);
    }
}
