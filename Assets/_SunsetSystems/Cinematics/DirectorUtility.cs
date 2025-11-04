using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace SunsetSystems.Cinematics
{
    public class DirectorUtility : MonoBehaviour
    {
        [SerializeField, Required]
        private PlayableDirector _director;

        private int _resumeRequests = 0;
        private bool _isPaused = false;

        [Button]
        public void HoldPlayback()
        {
            if (_resumeRequests <= 0)
            {
                _director.playableGraph.GetRootPlayable(0).SetSpeed(0);
                _isPaused = true;
                _resumeRequests = 0;
            }
            if (_resumeRequests > 0)
            {
                _resumeRequests--;
            }
        }

        [Button]
        public void ResumePlayback()
        {
            if (_isPaused)
            {
                _director.playableGraph.GetRootPlayable(0).SetSpeed(1);
                _resumeRequests = 0;
                _isPaused = false;
            }
            else
            {
                _resumeRequests++;
            }
        }
    }
}
