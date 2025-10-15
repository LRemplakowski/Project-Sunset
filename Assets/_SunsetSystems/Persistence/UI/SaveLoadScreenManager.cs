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
        private ISaveView _saveDetailsView;
        [SerializeField]
        private IConfirmationPopup _deleteSaveConfirmationPopup;
        [SerializeField]
        private IConfirmationPopup<string> _newSaveConfirmationPopup;

        private void Start()
        {
            _saveDetailsView.Initialize(this);
            ClearSelectedSave();
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
                ISaveView saveEntry = Instantiate(_saveEntryPrefab, _saveEntriesParent);
                saveEntry.Initialize(this);
                saveEntry.Show(metaData);
            }
        }

        public void SetSelectedSave(SaveMetaData saveMetaData)
        {
            _saveDetailsView.Show(saveMetaData);
        }

        public void ClearSelectedSave()
        {
            _saveDetailsView.Show(default);
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
            _deleteSaveConfirmationPopup.Show(CreatePopupData(saveMetaData), () => ConfirmDeleteSave(saveMetaData));

            static ConfirmationViewData CreatePopupData(SaveMetaData saveMetaData)
            {
                return new ConfirmationViewData("Delete Save", $"Are you sure you want to delete the save file '{saveMetaData.SaveName}'?\nThis action cannot be undone.");
            }
        }

        private void ConfirmDeleteSave(SaveMetaData meta)
        {
            SaveLoadManager.DeleteSaveFile(meta.SaveID);
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
            _newSaveConfirmationPopup.Show(CreatePopupData(), CreateNewSave);

            static ConfirmationViewData CreatePopupData()
            {
                return new ConfirmationViewData("New Save", "Enter a name for your new save file.");
            }
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
