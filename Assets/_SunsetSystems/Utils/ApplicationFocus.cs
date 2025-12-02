using UnityEngine;

namespace SunsetSystems.Utils
{
    public class ApplicationFocus : MonoBehaviour
    {
        [SerializeField]
        private UltEvents.UltEvent OnFocusGained, OnFocusLost;

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                OnFocusGained?.Invoke();
            }
            else
            {
                OnFocusLost?.Invoke();
            }
        }
    }
}
