using SunsetSystems.Entities.Characters;
using System.Collections.Generic;
using UnityEngine;
using SunsetSystems.Game;
using SunsetSystems.Combat.Grid;
using Sirenix.OdinInspector;
using UltEvents;
using System.Linq;

namespace SunsetSystems.Combat
{
    public class Encounter : SerializedMonoBehaviour, IEncounter
    {
        [field: SerializeField]
        public GridManager GridManager { get; private set; }

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

        [Title("Events")]
        public UltEvent OnEncounterStart = new();
        public UltEvent OnEncounterEnd = new();

        [Title("Editor Utility")]
        [Button("Begin Encounter")]
        public async void Begin()
        {
            Debug.LogWarning("Begin encounter, do encounter start logic.");
            if (encounterStartLogic)
                await encounterStartLogic.Perform();
            GameManager.Instance.CurrentState = GameState.Combat;
            GridManager.EnableGrid();
            OnEncounterStart?.InvokeSafe();
            _creatureCounter = Creatures.Count;
            _ = CombatManager.Instance.BeginEncounter(this);
            if (_encounterEndTrigger == EncounterEndTrigger.Automatic)
            {
                Creatures.ForEach(c => c.References.StatsManager.OnCreatureDied += DecrementCounterAndCheckForEncounterEnd);
            }
        }

        private void DecrementCounterAndCheckForEncounterEnd(ICreature creature)
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
            GridManager.DisableGrid();
            await CombatManager.Instance.EndEncounter(this);
            GameManager.Instance.CurrentState = GameState.Exploration;
            if (encounterEndLogic)
                await encounterEndLogic.Perform();
            OnEncounterEnd?.InvokeSafe();
        }

        public void AddToEncounter(Creature creature)
        {
            Creatures.Add(creature);
            Creatures = Creatures.Distinct().ToList();
        }

        public void RemoveFromEncounter(Creature creature)
        {
            Creatures.Remove(creature);
        }

        private enum EncounterEndTrigger
        {
            Automatic, Manual
        }
    }
}
