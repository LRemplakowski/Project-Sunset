using System;
using System.Collections.Generic;
using System.Linq;
using Redcode.Awaiting;
using Sirenix.OdinInspector;
using SunsetSystems.ActionSystem;
using SunsetSystems.Entities.Data;
using SunsetSystems.Equipment;
using UMA.CharacterSystem;
using UnityEngine;

namespace SunsetSystems.Entities.Characters
{
    public class Creature : PersistentEntity, ICreature
    {
        [Title("Runtime")]
        [ShowInInspector]
        private Queue<EntityAction> _actionQueue = new();
        private Queue<EntityAction> ActionQueue
        {
            get
            {
                if (_actionQueue == null)
                {
                    _actionQueue = new Queue<EntityAction>();
                    _actionQueue.Enqueue(new Idle(this));
                }
                return _actionQueue;
            }
        }

        #region Unity messages
        protected virtual void Awake()
        {

        }

        protected virtual void Start()
        {
            ActionQueue.Enqueue(new Idle(this));
        }

        protected virtual void OnDestroy()
        {
            Debug.Log($"Destroying creature {gameObject.name}!");
        }

        public void Update()
        {
            if (ActionQueue.Count <= 0)
                ActionQueue.Enqueue(new Idle(this));
            if (ActionQueue.Peek() is Idle && ActionQueue.Count > 1)
            {
                var idle = ActionQueue.Dequeue();
                idle.Abort();
                idle.Cleanup();
                ActionQueue.Peek().Begin();
            }
            else if (ActionQueue.Peek().EvaluateAction())
            {
                ActionQueue.Dequeue();
                if (ActionQueue.Count == 0)
                    ActionQueue.Enqueue(new Idle(this));
                ActionQueue.Peek().Begin();
            }
        }
        #endregion

        #region ICreature
        public Transform Transform => References.Transform;
        public MonoBehaviour CoroutineRunner => this;
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

        public void ForceToPosition(Vector3 position, Quaternion rotation)
        {
            ClearAllActions();
            References.NavigationManager.Warp(position, rotation);
            //if (NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, (int)NavMeshAreas.Walkable))
            //{
            //    Debug.Log($"Forcing Creature {gameObject.name} to position: {hit.position}!");
            //    References.NavigationManager.Warp(hit.position);
            //}
            //else
            //{
            //    Debug.LogError($"Could not force creature {this} to position {position}! Could not find walkable NavMesh!");
            //}
        }

        [Button]
        public void ForceToPosition(Transform positionTransform) => ForceToPosition(positionTransform.position, positionTransform.rotation);

        public void FacePointInSpace(Vector3 point) => References.NavigationManager.FaceDirectionAfterMovementFinished(point);

        public void FaceTransform(Transform transform)
        {
            if (transform == null)
            {
                Debug.LogError($"Creature {Name} was requested to face a null transfrom!");
            }
            else
            {
                References.NavigationManager.FaceDirectionAfterMovementFinished(transform.position);
            }
        }

        public void ClearAllActions()
        {
            while (ActionQueue.Count > 0)
            {
                var action = ActionQueue.Dequeue();
                action.Abort();
                action.Cleanup();
            }
            ActionQueue.Enqueue(new Idle(this));
        }

        public EntityAction PeekActionFromQueue()
        {
            return ActionQueue.Peek();
        }

        public async Awaitable PerformAction(EntityAction action, bool clearQueue = false)
        {
            if (action.IsPriority || clearQueue)
                ClearAllActions();
            ActionQueue.Enqueue(action);
            await Awaitable.NextFrameAsync();
            await new WaitUntil(() => action.ActionFinished || action.ActionCanceled);
        }

        [Button]
        public void InjectDataFromTemplate(ICreatureTemplate template)
        {
            gameObject.name = template.FullName;
            References.CreatureData.CopyFromTemplate(template);
            References.StatsManager.CopyFromTemplate(template);
            References.UMAManager.BuildUMAFromTemplate(template);
            References.EquipmentManager.CopyFromTemplate(template);
            References.SpellbookManager.CopyFromTemplate(template);
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode is false)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
#endif
        }

        [Button]
        public void InjectDataFromTemplate(ICreatureTemplateProvider templateProvider)
        {
            InjectDataFromTemplate(templateProvider.CreatureTemplate);
        }

        public new T GetComponent<T>() where T : Component => References.GetCachedComponent<T>();
        public new T GetComponentInChildren<T>() where T : Component => References.GetCachedComponentInChildren<T>();
        #endregion

        #region ICreatureTemplateProvider
        public ICreatureTemplate CreatureTemplate => new TemplateFromInstance(this);

        [Serializable]
        public class TemplateFromInstance : ICreatureTemplate
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
                BaseLookWardrobeReadableID = instance.References.UMAManager.BaseLookWardrobeReadableID;
                EquipmentSlotsData = new();
                foreach (var item in instance.References.EquipmentManager.EquipmentSlots)
                {
                    EquipmentSlotsData[item.Key] = item.Value.GetEquippedItem().ReadableID;
                }
                StatsData = new(instance.References.StatsManager.Stats);
                KnownPowers = instance.References.SpellbookManager.KnownPowerIDs.ToList();
            }

            public TemplateFromInstance()
            {

            }

            [ShowInInspector]
            public string DatabaseID { get; private set; }
            [ShowInInspector]
            public string ReadableID { get; private set; }
            [ShowInInspector]
            public string FullName => $"{FirstName} {LastName}".Trim();
            [ShowInInspector]
            public string FirstName { get; private set; }
            [ShowInInspector]
            public string LastName { get; private set; }
            [ShowInInspector]
            public Faction Faction { get; private set; }
            [ShowInInspector]
            public BodyType BodyType { get; private set; }
            [ShowInInspector]
            public CreatureType CreatureType { get; private set; }
            [ShowInInspector]
            public string BaseLookWardrobeReadableID { get; private set; }
            [ShowInInspector]
            public Dictionary<EquipmentSlotID, string> EquipmentSlotsData { get; private set; }
            [ShowInInspector]
            public StatsData StatsData { get; private set; }
            [ShowInInspector]
            public List<string> KnownPowers { get; private set; }
        }
        #endregion

        public override object GetPersistenceData()
        {
            return new CreaturePersistenceData(this);
        }

        public override void InjectPersistenceData(object data)
        {
            base.InjectPersistenceData(data);
            if (data is not CreaturePersistenceData creaturePersistenceData)
                return;
            ForceToPosition(creaturePersistenceData.WorldPosition, Quaternion.identity);
            var dna = References.GetCachedComponentInChildren<DynamicCharacterAvatar>();
            if (dna.UpdatePending())
                dna.CharacterCreated.AddAction((ud) => { if (creaturePersistenceData.UMAHidden) ud.Hide(); else ud.Show(); });
            else
                dna.ToggleHide(creaturePersistenceData.UMAHidden);
        }

        [Serializable]
        public class CreaturePersistenceData : PersistenceData
        {
            public Vector3 WorldPosition;
            public bool UMAHidden;

            public CreaturePersistenceData(Creature creature) : base(creature)
            {
                WorldPosition = creature.References.Transform.position;
                UMAHidden = false;
                if (creature.References != null)
                {
                    var dna = creature.References.GetCachedComponentInChildren<DynamicCharacterAvatar>();
                    if (dna != null)
                        UMAHidden = dna.hide;
                }
            }

            public CreaturePersistenceData() : base()
            {

            }
        }
    }
}
