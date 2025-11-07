using System;
using Sirenix.OdinInspector;
using SunsetSystems.UI.Utils;
using UnityEngine;

namespace SunsetSystems.Utils
{
    public class PostGameScreenInfo : SerializedMonoBehaviour
    {
        private static bool _showPostGameScreen = false;

        [SerializeField]
        private IConfirmationPopup _confirmationPopup;

        private void Start()
        {
            if (_showPostGameScreen)
            {
                DoShowPopup();
                _showPostGameScreen = false;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void DoShowPopup()
        {
            if (_confirmationPopup == null) return;

            void OnConfirm() => gameObject.SetActive(false);
            _confirmationPopup.Show(OnConfirm);
        }

        public void SetShowPostGameScreenAfterLevelLoad()
        {
            _showPostGameScreen = true;
        }
    }
}
