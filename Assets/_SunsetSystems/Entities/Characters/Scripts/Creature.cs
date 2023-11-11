﻿using Redcode.Awaiting;
using Sirenix.OdinInspector;
using SunsetSystems.Entities.Characters.Actions;
using SunsetSystems.Entities.Characters.Interfaces;
using SunsetSystems.Entities.Creatures.Interfaces;
using SunsetSystems.Entities.Data;
using SunsetSystems.Equipment;
using System.Collections.Generic;
using System.Threading.Tasks;
using UMA;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

namespace SunsetSystems.Entities.Characters
{
    public class Creature : PersistentEntity, ICreature, ICreatureTemplateProvider
    {
        [ShowInInspector]
        private Queue<EntityAction> _actionQueue = new();
        private Queue<EntityAction> ActionQueue
        {
            get
            {
                if (_actionQueue == null)
                {
                    _actionQueue = new Queue<EntityAction>();
                    _ = PerformAction(new Idle(this));
                }
                return _actionQueue;
            }
        }

        #region Unity messages
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            ActionQueue.Enqueue(new Idle(this));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Debug.Log($"Destroying creature {gameObject.name}!");
        }

        public void Update()
        {
            if (ActionQueue.Count <= 0)
                ActionQueue.Enqueue(new Idle(this));
            if (ActionQueue.Peek() is Idle && ActionQueue.Count > 1)
            {
                ActionQueue.Dequeue();
                ActionQueue.Peek().Begin();
            }
            else if (ActionQueue.Peek().EvaluateActionFinished())
            {
                ActionQueue.Dequeue();
                if (ActionQueue.Count == 0)
                    ActionQueue.Enqueue(new Idle(this));
                ActionQueue.Peek().Begin();
            }
        }
        #endregion

        #region ICreature
        public new Faction Faction => References.CreatureData.Faction;
        public new ICreatureReferences References
        {
            get
            {
                if (_references is not ICreatureReferences)
                    _references = base.GetComponent<ICreatureReferences>();
                return _references as ICreatureReferences;
            }
        }

        public EntityAction PeekCurrentAction => _actionQueue.Peek();
        public bool HasActionsQueued => PeekCurrentAction is not Idle || _actionQueue.Count > 1;

        public void ForceToPosition(Vector3 position)
        {
            ClearAllActions();
            Debug.Log($"Forcing Creature {gameObject.name} to position: {position}");
            References.GetComponent<NavMeshAgent>().Warp(position);
        }

        public void ClearAllActions()
        {
            while (ActionQueue.Count > 0)
                ActionQueue.Dequeue().Abort();
            ActionQueue.Enqueue(new Idle(this));
        }

        public EntityAction PeekActionFromQueue()
        {
            return ActionQueue.Peek();
        }

        public bool HasActionsInQueue()
        {
            return !ActionQueue.Peek().GetType().IsAssignableFrom(typeof(Idle)) || ActionQueue.Count > 1;
        }

        public async Task PerformAction(EntityAction action, bool clearQueue = false)
        {
            if (action.IsPriority || clearQueue)
                ClearAllActions();
            ActionQueue.Enqueue(action);
            await new WaitForUpdate();
            while (action.ActionFinished is false)
                await new WaitForUpdate();
        }

        [Button]
        public void InjectDataFromTemplate(ICreatureTemplate template)
        {
            References.CreatureData.CopyFromTemplate(template);
            References.StatsManager.CopyFromTemplate(template);
            References.UMAManager.InjectDefaultRecipes(template.BaseUmaRecipes);
            References.EquipmentManager.CopyFromTemplate(template);
        }

        public new T GetComponent<T>() where T : Component => References.GetComponent<T>();
        public new T GetComponentInChildren<T>() where T : Component => References.GetComponentInChildren<T>();
        #endregion

        #region ICreatureTemplateProvider
        public ICreatureTemplate CreatureTemplate => new TemplateFromInstance(this);

        public int MovementRange => throw new System.NotImplementedException();

        private class TemplateFromInstance : ICreatureTemplate
        {
            public TemplateFromInstance(ICreature instance)
            {
                DatabaseID = instance.References.CreatureData.DatabaseID;
                ReadableID = instance.References.CreatureData.ReadableID;
                FirstName = instance.References.CreatureData.FirstName;
                LastName = instance.References.CreatureData.LastName;
                Faction = instance.Faction;
                BodyType = instance.References.CreatureData.BodyType;
                CreatureType = instance.References.CreatureData.CreatureType;
                PortraitAssetRef = instance.References.CreatureData.PortraitAssetRef;
                BaseUmaRecipes = instance.References.CreatureData.BaseUmaRecipes;
                EquipmentSlotsData = instance.References.EquipmentManager.EquipmentSlots;
                StatsData = new(instance.References.StatsManager.Stats);
            }

            public string DatabaseID { get; }

            public string ReadableID { get; }

            public string FullName => $"{FirstName} {LastName}".Trim();

            public string FirstName { get; }

            public string LastName { get; }

            public Faction Faction { get; }

            public BodyType BodyType { get; }

            public CreatureType CreatureType { get; }

            public AssetReferenceSprite PortraitAssetRef { get; }

            public List<UMARecipeBase> BaseUmaRecipes { get; }

            public Dictionary<EquipmentSlotID, IEquipmentSlot> EquipmentSlotsData { get; }

            public StatsData StatsData { get; }
        }
        #endregion
    }
}
