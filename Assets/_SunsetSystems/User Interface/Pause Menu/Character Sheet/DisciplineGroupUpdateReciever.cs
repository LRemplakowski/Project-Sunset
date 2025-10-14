using SunsetSystems.Abilities;
using SunsetSystems.UI.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SunsetSystems.UI
{
    public class DisciplineGroupUpdateReciever : MonoBehaviour, IUserInterfaceUpdateReciever<IDisciplineInfo>
    {
        [SerializeField]
        private DisciplineType _disciplines;
        [SerializeField]
        private List<DisciplineStatView> _views = new();
        [SerializeField]
        private Transform _viewsParent;
        [SerializeField]
        private DisciplineStatView _viewPrefab;

        public void DisableViews()
        {
            _views.ForEach(v => v.gameObject.SetActive(false));
        }

        public void UpdateViews(List<IUserInfertaceDataProvider<IDisciplineInfo>> data)
        {
            DisableViews();
            List<IDisciplineInfo> stats = data
                .Select(s => s.UIData)
                .Where(d => d.CurrentLevel > 0)
                .OrderBy(d => d.Discipline)
                .ToList();
            foreach (var stat in data)
            {
                DisciplineStatView view = GetView();
                view.UpdateView(stat);
                view.gameObject.SetActive(true);
            }
        }

        private DisciplineStatView GetView()
        {
            DisciplineStatView view;
            view = _views.FirstOrDefault(v => v.isActiveAndEnabled == false);
            if (view == null)
            {
                view = Instantiate(_viewPrefab, _viewsParent);
                view.gameObject.SetActive(false);
                _views.Add(view);
            }
            return view;
        }
    }
}
