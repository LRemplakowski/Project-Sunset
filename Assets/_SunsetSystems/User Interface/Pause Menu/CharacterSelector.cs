using Sirenix.OdinInspector;
using SunsetSystems.Party;
using TMPro;
using UltEvents;
using UnityEngine;

namespace SunsetSystems.UI
{
    public class CharacterSelector : SerializedMonoBehaviour
    {
        [Title("References")]
        [SerializeField]
        private TextMeshProUGUI _selectedCharacterText;

        [Title("Events")]
        [SerializeField]
        private UltEvent OnSelectedCharacterChanged;

        [Title("Runtime")]
        [ShowInInspector]
        private static string _selectedCharacterKey = "";
        public static string SelectedCharacterKey 
        { 
            get
            {
                if (string.IsNullOrEmpty(_selectedCharacterKey))
                    _selectedCharacterKey = PartyManager.Instance.MainCharacter.References.CreatureData.DatabaseID;
                return _selectedCharacterKey;
            }
        }

        private void Awake()
        {
            _selectedCharacterKey = "";
        }

        private void OnEnable()
        {
            UpdateSelectedText();
        }

        private void OnDestroy()
        {
            _selectedCharacterKey = "";
        }

        private void UpdateSelectedText()
        {
            _selectedCharacterText.text = PartyManager.Instance.GetPartyMemberByID(SelectedCharacterKey).References.CreatureData.FullName;
        }

        public void NextCharacter()
        {
            int currentIndex = PartyManager.Instance.AllCoterieMembers.FindIndex(cd => cd.Equals(SelectedCharacterKey));
            currentIndex = currentIndex + 1 < PartyManager.Instance.AllCoterieMembers.Count ? currentIndex + 1 : 0;
            _selectedCharacterKey = PartyManager.Instance.AllCoterieMembers[currentIndex];
            OnSelectedCharacterChanged?.InvokeSafe();
            UpdateSelectedText();
        }

        public void SelectCharacter(string characterKey)
        {
            if (PartyManager.Instance.AllCoterieMembers.Contains(characterKey))
            {
                _selectedCharacterKey = characterKey;
                OnSelectedCharacterChanged?.InvokeSafe();
                UpdateSelectedText();
            }
        }

        public void PreviousCharacter()
        {
            int currentIndex = PartyManager.Instance.AllCoterieMembers.FindIndex(cd => cd.Equals(SelectedCharacterKey));
            currentIndex = currentIndex - 1 >= 0 ? currentIndex - 1 : PartyManager.Instance.AllCoterieMembers.Count - 1;
            _selectedCharacterKey = PartyManager.Instance.AllCoterieMembers[currentIndex];
            OnSelectedCharacterChanged?.InvokeSafe();
            UpdateSelectedText();
        }

        public void SetActive(bool active) => gameObject.SetActive(active);
    }
}
