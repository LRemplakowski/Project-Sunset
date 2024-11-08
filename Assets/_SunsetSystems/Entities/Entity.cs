﻿using UnityEngine;
using SunsetSystems.Entities.Interfaces;
using Sirenix.OdinInspector;

namespace SunsetSystems.Entities
{
    [DisallowMultipleComponent]
    public abstract class Entity : SerializedMonoBehaviour, IEntity
    {
        public abstract string ID { get; }
        public virtual string Name => gameObject.name;
        public abstract IEntityReferences References { get; }


        [field: SerializeField, Title("Entity Setup"), HideIf("@this is Creature")]
        public Faction Faction { get; private set; }
    }
}
