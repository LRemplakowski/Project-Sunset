using SunsetSystems.Entities.Characters;
using System.Collections.Generic;
using UnityEngine;
using System;
using SunsetSystems.Game;
using SunsetSystems.Combat.Grid;
using Sirenix.OdinInspector;
using SunsetSystems.Entities.Interfaces;
using SunsetSystems.Entities.Characters.Interfaces;

namespace SunsetSystems.Combat
{
    public class Encounter : SerializedMonoBehaviour, IEncounter
    {
        [field: SerializeField]
        public CachedMultiLevelGrid MyGrid { get; private set; }

        [field: SerializeField, Tooltip("Creatures taking part in this encounter.")]
        public List<ICreature> Creatures { get; private set; } = new();

        [SerializeField]
        private EncounterEndTrigger _encounterEndTrigger = EncounterEndTrigger.Automatic;

        [Title("Optional")]
        [SerializeField, Tooltip("(Optional) Custom logic run before the start of the encounter.")]
        private AbstractEncounterLogic encounterStartLogic;
        [SerializeField, Tooltip("(Optional) Custom logic run after the end of the encounter.")]
        private AbstractEncounterLogic encounterEndLogic;

        private int _creatureCounter = 0;

        private void Start()
        {
            if (!MyGrid)
                MyGrid = GetComponent<CachedMultiLevelGrid>();
        }

        [Title("Editor Utility")]
        [Button("Begin Encounter")]
        public async void Begin()
        {
            Debug.LogWarning("Begin encounter, do encounter start logic.");
            if (encounterStartLogic)
                await encounterStartLogic.Perform();
            GameManager.CurrentState = GameState.Combat;
            await MyGrid.EnableGrid();
            _creatureCounter = Creatures.Count;
            await CombatManager.Instance.BeginEncounter(this);
            //if (_encounterEndTrigger == EncounterEndTrigger.Automatic)
            //{
            //    Creatures.ForEach(c => c.References.StatsManager.OnCreatureDied += DecrementCounterAndCheckForEncounterEnd);
            //}
        }

        private void DecrementCounterAndCheckForEncounterEnd(Creature creature)
        {
            _creatureCounter -= 1;
            creature.References.StatsManager.OnCreatureDied -= DecrementCounterAndCheckForEncounterEnd;
            if (_creatureCounter <= 0)
                End();
        }

        [Button("End Encounter")]
        public async void End()
        {
            Debug.LogWarning("End encounter, do encounter end logic.");
            MyGrid.DisableGrid();
            await CombatManager.Instance.EndEncounter(this);
            GameManager.CurrentState = GameState.Exploration;
            if (encounterEndLogic)
                await encounterEndLogic.Perform();
        }

        private enum EncounterEndTrigger
        {
            Automatic, Manual
        }
    }
}
