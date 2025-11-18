using SunsetSystems.Entities.Characters.Navigation;
using UnityEngine;

namespace SunsetSystems.ActionSystem
{
    [System.Serializable]
    public class Destination : Condition
    {
        [SerializeField]
        private INavigationManager agent;

        public Destination(INavigationManager agent)
        {
            this.agent = agent;
        }

        public override bool IsMet()
        {
            return agent.FinishedCurrentPath;
        }
    }
}
