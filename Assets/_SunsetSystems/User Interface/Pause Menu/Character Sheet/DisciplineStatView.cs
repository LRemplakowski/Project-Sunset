using SunsetSystems.Abilities;
using SunsetSystems.UI.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SunsetSystems.UI
{
    public class DisciplineStatView : MonoBehaviour, IUserInterfaceView<IDisciplineInfo>
    {
        [SerializeField]
        private TextMeshProUGUI _text;
        [SerializeField]
        private List<Image> _chips;
        [SerializeField]
        private Sprite _activeChip, _disabledChip;

        public void UpdateView(IUserInfertaceDataProvider<IDisciplineInfo> dataProvider)
        {
            var stat = dataProvider.UIData;
            _text.text = stat.Discipline.Name;
            for (int i = 0; i < _chips.Count; i++)
            {
                _chips[i].sprite = i < stat.CurrentLevel ? _activeChip : _disabledChip;
            }
        }
    }
}
