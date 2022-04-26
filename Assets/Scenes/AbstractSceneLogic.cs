using Entities.Characters;
using UnityEngine;
using SunsetSystems.Resources;
using System.Threading.Tasks;
using SunsetSystems.Loading;
using CleverCrow.Fluid.UniqueIds;
using SunsetSystems.Data;

namespace SunsetSystems.Loading
{
    [RequireComponent(typeof(UniqueId))]
    internal abstract class AbstractSceneLogic : MonoBehaviour, ISaveRuntimeData
    {
        [SerializeField]
        protected CreatureData creaturePrefab;
        [SerializeField]
        protected UniqueId unique;

        private void Reset() => creaturePrefab = ResourceLoader.GetEmptyCreaturePrefab();

        protected void OnValidate()
        {
            unique = GetComponent<UniqueId>();
        }

        protected void Awake()
        {
            unique = GetComponent<UniqueId>();
        }

        protected void Start()
        {
            ES3ReferenceMgr es3 = FindObjectOfType<ES3ReferenceMgr>();
            if (es3)
                es3.RefreshDependencies();
        }

        public abstract Task StartSceneAsync(SceneLoadingData data);

        public abstract void SaveRuntimeData();

        public abstract void LoadRuntimeData();
    }
}
