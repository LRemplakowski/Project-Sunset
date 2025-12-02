using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Redcode.Awaiting;
using Sirenix.OdinInspector;
using SunsetSystems.Audio;
using SunsetSystems.Core.Database;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Game;
using SunsetSystems.Party;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Yarn.Unity;

namespace SunsetSystems.Dialogue
{
    public class DialogueWithHistoryView : DialogueViewBase
    {
        [Title("Config")]
        [SerializeField, Required]
        private TextMeshProUGUI _lineHistory;
        [SerializeField]
        private Button _proceedToNextLineButton;
        [SerializeField, Required]
        private Scrollbar _scrollbar;
        [SerializeField, Required]
        private Transform _optionParent;
        [SerializeField, Required]
        private OptionView _optionViewPrefab;
        [SerializeField]
        private bool _typewriterEffect;
        [SerializeField, ShowIf("_typewriterEffect"), MinValue(0f)]
        private float _typeSpeed;
        [SerializeField]
        private bool _showUnavailableOptions;
        [SerializeField]
        private float _lineCompletionDelay = .5f;
        [SerializeField]
        private Color _speakerNameColor = Color.red;
        [SerializeField]
        private Color _bracketedTextColor = Color.gray;
        [Title("Portrait")]
        [SerializeField]
        private Sprite _placeholderPortraitAsset;
        [SerializeField]
        private GameObject _photoParent;
        [SerializeField]
        private Image _photo;
        [SerializeField]
        private TextMeshProUGUI _photoText;
        [SerializeField]
        private List<OptionView> _optionViews;

        private StringBuilder _lineHistoryStringBuilder = new();

        private const string ROLL_SUCCESS_TAG = "success";
        private const string ROLL_FAIL_TAG = "failure";
        private const string LAST_LINE_TAG = "lastline";
        private const string ALWAYS_SHOW_OPTION = "showAlways";

        public event Action OnOptionsPresented, OnOptionSelectedCustom;
        private Action<int> OnOptionSelected;

        [SerializeField]
        private UnityEvent OnDialogueStarted, OnDialogueFinished;

        private bool _clampScrollbarNextFrame;
        private bool _requestedLineInterrupt = false;
        private bool _optionsPresented = false;
        private bool _canProceedToNextLine = false;

        public void Cleanup()
        {
            _lineHistory.text = string.Empty;
            _lineHistory.maxVisibleCharacters = 0;
            _lineHistoryStringBuilder = new();
        }

        private void Awake()
        {
            _optionViews = new();
        }

        private void Start()
        {
            DialogueManager.Instance.RegisterView(this);
            GameManager.OnGameStateChanged += OnGameStateChanged;
            //gameObject.SetActive(false);
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state != GameState.Dialogue)
            {
                _requestedLineInterrupt = true;
            }
        }

