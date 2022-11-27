using Redcode.Awaiting;
using SunsetSystems.Data;
using SunsetSystems.Dialogue;
using SunsetSystems.Entities.Interactable;
using SunsetSystems.Inventory;
using SunsetSystems.Inventory.Data;
using SunsetSystems.Party;
using System.Threading.Tasks;
using UnityEngine;
using Yarn.Unity;

namespace SunsetSystems.Loading
{
    public class HavenSceneLogic : DefaultSceneLogic
    {
        [SerializeField, ES3NonSerializable]
        private YarnProject _sceneDialogues;
        [SerializeField, ES3NonSerializable]
        private string _wakeUpStartNode;
        [SerializeField, ES3NonSerializable]
        private Transform _startPosition;
        [Header("Main room")]
        [SerializeField]
        private GameObject _desireeOnBed;
        [SerializeField]
        private GameObject _handgun;
        [SerializeField]
        private GameObject _crowbar;
        [SerializeField]
        private Weapon _handgunItem;
        [Header("Bathroom")]
        [SerializeField]
        private DialogueEntity _bathroomDoorsDialogue;
        [SerializeField]
        private Doors _havenDoors, _bathroomDoors;

        protected override void Awake()
        {
            base.Awake();
            HavenDialogueCommands.HavenSceneLogic = this;
        }

        public async override Task StartSceneAsync(SceneLoadingData data)
        {
            await base.StartSceneAsync(data);
            await new WaitForUpdate();
            PartyManager.MainCharacter.Agent.Warp(new Vector3(100, 100, 100));
            await new WaitForSeconds(2);
            DialogueHelper.VariableStorage.SetValue(DialogueHelper.SPEAKER_ID, PartyManager.MainCharacter.Data.FullName);
            await new WaitForUpdate();
            DialogueManager.StartDialogue(_wakeUpStartNode, _sceneDialogues);
        }

        private async Task MovePCToPositionAfterDialogue()
        {
            SceneLoadingUIManager fade = this.FindFirstComponentWithTag<SceneLoadingUIManager>(TagConstants.SCENE_LOADING_UI);
            await fade.DoFadeOutAsync(.5f);
            await new WaitForUpdate();
            _desireeOnBed.SetActive(false);
            PartyManager.MainCharacter.Agent.Warp(_startPosition.position);
            await new WaitForSeconds(.5f);
            await new WaitForUpdate();
            await fade.DoFadeInAsync(.5f);
        }

        private static class HavenDialogueCommands
        {
            public static HavenSceneLogic HavenSceneLogic;

            [YarnCommand("GetUpFromBedDesiree")]
            public static void GetUpFromBedDesiree()
            {
                _ = HavenSceneLogic.MovePCToPositionAfterDialogue();
            }

            [YarnCommand("ActivateBathroomDoorInteraction")]
            public static void ActivateBathroomDoorInteraction()
            {
                HavenSceneLogic._bathroomDoorsDialogue.Interactable = true;
            }

            [YarnCommand("OpenBathroomDoors")]
            public static void OpenBathroomDoors()
            {
                HavenSceneLogic._bathroomDoorsDialogue.Interactable = false;
                HavenSceneLogic._bathroomDoors.Interactable = true;
                HavenSceneLogic._bathroomDoors.Interact();
            }

            [YarnCommand("DestroyBathroomDoors")]
            public static void DestroyBathroomDoors()
            {
                HavenSceneLogic._bathroomDoors.Interactable = true;
                HavenSceneLogic._bathroomDoors.Interact();
                Destroy(HavenSceneLogic._bathroomDoors.gameObject);
            }

            [YarnCommand("ActivateApartmentDoorInteraction")]
            public static void ActivateApartmentDoorInteraction()
            {
                HavenSceneLogic._havenDoors.Interactable = true;
            }

            [YarnCommand("HandleGunTaken")]
            public static void HandleGunTaken()
            {
                HavenSceneLogic._handgun.gameObject.SetActive(false);
                InventoryManager.PlayerInventory.AddItem(new(HavenSceneLogic._handgunItem));
            }

            [YarnCommand("HandleCrowbarTaken")]
            public static void HandleCrwobarTaken()
            {
                HavenSceneLogic._crowbar.gameObject.SetActive(false);
            }
        }
    }
}
