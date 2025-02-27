using System.Collections;
using Sirenix.OdinInspector;
using SunsetSystems.Entities.Characters;
using SunsetSystems.ActionSystem;
using SunsetSystems.Entities.Interactable;
using SunsetSystems.Game;
using SunsetSystems.Input.CameraControl;
using SunsetSystems.LevelUtility;
using SunsetSystems.Party;
using SunsetSystems.Persistence;
using UltEvents;
using UnityEngine;

namespace SunsetSystems.Core.SceneLoading
{
    public class SceneTransition : SerializedMonoBehaviour, IInteractionHandler
    {
        [Title("References")]
        [SerializeField]
        private TransitionType _type;
        [SerializeField, ShowIf("@this._type == TransitionType.SceneTransition")]
        private SceneLoadingDataAsset _sceneToLoad;
        [SerializeField, ShowIf("@this._type == TransitionType.InternalTransition")]
        private Waypoint _targetEntryPoint;
        [SerializeField, ShowIf("@this._type == TransitionType.InternalTransition")]
        private BoundingBox _targetBoundingBox;

        [Title("Configs")]
        [SerializeField, ShowIf("@this._type == TransitionType.SceneTransition")]
        private bool _overrideDefaultWaypoint = false;
        [SerializeField, ShowIf("@this._type == TransitionType.SceneTransition && this._overrideDefaultWaypoint == true")]
        private string _targetWaypointTag = "";
        [SerializeField, ShowIf("@this._type == TransitionType.InternalTransition")]
        private CanvasGroup _fadeScreenCanvasGroup;
        [SerializeField, ShowIf("@this._type == TransitionType.InternalTransition"), Min(0)]
        private float _fadeScreenTime = .5f;

        [Title("Events")]
        public UltEvent OnAfterFadeOut = new();

        private IEnumerator _internalTransitionCoroutine;

        private void OnEnable()
        {
            LevelLoader.OnAfterScreenFadeOut += InvokeAfterFadeOut;
        }

        private void OnDisable()
        {
            LevelLoader.OnAfterScreenFadeOut -= InvokeAfterFadeOut;
        }

        public void TriggerTransition()
        {
            HandleInteraction(null);
        }

        public bool HandleInteraction(IActionPerformer interactee)
        {
            Debug.Log("Interacting with area transition!");
            switch (_type)
            {
                case TransitionType.SceneTransition:
                    MoveToScene(_sceneToLoad);
                    break;
                case TransitionType.InternalTransition:
                    MoveToArea(_targetEntryPoint, _targetBoundingBox);
                    break;
                case TransitionType.WorldMap:
                    MoveToWorldMap();
                    break;
                default:
                    return false;
            }
            return true;
        }

        private void MoveToScene(SceneLoadingDataAsset data)
        {
            if (_overrideDefaultWaypoint)
                WaypointManager.Instance.OverrideSceneWaypoint(_targetWaypointTag);
            _ = LevelLoader.Instance.LoadNewScene(data);
        }

        private void MoveToArea(Waypoint waypoint, BoundingBox cameraBoundingBox)
        {
            if (_internalTransitionCoroutine != null)
                StopCoroutine(_internalTransitionCoroutine);
            _internalTransitionCoroutine = FadeOutScreenAndMoveToArea(waypoint, cameraBoundingBox);
            StartCoroutine(_internalTransitionCoroutine);
        }

        private void MoveToWorldMap()
        {
            _ = LevelLoader.Instance.LoadWorldMap();
        }

        private IEnumerator FadeOutScreenAndMoveToArea(Waypoint waypoint, BoundingBox cameraBoundingBox)
        {
            if (_fadeScreenTime <= 0f || _fadeScreenCanvasGroup == null)
            {
                InvokeAfterFadeOut();
                MovePlayerPartyToWaypoint(waypoint);
                MoveCameraToWaypoint(waypoint, cameraBoundingBox);
            }
            else
            {
                float lerp = 0f;
                _fadeScreenCanvasGroup.gameObject.SetActive(true);
                while (lerp < 1)
                {
                    lerp += Time.deltaTime * (1 / _fadeScreenTime);
                    _fadeScreenCanvasGroup.alpha = Mathf.Lerp(0, 1, lerp);
                    yield return null;
                }
                InvokeAfterFadeOut();
                _fadeScreenCanvasGroup.alpha = 1f;
                MovePlayerPartyToWaypoint(waypoint);
                MoveCameraToWaypoint(waypoint, cameraBoundingBox);
                yield return new WaitForSeconds(_fadeScreenTime / 2);
                while (lerp > 0)
                {
                    lerp -= Time.deltaTime * (1 / _fadeScreenTime);
                    _fadeScreenCanvasGroup.alpha = Mathf.Lerp(0, 1, lerp);
                    yield return null;
                }
                _fadeScreenCanvasGroup.alpha = 0f;
                _fadeScreenCanvasGroup.gameObject.SetActive(false);
            }

            static void MovePlayerPartyToWaypoint(Waypoint waypoint)
            {
                var party = PartyManager.Instance.ActiveParty;
                var positions = waypoint.GetPositions(party.Count);
                for (int i = 0; i < party.Count; i++)
                {
                    ICreature creature = party[i];
                    Vector3 position = positions[i];
                    creature.ForceToPosition(position);
                }
            }

            static void MoveCameraToWaypoint(Waypoint waypoint, BoundingBox cameraBoundingBox)
            {
                var camera = GameManager.Instance.GameCamera;
                camera.ForceToPosition(waypoint.transform.position);
                camera.CurrentBoundingBox = cameraBoundingBox;
            }
        }

        public void InvokeAfterFadeOut()
        {
            OnAfterFadeOut?.InvokeSafe();
        }
    }

    public enum TransitionType
    {
        SceneTransition, InternalTransition, WorldMap
    }
}