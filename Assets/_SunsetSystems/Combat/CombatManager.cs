using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redcode.Awaiting;
using Sirenix.OdinInspector;
using SunsetSystems.ActionSystem;
using SunsetSystems.Entities;
using SunsetSystems.Party;
using UltEvents;
using UnityEngine;

namespace SunsetSystems.Combat
{
    public class CombatManager : SerializedMonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        [Title("References")]
        [SerializeField]
        private CanvasGroup _combatUICanvasGroup;
        [field: Title("Runtime")]
        [field: ShowInInspector, ReadOnly]
        public Encounter CurrentEncounter { get; private set; }

        [field: ShowInInspector, ReadOnly]
        public List<ICombatant> Actors { get; private set; }
        public List<ICombatant> LivingActors => Actors.Where(a => a.GetContext().IsAlive).ToList();

        public static event Action<IEnumerable<ICombatant>> OnCombatStart;
        public static event Action<IEnumerable<ICombatant>> OnCombatEnd;
        public static event Action<ICombatant> OnCombatRoundBegin;
        public static event Action<ICombatant> OnCombatRoundEnd;
        public static event Action<ICombatant> OnFullTurnCompleted;

        private int turnCounter;

        private ICombatant _currentActiveActor;
        public ICombatant CurrentActiveActor
        {
            get => _currentActiveActor;
            private set
            {
                ICombatant previous = _currentActiveActor;
                _currentActiveActor = value;
            }
        }
        private ICombatant FirstActor;

        private void Awake()
        {
            Instance = this;
        }

        private void SetCurrentActiveActor(int index)
        {
            ICombatant c = null;
            var livingActors = LivingActors;
            if (index < livingActors.Count)
            {
                c = livingActors[index];
            }
            CurrentActiveActor = c;
        }

        [Title("Editor")]
        [Button]
        public void NextRound()
        {
            if (CurrentActiveActor != null)
            {
                OnCombatRoundEnd?.Invoke(CurrentActiveActor);
                Debug.Log("Combat Manager: " + CurrentActiveActor.References.GameObject.name + " finished round " + turnCounter + "!");
                int index = Actors.IndexOf(CurrentActiveActor);
                SetCurrentActiveActor(++index < LivingActors.Count ? index : 0);
            }
            else
            {
                FirstActor = DecideFirstActor(Actors);
                CurrentActiveActor = FirstActor;
            }
            if (LivingActors.IndexOf(CurrentActiveActor) == 0)
            {
                turnCounter++;
                OnFullTurnCompleted?.Invoke(CurrentActiveActor);
            }
            OnCombatRoundBegin?.Invoke(CurrentActiveActor);
            SetCombatUIActive(IsActiveActorPlayerControlled());
            Debug.Log("Combat Manager: " + CurrentActiveActor.References.GameObject.name + " begins round " + turnCounter + "!");
        }

        public async Task BeginEncounter(Encounter encounter)
        {
            CurrentEncounter = encounter;
            turnCounter = 0;
            Actors = new();
            Actors.AddRange(PartyManager.Instance.ActiveParty.Select(c => c.References.CombatBehaviour));
            Actors.AddRange(encounter.Creatures.FindAll(c => c != null).Select(c => c.References.CombatBehaviour));
            OnCombatStart?.Invoke(Actors);
            SetCombatUIActive(false);
            await MoveAllCreaturesToNearestGridPosition(Actors, CurrentEncounter);
            await new WaitForSeconds(1f);
            NextRound();
        }

        private void SetCombatUIActive(bool active)
        {
            if (_combatUICanvasGroup != null)
                _combatUICanvasGroup.interactable = active;
        }

        private ICombatant DecideFirstActor(IList<ICombatant> creatures)
        {
            //creatures.OrderByDescending(creature => creature.References.GetComponentInChildren<StatsManager>().GetInitiative());
            return creatures[0];
        }

        public async Task EndEncounter(Encounter encounter)
        {
            await Task.Yield();
            OnCombatEnd?.Invoke(Actors);
            CurrentEncounter = null;
            Actors = null;
            CurrentActiveActor = null;
        }

        private static async Awaitable MoveAllCreaturesToNearestGridPosition(IEnumerable<ICombatant> actors, Encounter currentEncounter)
        {
            List<Awaitable> moveIntoPositions = new();
            foreach (ICombatant combatant in actors)
            {
                Vector3Int gridPosition = currentEncounter.GridManager.GetNearestWalkableGridPosition(combatant.References.Transform.position, false);
                Debug.Log($"{nameof(CombatManager)} >>> Nearest grid position for Combatant {combatant.References.GameObject.name} is {gridPosition}!");
                moveIntoPositions.Add(combatant.PerformAction(new Move(combatant, currentEncounter.GridManager[gridPosition], currentEncounter.GridManager), true));
                await Awaitable.NextFrameAsync();
            }
            foreach (var awaitable in moveIntoPositions)
            {
                await awaitable;
            }
        }

        public bool IsBeforeFirstRound()
        {
            return turnCounter <= 0;
        }

        public bool IsFirstRoundOfCombat()
        {
            return turnCounter == 1;
        }

        public bool IsActiveActorPlayerControlled()
        {
            return _currentActiveActor != null && CurrentActiveActor.GetContext().IsPlayerControlled;
        }

        public int GetRound()
        {
            return turnCounter;
        }

        public bool IsCurrentActiveActor(ICombatant actor)
        {
            return _currentActiveActor == actor;
        }

        public List<ICombatant> GetCombatantsInTurnOrder()
        {
            var livingActors = LivingActors;
            if (livingActors.Count <= 0)
                return livingActors;
            int currentActorIndex = livingActors.IndexOf(CurrentActiveActor);
            ICombatant[] offsetCopy = ShiftLeft(livingActors.ToArray(), currentActorIndex);
            return offsetCopy.ToList();
        }

        private static ICombatant[] ShiftLeft(ICombatant[] array, int shiftAmount)
        {
            int length = array.Length;
            ICombatant[] result = new ICombatant[array.Length];
            // Ensure shiftAmount is within the _range of the array length
            shiftAmount %= length;

            // Shift indices to the left by X positions
            for (int i = 0; i < length; i++)
            {
                int newIndex = (i - shiftAmount + length) % length;
                result[newIndex] = array[i];
            }
            return result;
        }
    }
}
