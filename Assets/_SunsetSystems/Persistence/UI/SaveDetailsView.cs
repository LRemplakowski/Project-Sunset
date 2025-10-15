using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SunsetSystems.Persistence.UI
{
    public class SaveDetailsView : SerializedMonoBehaviour, ISaveView
    {
        [SerializeField, Required]
        private TMP_Text _saveNameText;
        [SerializeField, Required]
        private TMP_Text _saveDateText;
        [SerializeField, Required]
        private TMP_Text _playTimeText;
        [SerializeField, Required]
        private TMP_Text _activeQuestText;
        [SerializeField, Required]
        private Image _saveThumbnail;

        public void Initialize(SaveLoadScreenManager manager)
        {
            // No initialization needed for this view
        }

        public void Show(SaveMetaData saveMetaData)
        {
            if (!saveMetaData.IsValid())
            {
                _saveNameText.text = "No Save Selected";
                _saveDateText.text = string.Empty;
                _playTimeText.text = string.Empty;
                _activeQuestText.text = string.Empty;
                _saveThumbnail.color = Color.clear;
                return;
            }
            _saveNameText.text = saveMetaData.SaveName;
            _saveDateText.text = $"Saved on: {saveMetaData.SaveDate:G}";
            _playTimeText.text = $"Play Time: {FormatPlayTime(saveMetaData.PlayTime)}";
            _activeQuestText.text = string.IsNullOrWhiteSpace(saveMetaData.ActiveQuestName) ? "No Active Quest" : $"{saveMetaData.ActiveQuestName}";
            var imgTexture = saveMetaData.SaveScreenShot;
            if (imgTexture != null)
            {
                _saveThumbnail.color = Color.white;
                _saveThumbnail.sprite = Sprite.Create(imgTexture, new(0, 0, imgTexture.width, imgTexture.height), new(.5f, .5f));
            }
        }

        private string FormatPlayTime(double totalSeconds)
        {
            int hours = (int)(totalSeconds / 3600);
            int minutes = (int)((totalSeconds % 3600) / 60);
            int seconds = (int)(totalSeconds % 60);
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
    }
}
