using SunsetSystems.UI;
using SunsetSystems.Utils;
using UnityEngine;

namespace UI.CharacterPortraits
{
    public class PortraitController : MonoBehaviour
    {
        [SerializeField]
        private PortraitIcon portraitIcon;

        private string _characterID;
        private PauseMenuScreenHandler _pauseMenu;

        private void Start()
        {
            if (portraitIcon == null)
                portraitIcon = GetComponentInChildren<PortraitIcon>();
            _pauseMenu = FindAnyObjectByType<PauseMenuScreenHandler>(FindObjectsInactive.Include);
        }

        public void InitPotrait(Sprite portrait, string characterID)
        {
            portraitIcon.SetIcon(portrait);
            _characterID = characterID;
        }    

        public void OnClick()
        {
            _pauseMenu.OpenMenuScreen(PauseMenuScreen.CharacterSheet);
            _pauseMenu.SetSelectedCharacter(_characterID);
        }
    }
}
