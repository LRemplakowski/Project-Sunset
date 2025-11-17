using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SunsetSystems.Core.AddressableManagement;
using SunsetSystems.Utils.Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SunsetSystems.Core.SceneLoading.UI
{
    [CreateAssetMenu(fileName = "New Loading Screen Config", menuName = "Sunset Core/Loading Screen Provider Config")]
    public class LoadingScreenProviderConfig : SerializedScriptableObject, IAddressableAssetSource<Sprite>
    {
        [SerializeField]
        private List<AssetReference> defaultLoadingScreens = new();
        [ShowInInspector, ReadOnly]
        private readonly List<AssetReference> _loadedScreens = new();

        private void Awake()
        {
            _loadedScreens.Clear();
        }

        public async UniTask<Sprite> GetRandomLoadingScreenAsync()
        {
            AssetReference loadingScreenAssetRef = defaultLoadingScreens.GetRandom();
            var result = await GetAssetAsync(loadingScreenAssetRef);
            return result;
        }

        public void ReleaseLoadingScreens()
        {
            List<AssetReference> toRelease = new(_loadedScreens);
            toRelease.ForEach(screen => ReturnAsset(screen));
        }

        public async UniTask<Sprite> GetAssetAsync(AssetReference assetReference)
        {
            _loadedScreens.Add(assetReference);
            var asyncOp = Addressables.LoadAssetAsync<Sprite>(assetReference);
            await asyncOp.ToUniTask();
            return asyncOp.Result;
        }

        public void ReturnAsset(AssetReference asset)
        {
            if (_loadedScreens.Remove(asset))
                Addressables.Release(asset);
        }
    }
}
