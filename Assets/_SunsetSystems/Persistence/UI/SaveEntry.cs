using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SunsetSystems.Persistence.UI
{
    public class SaveEntry : SerializedMonoBehaviour, ISaveView
    {
        [Title("References")]
        [SerializeField]
        private Image _saveImage;
        [SerializeField]
        private TextMeshProUGUI _saveName, _saveActiveQuest, _saveDate;

        [Title("Runtime")]
        [ShowInInspector, ReadOnly]
        private SaveLoadScreenManager _saveScreenManager;
        [ShowInInspector, ReadOnly]
        private SaveMetaData _saveMeta;

        public void Initialize(SaveLoadScreenManager manager)
        {
            _saveScreenManager = manager;
        }

        public void Show(SaveMetaData metaData)
        {
            _saveMeta = metaData;
            _saveName.text = metaData.SaveName;
            _saveDate.text = GetFormattedSaveDate(metaData.SaveDate);
            _saveActiveQuest.text = string.IsNullOrWhiteSpace(metaData.ActiveQuestName) ? "No Active Quest" : metaData.ActiveQuestName;
            var imgTexture = metaData.SaveScreenShot;
            if (imgTexture != null)
                _saveImage.sprite = Sprite.Create(imgTexture, new(0, 0, imgTexture.width, imgTexture.height), new(.5f, .5f));
        }

        private string GetFormattedSaveDate(string dateString)
        {
            return dateString;
        }

        public void OnSelect()
        {
            _saveScreenManager.SetSelectedSave(_saveMeta);
        }

        [Button]
        public void LoadSave()
        {
            _saveScreenManager.LoadSave(_saveMeta);
        }

        [Button]
        public void DeleteSave()
        {
            _saveScreenManager.DeleteSave(_saveMeta);
        }
    }
}
