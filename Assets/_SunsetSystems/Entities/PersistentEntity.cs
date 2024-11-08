using CleverCrow.Fluid.UniqueIds;
using Sirenix.OdinInspector;
using SunsetSystems.Persistence;
using System;
using UnityEngine;
using SunsetSystems.Entities.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SunsetSystems.Entities
{
    [RequireComponent(typeof(UniqueId))]
    [RequireComponent(typeof(CachedReferenceManager))]
    public class PersistentEntity : Entity, IPersistentObject
    {
        [SerializeField, ReadOnly]
        private UniqueId _unique;
        [field: SerializeField]
        public bool EnablePersistence { get; private set; } = true;
        public string PersistenceID 
        { 
            get 
            {
                if (_unique == null)
                    _unique = GetComponent<UniqueId>();
                return _unique.Id;
            } 
        }
        public override string ID => PersistenceID;
        public string GameObjectName => gameObject.name;

        [field: SerializeField]
        public List<IPersistentComponent> PersistentComponents { get; private set; } = new();

        [SerializeField, Required]
        protected IEntityReferences _references;
        public override IEntityReferences References
        {
            get
            {
                if (_references == null)
                    _references = GetComponent<IEntityReferences>();
                return _references;
            }
        }

        protected virtual void OnValidate()
        {
            if (_unique == null)
                _unique = GetComponent<UniqueId>();
            if (_references == null)
                _references = GetComponent<IEntityReferences>();
            List<IPersistentComponent> cachedComponents = new(PersistentComponents);
            cachedComponents.AddRange(GetComponents<IPersistentComponent>());
            PersistentComponents.Clear();
            PersistentComponents.AddRange(cachedComponents.Distinct());
        }

        public virtual object GetPersistenceData()
        {
            PersistenceData data = new(this);
            return data;
        }

        public virtual void InjectPersistenceData(object data)
        {
            if (data is not PersistenceData saveData)
                return;
            gameObject.SetActive(saveData.GameObjectActive);
            if (saveData.PersistentComponentData != null)
            {
                foreach (IPersistentComponent component in PersistentComponents)
                {
                    if (saveData.PersistentComponentData.TryGetValue(component.ComponentID, out object componentData))
                        component.InjectComponentPersistenceData(componentData);
                }
            }
        }

        [Serializable]
        public class PersistenceData
        {
            [ES3Serializable]
            public bool GameObjectActive;
            [ES3Serializable]
            public Vector3 TransformPosition;
            [ES3Serializable]
            public Quaternion TransformRotation;
            [ES3Serializable]
            public Dictionary<string, object> PersistentComponentData;

            public PersistenceData(PersistentEntity persistentEntity)
            {
                GameObjectActive = persistentEntity.gameObject.activeSelf;
                TransformPosition = persistentEntity.transform.position;
                TransformRotation = persistentEntity.transform.rotation;
                PersistentComponentData = new();
                if (persistentEntity.PersistentComponents.Count > 0)
                {
                    foreach (IPersistentComponent persistentComponent in persistentEntity.PersistentComponents)
                    {
                        PersistentComponentData[persistentComponent.ComponentID] = persistentComponent.GetComponentPersistenceData();
                    }
                }
            }

            public PersistenceData() : base()
            {
                PersistentComponentData = new();
            }
        }
    }
}
