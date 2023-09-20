using SunsetSystems.Combat;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Entities.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SunsetSystems.Spellbook
{
    [CreateAssetMenu(fileName = "HealOnEndTurn", menuName = "Scriptable Powers/Heal On End Turn")]
    public class HealOnEndTurn : DisciplineScript
    {
        private List<ICombatant> _effectRecievers = new();

        private void OnEnable()
        {
            _effectRecievers = new();
            CombatManager.OnFullTurnCompleted += HealOnFullTurn;
        }

        private void OnDisable()
        {
            CombatManager.OnFullTurnCompleted -= HealOnFullTurn;
        }

        public override void Activate(ICombatant target, ICombatant caster)
        {
            _effectRecievers ??= new();
            _effectRecievers.RemoveAll(c => c == null);
            _effectRecievers.Add(caster);
        }

        private void HealOnFullTurn()
        {
            _effectRecievers.ForEach(c => DoHealing(c));
        }

        private void DoHealing(ICombatant creature)
        {
            StatsManager targetStatsManager = creature.References.GetComponentInChildren<StatsManager>();
            if (targetStatsManager != null && targetStatsManager.IsAlive())
            {
                int amount = UnityEngine.Random.Range(1, 3);
                targetStatsManager.Heal(amount);
            }
        }
    }
}
