using Redcode.Awaiting;
using Sirenix.OdinInspector;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Entities.Characters.Interfaces;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SunsetSystems.Entities.Creatures
{
    public class CreatureFactory : MonoBehaviour
    {
        [SerializeField]
        private AssetReference creaturePrefabReference;

        public static CreatureFactory Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        [Button]
        public async Task<ICreature> Create(ICreatureTemplate creatureTemplate)
        {
            ICreature newInstance = await GetNewCreatureInstance();
            await new WaitForUpdate();
            newInstance.InjectDataFromTemplate(creatureTemplate);
            return newInstance;
        }

        private async Task<ICreature> GetNewCreatureInstance()
        {
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(creaturePrefabReference.RuntimeKey);
            await handle.Task;
            return handle.Result.GetComponent<ICreature>();
        }

        public void DestroyCreature(ICreature instance)
        {
            Addressables.ReleaseInstance(instance.References.GameObject);
        }
    }
}
