using System.Collections;
using System.Linq;
using Redcode.Awaiting;
using Sirenix.OdinInspector;
using SunsetSystems.Core.Database;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Inventory.Data;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;

namespace SunsetSystems.UMA
{
    public interface IUMAManager
    {
        string BaseLookWardrobeReadableID { get; }

        void BuildUMAFromTemplate(ICreatureTemplate template);
    }

    public class UMAManager : SerializedMonoBehaviour, IUMAManager
    {
        [SerializeField, Required]
        private ScriptableUMAConfig _umaConfig;
        [SerializeField, Required]
        private GameObject _umaRoot;
        [field: SerializeField, ReadOnly]
        public string BaseLookWardrobeReadableID { get; private set; }
        [ShowInInspector, ReadOnly]
        private UMAWardrobeCollection _baseLookWardrobeCollection;
        [SerializeField, ReadOnly]
        private DynamicCharacterAvatar _umaAvatar;

        private IEnumerator _updatePendingCoroutine;
        private bool _isUMACreated = false;

        private void Start()
        {
            if (_umaAvatar == null)
            {
                PrepareUMA();
            }
            _umaAvatar.CharacterCreated.AddListener(OnUMACreated);
            RebuildUMADelayed();
            _ = LoadDefaultWardrobeCollection(BaseLookWardrobeReadableID);
        }

        private UMAWardrobeCollection WardrobeCollectionFromID(string readableID)
        {
            if (string.IsNullOrWhiteSpace(readableID))
            {
                return null;
            }
            return UMAWardrobeDatabase.Instance.TryGetEntryByReadableID(readableID, out var entry) ? entry.Data : null;
        }

        private void RebuildUMADelayed()
        {
            if (_updatePendingCoroutine == null)
            {
                _updatePendingCoroutine = UMARebuildAfterSeconds(1f);
                _ = StartCoroutine(_updatePendingCoroutine);
            }

            IEnumerator UMARebuildAfterSeconds(float seconds)
            {
                yield return new WaitForSeconds(seconds);
                _umaAvatar.BuildCharacter(true);
                _updatePendingCoroutine = null;
            }
        }

        public void BuildUMAFromTemplate(ICreatureTemplate template)
        {
            if (_umaAvatar == null)
                PrepareUMA();
            SetBodyType(template.BodyType);
            BaseLookWardrobeReadableID = template.BaseLookWardrobeReadableID;
            RebuildUMADelayed();
            _ = LoadDefaultWardrobeCollection(BaseLookWardrobeReadableID);
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode is false)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        [Button]
        private void PrepareUMA()
        {
            if (!_umaRoot.TryGetComponent(out _umaAvatar))
            {
                _umaAvatar = _umaRoot.AddComponent<DynamicCharacterAvatar>();
                _umaAvatar.BuildCharacterEnabled = false;
#if UNITY_EDITOR
                _umaAvatar.editorTimeGeneration = false;
                UnityEditor.EditorUtility.SetDirty(_umaAvatar);
#endif
                _umaAvatar.RecreateAnimatorOnRaceChange = false;
                _umaAvatar.KeepAnimatorController = true;
                _umaAvatar.predefinedDNA = new();
                var raceData = _umaConfig.BodyRaceData[BodyType.Female];
                var raceAnimator = _umaConfig.RaceAnimators[raceData];
                _umaAvatar.RacePreset = raceData.raceName;
                _umaAvatar.raceAnimationControllers.defaultAnimationController = raceAnimator;
                DynamicCharacterAvatar.RaceAnimator femaleAnimator = new()
                {
                    raceName = raceData.raceName,
                    animatorController = raceAnimator,
                    animatorControllerName = raceAnimator.name
                };
                raceData = _umaConfig.BodyRaceData[BodyType.Male];
                raceAnimator = _umaConfig.RaceAnimators[raceData];
                DynamicCharacterAvatar.RaceAnimator maleAnimator = new()
                {
                    raceName = raceData.raceName,
                    animatorController = raceAnimator,
                    animatorControllerName = raceAnimator.name
                };
                _umaAvatar.raceAnimationControllers.animators.Add(maleAnimator);
                _umaAvatar.raceAnimationControllers.animators.Add(femaleAnimator);
            }
            _umaAvatar.WardrobeRecipes.Clear();
            _ = LoadDefaultWardrobeCollection(_baseLookWardrobeCollection);
        }

        private void SetBodyType(BodyType bodyType)
        {
            _umaAvatar.ChangeRace(_umaConfig.BodyRaceData[bodyType], DynamicCharacterAvatar.ChangeRaceOptions.useDefaults, true);
        }

        private async Awaitable LoadDefaultWardrobeCollection(UMAWardrobeCollection wardrobeCollection)
        {
            if (CanUpdateUma() is false)
            {
                await new WaitUntil(CanUpdateUma);
            }
            if (_baseLookWardrobeCollection != null)
            {
                _umaAvatar.UnloadWardrobeCollection(_baseLookWardrobeCollection.name);
            }
            _baseLookWardrobeCollection = wardrobeCollection;
            if (_baseLookWardrobeCollection != null)
            {
                _umaAvatar.LoadWardrobeCollection(_baseLookWardrobeCollection);
                foreach (var baseColor in _baseLookWardrobeCollection.SharedColors)
                {
                    _umaAvatar.SetColor(baseColor.name, baseColor);
                }
            }
        }

        private async Awaitable LoadDefaultWardrobeCollection(string wardrobeID)
        {
            var wardrobeCollection = WardrobeCollectionFromID(wardrobeID);
            await LoadDefaultWardrobeCollection(wardrobeCollection);
        }

        private void OnUMACreated(UMAData data)
        {
            _isUMACreated = true;
        }

        public async void OnItemEquipped(IEquipableItem item)
        {
            if (CanUpdateUma() is false)
            {
                await new WaitUntil(CanUpdateUma);
            }
            if (item is IWearable wearable && _umaAvatar != null)
            {
                if (wearable.WearableWardrobe != null)
                {
                    _umaAvatar.LoadWardrobeCollection(wearable.WearableWardrobe);
                    _umaAvatar.umaData.Dirty();
                    RebuildUMADelayed();
                }
                else
                {
                    Debug.LogWarning($"Item {wearable} of Creature {_umaRoot.transform.parent.name} has a null WardrobeCollection reference!");
                }
            }
        }

        public async void OnItemUnequipped(IEquipableItem item)
        {
            if (CanUpdateUma() is false)
            {
                await new WaitUntil(CanUpdateUma);
            }
            if (item is IWearable wearable)
            {
                if (wearable.WearableWardrobe != null)
                {
                    _umaAvatar.UnloadWardrobeCollection(wearable.WearableWardrobe.name);
                    _umaAvatar.umaData.Dirty();
                    RebuildUMADelayed();
                }
            }
        }


        private bool CanUpdateUma()
        {
            return _umaAvatar != null && _umaAvatar.umaData != null && _isUMACreated;
        }
    }
}
