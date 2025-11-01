using System.Threading.Tasks;
using Redcode.Awaiting;
using Sirenix.OdinInspector;
using SunsetSystems.Core.SceneLoading.UI;
using SunsetSystems.Dialogue;
using SunsetSystems.Entities.Characters;
using SunsetSystems.ActionSystem;
using SunsetSystems.Entities.Interactable;
using SunsetSystems.Game;
using SunsetSystems.Input.CameraControl;
using SunsetSystems.Inventory.Data;
using SunsetSystems.LevelUtility;
using SunsetSystems.Party;
using UnityEngine;
using Yarn.Unity;

namespace SunsetSystems.Core
{
    public class HavenSceneLogic : DefaultSceneLogic
    {
        [Title("Prologue")]
        [SerializeField]
        private GameObject _desireeOnBed;
        [SerializeField]
        private string landlordDialogueEntryNode;
        [SerializeField]
        private ICreature _landlord;
        [SerializeField]
        private Waypoint _landlordSpawnWaypoint, _landlordSinkWaypoint, _pcLandlordVisitWaypoint, _pcLandlordSinkWaypoint;
        [SerializeField]
        private IInteractable _apartmentDoorLandlordInteraction;
        [SerializeField]
        private string _landlordVisitDialogueEntryNode;
        [SerializeField]
        private Vector3 _landlordEnterCameraPosition = default,
            _landlordEnterCameraRotation = default,
            _landlordSinkCameraPosition = default,
            _landlordSinkCameraRotation = default;
        [SerializeField]
        private Vector3 _tablePositionForCover, _tableRotationForCover;
        [SerializeField]
        private Transform _coffeeTableTransform;
        [SerializeField]
        private Waypoint _pcCoverWaypoint;
        [SerializeField]
        private DoorController _havenDoors;
        [SerializeField]
        private IInteractable _bathroomDoorsDialogue;
        [SerializeField]
        private DoorController _bathroomDoors;
        [Title("Action")]
        [SerializeField]
        private ICreature _dominic;
        [SerializeField]
        private ICreature _kieran;
        [SerializeField]
        private Waypoint _dominicWaypoint, _kieranWaypoint, _dominicFridgeWaypoint, _kieranFridgeWaypoint, _pcFridgeWaypoint, _dominicDoorWaypoint;
        [SerializeField]
        private Vector3 _cameraPositionDominicEnter, _cameraRotationDominicEnter, _cameraPositionPinnedToWall, _cameraRotationPinnedToWall;

        private CameraControlScript CameraControl => GameManager.Instance.GameCamera;

        protected override void Awake()
        {
            base.Awake();
            HavenDialogueCommands.HavenSceneLogic = this;
        }

        public async override Task StartSceneAsync()
        {
            await base.StartSceneAsync();
            await new WaitForUpdate();
        }

        private async Task MovePCToPositionAfterDialogue(ICreature _desiree)
        {
            SceneLoadingUIManager fade = SceneLoadingUIManager.Instance;
            await fade.DoFadeOutAsync(.5f);
            await new WaitForUpdate();
            _desireeOnBed.SetActive(false);
            var waypoint = WaypointManager.Instance.GetSceneDefaultEntryWaypoint();
            _desiree.ForceToPosition(waypoint.transform);
            await fade.DoFadeInAsync(.5f);
        }

        public async void BringKevinForVisit()
        {
            SceneLoadingUIManager fade = SceneLoadingUIManager.Instance;
            await fade.DoFadeOutAsync(.5f);
            _landlord.ForceToPosition(_landlordSpawnWaypoint.transform);
            PartyManager.Instance.MainCharacter.ForceToPosition(_pcLandlordVisitWaypoint.transform);
            CameraControl.ForceToPosition(_landlordEnterCameraPosition);
            CameraControl.ForceRotation(_landlordEnterCameraRotation);
            await fade.DoFadeInAsync(.5f);
        }

        public async Task MoveToSink()
        {
            SceneLoadingUIManager fade = SceneLoadingUIManager.Instance;
            await fade.DoFadeOutAsync(.5f);
            _landlord.ForceToPosition(_landlordSinkWaypoint.transform);
            PartyManager.Instance.MainCharacter.ForceToPosition(_pcLandlordSinkWaypoint.transform);
            CameraControl.ForceToPosition(_landlordSinkCameraPosition);
            CameraControl.ForceRotation(_landlordSinkCameraRotation);
            await new WaitForFixedUpdate();
            await fade.DoFadeInAsync(.5f);
        }

        private void KillTheLandlord()
        {
            _landlord.References.StatsManager.Die();
        }

        public void MoveCoffeeTable()
        {
            _coffeeTableTransform.position = _tablePositionForCover;
            _coffeeTableTransform.eulerAngles = _tableRotationForCover;
            PartyManager.Instance.MainCharacter.ForceToPosition(_pcCoverWaypoint.transform);
        }

