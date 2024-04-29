using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace SunsetSystems.Core.AddressableManagement
{
    public interface IAddressableAssetSource<T> : IAddressableAssetSource where T : UnityEngine.Object
    {
        Task<T> GetAssetAsync(AssetReference assetReference);
        void ReturnAsset(AssetReference asset);
    }

    public interface IAddressableAssetSource
    {

    }
}
