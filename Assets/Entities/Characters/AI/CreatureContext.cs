﻿using UnityEngine;
using Apex.AI;
using Apex;
using System.Collections.Generic;
using Entities.Characters;
using Entities.Cover;
using SunsetSystems.Game;

public sealed class CreatureContext : IAIContext
{
    public CreatureContext(Creature owner)
    {
        Owner = owner;
    }

    public Creature Owner { get; private set; }

    public Creature CurrentTarget { get; set; }

    public GridElement CurrentMoveTarget { get; set; }

    public List<Cover> CoverSourcesInCombatGrid => GameManager.Instance.GetGridController().CoverSourcesInGrid;

    public bool IsInCombat => GameManager.Instance.IsCurrentState(GameState.Combat);

    public bool IsPlayerControlled => Owner.Data.Faction.Equals(Faction.PlayerControlled);

    public bool HasMoved => Behaviour.HasMoved;
    public bool HasActed => Behaviour.HasActed;

    public CombatBehaviour Behaviour => Owner.GetComponent<CombatBehaviour>();

    public StatsManager StatsManager => Owner.GetComponent<StatsManager>();

    public CharacterStats Stats => StatsManager.Stats;

    public List<GridElement> PositionsInRange => GameManager.Instance.GetGridController().GetElementsInRangeOfActor(Owner);

    public List<Creature> OtherCombatants => TurnCombatManager.Instance.GetCreaturesInCombat().FindAll(c => !c.Equals(Owner));

    public List<Creature> PlayerControlledCombatants => TurnCombatManager.Instance.GetCreaturesInCombat().FindAll(c => c.Data.Faction.Equals(Faction.PlayerControlled));

    public List<Creature> FriendlyCombatants => TurnCombatManager.Instance
        .GetCreaturesInCombat()
        .FindAll(c => c.Data.Faction.Equals(Faction.Friendly));

    public List<Creature> EnemyCombatants => TurnCombatManager.Instance
        .GetCreaturesInCombat()
        .FindAll(c => c.Data.Faction.Equals(Faction.Hostile));
}
