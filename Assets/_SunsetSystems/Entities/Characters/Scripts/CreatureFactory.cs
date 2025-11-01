using Redcode.Awaiting;
using Sirenix.OdinInspector;
using SunsetSystems.Entities.Characters;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SunsetSystems.Entities.Creatures
{
    public class CreatureFactory : MonoBehaviour
    {
        [Title("References")]
        [SerializeField]
        private Transform _creatureStorageTransform;
        [SerializeField]
        private AssetReference _creaturePrefabReference;

        public static CreatureFactory Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        [Button]
        public async Awaitable<ICreature> Create(ICreatureTemplate creatureTemplate)
        {
            ICreature newInstance = await GetNewCreatureInstance();
            await Awaitable.NextFrameAsync();
            newInstance.InjectDataFromTemplate(creatureTemplate);
            return newInstance;
        }

        public async Awaitable<ICreature> Create(ICreatureTemplate creatureTemplate, Transform parent)
        {
            ICreature newInstance = await GetNewCreatureInstance();
            await Awaitable.NextFrameAsync();
            newInstance.InjectDataFromTemplate(creatureTemplate);
            newInstance.Transform.SetParent(parent);
            return newInstance;
        }

        public async Awaitable<ICreature> Create(ICreatureTemplate creatureTemplate, Vector3 position, Quaternion rotation, Transform parent) 
        {
            ICreature newInstance = await GetNewCreatureInstance();
            await Awaitable.NextFrameAsync();
            newInstance.InjectDataFromTemplate(creatureTemplate);
            newInstance.Transform.SetParent(parent);
            newInstance.ForceToPosition(position, rotation);
            return newInstance;
        }

        private async Awaitable<ICreature> GetNewCreatureInstance()
        {
            var opHandle = Addressables.InstantiateAsync(_creaturePrefabReference.RuntimeKey, _creatureStorageTransform.position, Quaternion.identity, _creatureStorageTransform);
            await opHandle.Task;
            return opHandle.Result.GetComponent<ICreature>();
        }

        public void DestroyCreature(ICreature instance)
        {
            Addressables.ReleaseInstance(instance.References.GameObject);
        }
    }
}
