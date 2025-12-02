using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Yarn.Unity;

namespace SunsetSystems.Dialogue
{
    public interface IOptionEventData
    {
        DialogueOption Option { get; }
        int OptionIndex { get; }
    }

    public class OptionView : Selectable, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] bool showCharacterName = false;

        public Action<IOptionEventData> OnOptionSelected;

        private DialogueOption _option;
        private int _optionIndex;
        private InputActionReference _quickSelectAction;

        bool hasSubmittedOptionSelection = false;
        
        public int OptionIndex
        {
            get => _optionIndex;
            set
            {
                _optionIndex = value;
            }
        }

        public DialogueOption Option
        {
            get => _option;

            set
            {
                _option = value;

                hasSubmittedOptionSelection = false;

                // When we're given an Option, use its text and update our
                // interactibility.
                if (showCharacterName)
                {
                    text.text = $"{OptionIndex + 1}. {value.Line.Text.Text}";
                }
                else
                {
                    text.text = $"{OptionIndex + 1}. {value.Line.TextWithoutCharacterName.Text}";
                }
                interactable = value.IsAvailable;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_quickSelectAction != null)
            {
                _quickSelectAction.action.performed -= OnQuickSelectPerformed;
            }
        }

        public void SetQuickSelectAction(InputActionReference actionReference)
        {
            if (_quickSelectAction != null)
            {
                _quickSelectAction.action.performed -= OnQuickSelectPerformed;
            }
            _quickSelectAction = actionReference;
            _quickSelectAction.action.performed += OnQuickSelectPerformed;
        }

        private void OnQuickSelectPerformed(InputAction.CallbackContext context)
        {
            if (interactable)
                InvokeOptionSelected();
        }

        // If we receive a submit or click event, invoke our "we just selected
        // this option" handler.
        public void OnSubmit(BaseEventData eventData)
        {
            if (interactable)
                InvokeOptionSelected();
        }

        public void InvokeOptionSelected()
        {
            // We only want to invoke this once, because it's an error to
            // submit an option when the Dialogue Runner isn't expecting it. To
            // prevent this, we'll only invoke this if the flag hasn't been cleared already.
            if (hasSubmittedOptionSelection == false)
            {
                OnOptionSelected.Invoke(new OptionEventData(Option, OptionIndex));
                hasSubmittedOptionSelection = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (interactable)
                InvokeOptionSelected();
        }

        // If we mouse-over, we're telling the UI system that this element is
        // the currently 'selected' (i.e. focused) element. 
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.Select();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            EventSystem.current.SetSelectedGameObject(null);
        }

        private class OptionEventData : IOptionEventData
        {
            public DialogueOption Option { get; private set; }
            public int OptionIndex { get; private set; }
            public OptionEventData(DialogueOption option, int optionIndex)
            {
                Option = option;
                OptionIndex = optionIndex;
            }
        }
    }
}
