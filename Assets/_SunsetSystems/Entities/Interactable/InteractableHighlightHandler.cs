using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SunsetSystems.Core.Rendering;
using UnityEngine;

namespace SunsetSystems.Entities.Interactable
{
    public class InteractableHighlightHandler : SerializedMonoBehaviour, IHighlightHandler, IRendererProcessor
    {
        private static WaitForSeconds _waitForSeconds1 = new(1f);

        [Title("Config")]
        [SerializeField, ValueDropdown("GetLayerNames")]
        private string _highlightLayer = "Highlighted";
        [SerializeField]
        private bool _findRenderersAtRuntime = false;
        [SerializeField, HideIf("@this._findRenderersAtRuntime == true")]
        private List<Renderer> _highlightRenderers = new();
        [SerializeField, ShowIf("@this._findRenderersAtRuntime == true"), Required]
        private GameObject _rendererParent;

        [Title("Runtime")]
        [ShowInInspector, ReadOnly]
        private readonly Dictionary<Renderer, int> _rendererLayerCache = new();

#if UNITY_EDITOR
        public string[] GetLayerNames()
        {
            return Enumerable.Range(0, 31).Select(index => LayerMask.LayerToName(index)).Where(l => !string.IsNullOrEmpty(l)).ToArray();
        }
#endif

        public async void Start()
        {
            if (_findRenderersAtRuntime && _rendererParent != null)
            {
                await UniTask.WaitForSeconds(1f);
                _highlightRenderers = _rendererParent.GetComponentsInChildren<Renderer>().ToList();
            }
            CacheDefaultRendererLayers();
        }

        private void CacheDefaultRendererLayers()
        {
            foreach (Renderer renderer in _highlightRenderers)
            {
                if (renderer != null)
                    _rendererLayerCache[renderer] = renderer.gameObject.layer;
            }
        }

        public void SetHighlightActive(bool active)
        {
            int highlightedLayer = LayerMask.NameToLayer(_highlightLayer);
            foreach (var renderer in _highlightRenderers)
            {
                if (renderer == null)
                    continue;
                if (active)
                    renderer.gameObject.layer = highlightedLayer;
                else if (_rendererLayerCache.TryGetValue(renderer, out var cachedLayer))
                    renderer.gameObject.layer = cachedLayer;
            }
        }

        public void InjectRenderers(IEnumerable<Renderer> renderers)
        {
            SetHighlightActive(false);
            _rendererLayerCache.Clear();
            _highlightRenderers.Clear();
            if (renderers != null)
                _highlightRenderers.AddRange(renderers);
            CacheDefaultRendererLayers();
        }
    }
}
