using Sirenix.OdinInspector;

namespace SunsetSystems.Combat.UI
{
    public class SimpleTooltip : SerializedMonoBehaviour, IUITooltip
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
