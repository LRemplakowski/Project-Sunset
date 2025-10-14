using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using SunsetSystems.Core.SceneLoading;
using SunsetSystems.UI.Utils;
using UnityEngine;

namespace SunsetSystems.Persistence.UI
{
    public class SaveLoadScreenManager : SerializedMonoBehaviour
    {
        [SerializeField, AssetsOnly, Required]
        private SaveEntry _saveEntryPrefab;
        [SerializeField, Required]
        private Transform _saveEntriesParent;
        [SerializeField, Required]
        private CanvasGroup _saveLoadCanvasGroup;
        [SerializeField]
        private GameObject _newSaveGameObject;
        [SerializeField]
        private IConfirmationPopup<string> _newSaveConfirmationPopup;

        private void OnEnable()
        {
            
        }

        public void ShowScreen(bool includeNewSaveSlot = false)
        {
            gameObject.SetActive(true);
            RefreshSaveScreen(includeNewSaveSlot);
        }

        private void RefreshSaveScreen(bool includeNewSaveSlot)
        {
            if (_newSaveGameObject != null)
            {
                _saveEntriesParent.DestroyChildren(_newSaveGameObject.transform);
                _newSaveGameObject.SetActive(includeNewSaveSlot);
            }
            else
            {
                _saveEntriesParent.DestroyChildren();
            }

            var saveMetaData = SaveLoadManager.GetAllSaveMetaData();
            saveMetaData = saveMetaData.OrderByDescending(save => save.SaveDate);
            foreach (var metaData in saveMetaData)
            {
                SaveEntry saveEntry = Instantiate(_saveEntryPrefab, _saveEntriesParent);
                saveEntry.Initialize(this, metaData);
            }
        }

        public void SetSelectedSave(SaveMetaData saveMetaData)
        {
            
        }

        public void ClearSelectedSave()
        {

        }

        public void LoadSave(SaveMetaData saveMetaData)
        {
            if (_saveLoadCanvasGroup)
                _saveLoadCanvasGroup.interactable = false;
            if (_newSaveGameObject == null || _newSaveGameObject.activeInHierarchy is false)
                _ = LevelLoader.Instance.LoadSavedGame(saveMetaData.SaveID);
            //StartCoroutine(DisableInteractionForSeconds(.5f));
        }

        public void DeleteSave(SaveMetaData saveMetaData)
        {
            SaveLoadManager.DeleteSaveFile(saveMetaData.SaveID);
            RefreshSaveScreen(_newSaveGameObject != null && _newSaveGameObject.activeInHierarchy);
            StartCoroutine(DisableInteractionForSeconds(.5f));
        }

        public void CreateNewSave(string saveName)
        {
            SaveLoadManager.CreateNewSaveFile(saveName);
            RefreshSaveScreen(true);
            StartCoroutine(DisableInteractionForSeconds(.5f));
        }

        public void ShowNewSaveConfirmation()
        {
            _newSaveConfirmationPopup.Show(new ConfirmationViewData("New Save", "Enter a name for your new save file."), CreateNewSave);
        }

        public void OnCancel()
        {
            gameObject.SetActive(false);
        }

        private IEnumerator DisableInteractionForSeconds(float seconds)
        {
            _saveLoadCanvasGroup.interactable = false;
            yield return new WaitForSeconds(seconds);
            _saveLoadCanvasGroup.interactable = true;
        }
    }
}
