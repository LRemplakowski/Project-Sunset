using SunsetSystems.Entities.Characters;
using SunsetSystems.Loading;
using UnityEngine;
using SunsetSystems.Resources;
using SunsetSystems.UI;
using SunsetSystems.Utils;
using NaughtyAttributes;
using SunsetSystems.Party;

namespace SunsetSystems.Data
{
    public class GameStarter : Singleton<GameStarter>
    {
        private const string MAIN_MENU = "MainMenuParent";

        [SerializeField]
        private CreatureData _playerCharacterData;
        [SerializeField]
        private PlayerCharacterBackground _selectedBackground;
        [SerializeField]
        private string _startSceneName;
        [SerializeField]
        private string _initialEntryPointTag;
        [SerializeField]
        private string _initialBoundingBoxTag;
        [SerializeField]
        private SceneLoader _sceneLoader;
        [SerializeField]
        private GameObject _mainMenuParent;

        private void Start()
        {
            if (!_sceneLoader)
                _sceneLoader = FindObjectOfType<SceneLoader>();
            if (!_mainMenuParent)
                _mainMenuParent = GameObject.FindGameObjectWithTag(MAIN_MENU);
        }

        public void SelectBackground(PlayerCharacterBackground selectedBackground)
        {
            this._selectedBackground = selectedBackground;
        }

        public void SelectBodyType(BodyType selectedBodyType)
        {
            this._playerCharacterData.bodyType = selectedBodyType;
        }

        public void SetAttribueValue(AttributeType attribute, int value)
        {
            _playerCharacterData.stats.attributes.GetAttribute(attribute).SetValue(value);
        }

        public void SetSkillValue(SkillType skill, int value)
        {
            _playerCharacterData.stats.skills.GetSkill(skill).SetValue(value);
        }

        public void SetCharacterName(string characterName)
        {
            this._playerCharacterData.firstName = characterName;
        }

        public async void InitializeGame()
        {
            Start();
            CreatureConfig mainCharacterAsset = GetMatchingCreatureAsset();
            PartyManager.RecruitMainCharacter(new(mainCharacterAsset));
            SceneLoadingData data = new NameLoadingData(_startSceneName, _initialEntryPointTag, _initialBoundingBoxTag, DisableMainMenu);
            await _sceneLoader.LoadGameScene(data);
        }

        public async void InitializeGameDebug()
        {
            Start();
            CreatureConfig debugAsset = ResourceLoader.GetDefaultCreatureAsset();
            PartyManager.RecruitMainCharacter(new(debugAsset));
            SceneLoadingData data = new NameLoadingData(_startSceneName, _initialEntryPointTag, _initialBoundingBoxTag, DisableMainMenu);
            await _sceneLoader.LoadGameScene(data);
        }

        public void DisableMainMenu()
        {
            _mainMenuParent.SetActive(false);
        }

        private CreatureConfig GetMatchingCreatureAsset()
        {
            return _playerCharacterData.bodyType switch
            {
                BodyType.M => _selectedBackground switch
                {
                    PlayerCharacterBackground.Agent => ResourceLoader.GetMaleAgentAsset(),
                    PlayerCharacterBackground.Convict => ResourceLoader.GetMaleConvictAsset(),
                    PlayerCharacterBackground.Journalist => ResourceLoader.GetMaleJournalistAsset(),
                    _ => ResourceLoader.GetDefaultCreatureAsset(),
                },
                BodyType.F => _selectedBackground switch
                {
                    PlayerCharacterBackground.Agent => ResourceLoader.GetFemaleAgentAsset(),
                    PlayerCharacterBackground.Convict => ResourceLoader.GetFemaleConvictAsset(),
                    PlayerCharacterBackground.Journalist => ResourceLoader.GetFemaleJournalistAsset(),
                    _ => ResourceLoader.GetDefaultCreatureAsset(),
                },
                _ => ResourceLoader.GetDefaultCreatureAsset(),
            };
        }
    }
}