        private void OnDestroy()
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.UnregisterView(this);
            GameManager.OnGameStateChanged -= OnGameStateChanged;
        }

        private void Update()
        {
            if (_clampScrollbarNextFrame)
            {
                ClampScrollbar();
                _clampScrollbarNextFrame = false;
            }
        }

        public override void DialogueComplete()
        {
            OnDialogueFinished?.Invoke();
        }

        public override void DialogueStarted()
        {
            _lineHistory.maxVisibleCharacters = 0;
            _clampScrollbarNextFrame = true;
            _lineHistory.text = string.Empty;
            _lineHistoryStringBuilder = new();
            Cleanup();
            OnDialogueStarted?.Invoke();
        }

        public override void DismissLine(Action onDismissalComplete)
        {
            _clampScrollbarNextFrame = true;
            onDismissalComplete?.Invoke();
        }

        public override void InterruptLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            _requestedLineInterrupt = true;
            //onDialogueLineFinished?.Invoke();
        }

        public async override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            _canProceedToNextLine = false;
            _requestedLineInterrupt = false;
            _clampScrollbarNextFrame = true;
            UpdateSpeakerPhoto(dialogueLine.CharacterName);
            _lineHistory.maxVisibleCharacters = _lineHistory.textInfo.characterCount;
            await new WaitForUpdate();
            string formattedLineText = BuildFormattedText(dialogueLine);
            _lineHistory.text = formattedLineText;
            LayoutRebuilder.MarkLayoutForRebuild(_lineHistory.transform.parent as RectTransform);
            await new WaitForUpdate();
            if (_typewriterEffect && _typeSpeed > 0)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayTyperwriterLoop();
                await TypewriteText(dialogueLine);
            }
            await new WaitForUpdate();
            _lineHistory.maxVisibleCharacters = _lineHistory.textInfo.characterCount;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayTypewriterEnd();
            _clampScrollbarNextFrame = true;
            await new WaitForSecondsRealtime(_lineCompletionDelay);
            if (GameManager.Instance.IsCurrentState(GameState.Dialogue) == false)
                await UniTask.WaitUntil(() => GameManager.Instance.IsCurrentState(GameState.Dialogue));
            if (dialogueLine.Metadata == null || (dialogueLine.Metadata.Any(tag => tag == LAST_LINE_TAG) is false))
                await WaitForProceedToNextLine();
            onDialogueLineFinished?.Invoke();

            async UniTask WaitForProceedToNextLine()
            {
                _proceedToNextLineButton.gameObject.SetActive(true);
                await UniTask.WaitUntil(() => _canProceedToNextLine);
            }
        }

        public void RunNextLine()
        {
            _canProceedToNextLine = true;
            _proceedToNextLineButton.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
        }

        private async UniTask TypewriteText(LocalizedLine line)
        {
            _lineHistory.maxVisibleCharacters += line.CharacterName?.Length ?? 0;
            float _currentVisibleCharacters = _lineHistory.maxVisibleCharacters;
            while (_lineHistory.textInfo.characterCount > _lineHistory.maxVisibleCharacters)
            {
                await UniTask.NextFrame();
                if (_requestedLineInterrupt)
                {
                    _clampScrollbarNextFrame = true;
                    break;
                }
                _lineHistory.maxVisibleCharacters = Mathf.RoundToInt(_currentVisibleCharacters);
                _currentVisibleCharacters += Time.deltaTime * _typeSpeed;
            }
        }

        private string BuildFormattedText(LocalizedLine line)
        {
            _lineHistoryStringBuilder
                .AppendLine("");
            bool appended = AppendRollPrefix(line);
            if (string.IsNullOrWhiteSpace(line.CharacterName) == false)
            {
                //if (appended == false)
                //    _lineHistoryStringBuilder.Append("<size=26>");
                appended = true;
                string speakerName = line.CharacterName;
                if (CreatureDatabase.Instance.TryGetEntryByReadableID(line.CharacterName, out CreatureConfig speakerConfig))
                    speakerName = speakerConfig.FullName;
                _lineHistoryStringBuilder
                    .Append($"<color={ColorToHex(_speakerNameColor)}>{speakerName}: </color>");
            }
            //if (appended)
            //    _lineHistoryStringBuilder.AppendLine("</size>");
            var text = ColorizeSquareBracketText(line.TextWithoutCharacterName.Text, _bracketedTextColor);
            _lineHistoryStringBuilder
                .AppendLine(text);
            return _lineHistoryStringBuilder.ToString();
        }

        private static string ColorizeSquareBracketText(string inputText, Color bracketedTextColor) 
        {
            string pattern = @"\[(.*?)\]";
            string result = Regex.Replace(inputText, pattern, match => $"<color={ColorToHex(bracketedTextColor)}>{match.Value}</color>");
            return result;
        }

        private static string ColorToHex(Color color) => $"#{ColorUtility.ToHtmlStringRGBA(color)}";

        private bool AppendRollPrefix(LocalizedLine dialogueLine)
        {
            if (dialogueLine.Metadata == null || dialogueLine.Metadata.Length <= 0)
                return false;
            //_lineHistoryStringBuilder.Append("<size=26>");
            if (dialogueLine.Metadata.Contains(ROLL_SUCCESS_TAG))
                _lineHistoryStringBuilder.Append("(Success) ");
            else if (dialogueLine.Metadata.Contains(ROLL_FAIL_TAG))
                _lineHistoryStringBuilder.Append("(Failure) ");
            return true;
        }

        private void UpdateSpeakerPhoto(string characterID)
        {
            string characterName = characterID;
            if (string.IsNullOrWhiteSpace(characterID)) 
            {
                if (CreatureDatabase.Instance.TryGetEntry(PartyManager.Instance.MainCharacterKey, out var config))
                {
                    characterID = config.ReadableID;
                    characterName = config.FullName;
                }
            }
            else if (CreatureDatabase.Instance.TryGetEntryByReadableID(characterID, out CreatureConfig config))
            {
                characterID = config.ReadableID;
                characterName = config.FullName;
            }
            //else
            //{
            //    if (PartyManager.Instance.MainCharacter == null)
            //        await new WaitUntil(() => PartyManager.Instance.MainCharacter != null);
            //    characterID = PartyManager.Instance.MainCharacter.References.CreatureData.ReadableID;
            //    characterName = PartyManager.Instance.MainCharacter.References.CreatureData.FullName;
            //}
            SetSpeakerPortrait(characterID);
            _photoText.text = characterName;
        }

        private void SetSpeakerPortrait(string speakerID)
        {
            if (this.GetSpeakerPortrait(speakerID, out var sprite) && sprite != null)
                _photo.sprite = sprite;
            else if (string.IsNullOrWhiteSpace(speakerID) is false)
                _photo.sprite = _placeholderPortraitAsset;
        }

        public void ClampScrollbar()
        {
            _scrollbar.value = 0f;
        }

        public override void RunOptions(DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
        {
            _clampScrollbarNextFrame = true;
            for (int i = 0; i < dialogueOptions.Length; i++)
            {
                DialogueOption option = dialogueOptions[i];
                if (option is null)
                {
                    Debug.LogError($"Encountered null option!");
                    return;
                }
                bool alwaysShowOption = option.Line.Metadata?.Contains(ALWAYS_SHOW_OPTION) ?? false;
                if (option.IsAvailable == false && _showUnavailableOptions == false && alwaysShowOption == false)
                {
                    continue;
                }
                OptionView optionView = Instantiate(_optionViewPrefab, _optionParent);
                optionView.transform.SetAsLastSibling();
                optionView.OnOptionSelected = OptionViewWasSelected;
                _optionViews.Add(optionView);
                optionView.Option = option;
                optionView.interactable = option.IsAvailable;
                optionView.gameObject.SetActive(true);
            }
            _optionsPresented = true;
            OnOptionsPresented?.Invoke();
            OnOptionSelected = onOptionSelected;

            async void OptionViewWasSelected(DialogueOption option)
            {
                CleanupOptions();
                if (option.Line.CharacterName != null)
                {
                    string formattedLineText = BuildFormattedText(option.Line);
                    _lineHistory.text = formattedLineText;
                    await new WaitForUpdate();
                    _lineHistory.maxVisibleCharacters = _lineHistory.textInfo.characterCount;
                    _clampScrollbarNextFrame = true;
                }
                OnOptionSelected(option.DialogueOptionID);
                OnOptionSelectedCustom?.Invoke();
                _optionsPresented = false;
                _clampScrollbarNextFrame = true;
            }

            void CleanupOptions()
            {
                _optionViews.ForEach(ov => Destroy(ov.gameObject));
                _optionViews.Clear();
            }
        }

        public override void UserRequestedViewAdvancement()
        {
            if (_optionsPresented)
                return;
            if (_proceedToNextLineButton.gameObject.activeInHierarchy && _requestedLineInterrupt)
            {
                RunNextLine();
            }
            else
            {
                requestInterrupt?.Invoke();
            }
        }


        public void SetTypeWriterSpeed(float speed)
        {
            _typeSpeed = speed;
        }
    }

    public interface IPortraitUpdateReciever
    {
        void InitializeSpeakerPhoto(string speakerID);
    }
}
