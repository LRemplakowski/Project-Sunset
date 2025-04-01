using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace SunsetSystems.Cinematics
{
    public class DirectorUtility : MonoBehaviour
    {
        [SerializeField, Required]
        private PlayableDirector _director;

        [Button]
        public void HoldPlayback()
        {
            _director.playableGraph.GetRootPlayable(0).SetSpeed(0);
        }

        [Button]
        public void ResumePlayback()
        {
            _director.playableGraph.GetRootPlayable(0).SetSpeed(1);
        }
    }
}
