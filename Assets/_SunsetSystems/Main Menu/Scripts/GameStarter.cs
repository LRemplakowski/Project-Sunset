using Sirenix.OdinInspector;
using SunsetSystems.Core.SceneLoading;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Game;
using SunsetSystems.Party;
using SunsetSystems.Persistence;
using UltEvents;
using UnityEngine;

namespace SunsetSystems.Data
{
    public class GameStarter : SerializedMonoBehaviour
    {
        private const string MAIN_MENU = "MainMenuParent";

        [Title("References")]
        [SerializeField]
        private GameObject _mainMenuParent;
        [SerializeField, Required]
        private SceneLoadingDataAsset startSceneData;

        [Title("Runtime")]
        [SerializeField]
        private ICreatureTemplateProvider _playerCharacterTemplate;

        [field: Title("Events")]
        [field: SerializeField]
        public UltEvent OnGameStart { get; private set; }

        private void Start()
        {
            if (!_mainMenuParent)
                _mainMenuParent = GameObject.FindGameObjectWithTag(MAIN_MENU);
        }

        public void SelectBackground(PlayerCharacterBackground selectedBackground)
        {
            //this._selectedBackground = selectedBackground;
        }

        public void SelectBodyType(BodyType selectedBodyType)
        {
            //this._playerCharacterData.BodyType = selectedBodyType;
        }

        public void SetAttribueValue(AttributeType attribute, int value)
        {
            //_playerCharacterData.Stats.Attributes.GetAttribute(attribute).SetValue(value);
        }

        public void SetSkillValue(SkillType skill, int value)
        {
            //_playerCharacterData.Stats.Skills.GetSkill(skill).SetValue(value);
        }

        public void SetCharacterName(string characterName)
        {
            //this._playerCharacterData.FirstName = characterName;
        }

        public void StartGame()
        {            
            PartyManager.Instance.RecruitMainCharacter(_playerCharacterTemplate);
            OnGameStart?.Invoke();
            SaveLoadManager.ForceCreateNewSaveData();
            _ = LevelLoader.Instance.LoadNewScene(startSceneData);
        }

        //private CreatureConfig GetMatchingCreatureAsset()
        //{
        //    return _playerCharacterData.BodyType switch
        //    {
        //        BodyType.Male => _selectedBackground switch
        //        {
        //            PlayerCharacterBackground.Agent => ResourceLoader.GetMaleAgentAsset(),
        //            PlayerCharacterBackground.Convict => ResourceLoader.GetMaleConvictAsset(),
        //            PlayerCharacterBackground.Journalist => ResourceLoader.GetMaleJournalistAsset(),
        //            _ => ResourceLoader.GetDefaultCreatureAsset(),
        //        },
        //        BodyType.Female => _selectedBackground switch
        //        {
        //            PlayerCharacterBackground.Agent => ResourceLoader.GetFemaleAgentAsset(),
        //            PlayerCharacterBackground.Convict => ResourceLoader.GetFemaleConvictAsset(),
        //            PlayerCharacterBackground.Journalist => ResourceLoader.GetFemaleJournalistAsset(),
        //            _ => ResourceLoader.GetDefaultCreatureAsset(),
        //        },
        //        _ => ResourceLoader.GetDefaultCreatureAsset(),
        //    };
        //}
    }

    public interface IResetable
    {
        void ResetOnGameStart();
    }
}
