using Shin_Megami_Tensei.Domain.Combat;
using Shin_Megami_Tensei.Domain.Enums;
using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.GameLogic;

/// <summary>
/// Adaptador del BattleEngine original que utiliza el nuevo CombatResolver
/// Mantiene compatibilidad con el código existente
/// </summary>
public class RefactoredBattleEngine
{
    private readonly CombatResolver _combatResolver;

    public class AttackResult
    {
        public int Damage { get; set; }
        public string AffinityEffect { get; set; } = "-";
        public bool WasRepelled { get; set; }
        public bool WasDrained { get; set; }
        public bool WasNulled { get; set; }
        public bool Missed { get; set; }
        public bool InstantKill { get; set; }
        public string AttackerName { get; set; } = "";
        public TurnManager.TurnEffect TurnEffect { get; set; } = new();
    }

    public RefactoredBattleEngine()
    {
        _combatResolver = new CombatResolver();
    }

    public AttackResult ExecuteAttack(Unit attacker, Unit target, string attackType, int? skillPower = null)
    {
        ElementType element = ElementTypeExtensions.FromString(attackType);
        AttackOutcome outcome = _combatResolver.ResolveAttack(attacker, target, element, skillPower);

        return ConvertToLegacyResult(outcome);
    }

    private AttackResult ConvertToLegacyResult(AttackOutcome outcome)
    {
        return new AttackResult
        {
            Damage = outcome.DamageDealt,
            AffinityEffect = outcome.AffinityEffect.ToDisplayString(),
            WasRepelled = outcome.IsRepelled,
            WasDrained = outcome.IsDrained,
            WasNulled = outcome.IsNullified,
            Missed = outcome.IsMissed,
            InstantKill = outcome.IsInstantKill,
            AttackerName = outcome.AttackerName,
            TurnEffect = ConvertToLegacyTurnEffect(outcome.TurnCost)
        };
    }

    private TurnManager.TurnEffect ConvertToLegacyTurnEffect(TurnCost turnCost)
    {
        return new TurnManager.TurnEffect
        {
            FullTurnsConsumed = turnCost.FullTurnsConsumed,
            BlinkingTurnsConsumed = turnCost.BlinkingTurnsConsumed,
            BlinkingTurnsGained = turnCost.BlinkingTurnsGained,
            ConsumeAllTurns = turnCost.ConsumeAllTurns
        };
    }
}

