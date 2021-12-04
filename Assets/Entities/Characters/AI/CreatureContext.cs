﻿using UnityEngine;
using Apex.AI;
using Apex;
using System.Collections.Generic;
using SunsetSystems.Management;
using Entities.Characters;
using Entities.Cover;

public sealed class CreatureContext : IAIContext
{
    public CreatureContext(Creature owner) 
    {
        Owner = owner;
    }

    public Creature Owner { get; private set; }

    public Creature CurrentTarget { get; set; }

    public GridElement CurrentMoveTarget { get; set; }

    public List<Cover> CoverSourcesInCombatGrid => GameManager.GetGridController().CoverSourcesInGrid;

    public bool IsInCombat => StateManager.GetCurrentState().Equals(GameState.Combat);

    public bool IsPlayerControlled => Owner.Faction.Equals(Faction.Player);

    public bool HasMoved => Behaviour.HasMoved;
    public bool HasActed => Behaviour.HasActed;

    public CombatBehaviour Behaviour => Owner.GetComponent<CombatBehaviour>();

    public StatsManager StatsManager => Owner.GetComponent<StatsManager>();

    public CharacterStats Stats => StatsManager.Stats;

    public List<GridElement> PositionsInRange => GameManager.GetGridController().GetElementsInRangeOfActor(Owner);

    public List<Creature> OtherCombatants => ReferenceManager.GetManager<TurnCombatManager>().GetCreaturesInCombat().FindAll(c => !c.Equals(Owner));

    public List<Creature> PlayerControlledCombatants => ReferenceManager.GetManager<TurnCombatManager>().GetCreaturesInCombat().FindAll(c => c.Faction.Equals(Faction.Player));

    public List<Creature> FriendlyCombatants => ReferenceManager.GetManager<TurnCombatManager>()
        .GetCreaturesInCombat()
        .FindAll(c => c.Faction.Equals(Faction.Friendly));

    public List<Creature> EnemyCombatants => ReferenceManager.GetManager<TurnCombatManager>()
        .GetCreaturesInCombat()
        .FindAll(c => c.Faction.Equals(Faction.Hostile));
}