        public async void BargeIn()
        {
            SceneLoadingUIManager fade = SceneLoadingUIManager.Instance;
            await fade.DoFadeOutAsync(.5f);

            _havenDoors.Open = true;
            _dominic.References.GameObject.SetActive(true);
            _kieran.References.GameObject.SetActive(true);
            _dominic.ForceToPosition(_dominicWaypoint.transform);
            _kieran.ForceToPosition(_kieranWaypoint.transform);
            CameraControl.ForceToPosition(_cameraPositionDominicEnter);
            await new WaitForFixedUpdate();
            CameraControl.ForceRotation(_cameraRotationDominicEnter);
            PartyManager.Instance.MainCharacter.FacePointInSpace(_pcCoverWaypoint.GetFacingDirection());
            _dominic.FacePointInSpace(_dominicWaypoint.GetFacingDirection());
            _kieran.FacePointInSpace(_kieranWaypoint.GetFacingDirection());
            await new WaitForFixedUpdate();
            await fade.DoFadeInAsync(.5f);
        }

        private async void MoveActorsAndCameraToFridgeConfig()
        {
            SceneLoadingUIManager fade = SceneLoadingUIManager.Instance;
            await fade.DoFadeOutAsync(.5f);
            PartyManager.Instance.MainCharacter.ForceToPosition(_pcFridgeWaypoint.transform);
            _dominic.ForceToPosition(_dominicFridgeWaypoint.transform);
            _kieran.ForceToPosition(_kieranFridgeWaypoint.transform);
            CameraControl.ForceToPosition(_cameraPositionPinnedToWall);
            await new WaitForFixedUpdate();
            CameraControl.ForceRotation(_cameraRotationPinnedToWall);
            PartyManager.Instance.MainCharacter.FacePointInSpace(_pcFridgeWaypoint.GetFacingDirection());
            _dominic.FacePointInSpace(_dominicFridgeWaypoint.GetFacingDirection());
            _kieran.FacePointInSpace(_kieranFridgeWaypoint.GetFacingDirection());
            await fade.DoFadeInAsync(.5f);
        }

        public void RecruitKieran()
        {
            PartyManager.Instance.RecruitCharacter(_kieran);
        }

        public async void MoveDominicToDoorAndDestroy()
        {
            await _dominic.PerformAction(new Move(_dominic, _dominicDoorWaypoint.transform.position));
            await new WaitForSeconds(.5f);
            _dominic.References.GameObject.SetActive(false);
        }



        //public async void QuitGame()
        //{
        //    SceneLoadingUIManager loading = this.FindFirstComponentWithTag<SceneLoadingUIManager>(TagConstants.SCENE_LOADING_UI);
        //    await loading.DoFadeOutAsync(.5f);
        //    //await LevelLoader.Instance.UnloadGameScene();
        //    this.FindFirstComponentWithTag<MainMenuUIManager>(TagConstants.MAIN_MENU_UI).gameObject.SetActive(true);
        //    this.FindFirstComponentWithTag<GameplayUIManager>(TagConstants.GAMEPLAY_UI).gameObject.SetActive(false);
        //    await loading.DoFadeInAsync(.5f);
        //}

        private class HavenSceneData : SceneLogicData
        {

        }

        public static class HavenDialogueCommands
        {
            public static HavenSceneLogic HavenSceneLogic;

            [YarnCommand("GetUpFromBedDesiree")]
            public async static void GetUpFromBedDesiree()
            {
                await HavenSceneLogic.MovePCToPositionAfterDialogue(PartyManager.Instance.MainCharacter);
            }

            [YarnCommand("OpenBathroomDoors")]
            public static void OpenBathroomDoors()
            {
                HavenSceneLogic._bathroomDoorsDialogue.Interactable = false;
                HavenSceneLogic._bathroomDoors.Open = true;
            }

            [YarnCommand("DestroyBathroomDoors")]
            public static void DestroyBathroomDoors()
            {
                HavenSceneLogic._bathroomDoors.gameObject.SetActive(false);
            }

            [YarnCommand("MoveLandlordAndPCToSinkPositions")]
            public async static void MoveLandlordAndPCToSinkPositions()
            {
                await HavenSceneLogic.MoveToSink();
            }

            [YarnCommand("KillKevin")]
            public static void KillKevin()
            {
                HavenSceneLogic.KillTheLandlord();
            }

            [YarnCommand("AddBobbyPinToInventory")]
            public static void AddBobbyPinToInventory()
            {
                Debug.LogError("Should add bobby pin to inventory");
            }

            [YarnCommand("KieranDominicBargeIn")]
            public static void KieranDominicBargeIn()
            {
                HavenSceneLogic.BargeIn();
            }

            [YarnCommand("HandleAltercationWithDominic")]
            public static void HandleAltercationWithDominic()
            {
                HavenSceneLogic.MoveActorsAndCameraToFridgeConfig();
            }

            [YarnCommand("MakeDominicLeave")]
            public static void MakeDominicLeave()
            {
                HavenSceneLogic.MoveDominicToDoorAndDestroy();
            }

            [YarnCommand("BringLandlordForVisit")]
            public static void BringLandlordForVisit()
            {
                HavenSceneLogic.BringKevinForVisit();
            }
        }
    }
}
