using Shin_Megami_Tensei.Domain.Combat.Affinity;
using Shin_Megami_Tensei.Domain.Combat.InstantKill;
using Shin_Megami_Tensei.Domain.Enums;
using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Combat;

public class CombatResolver
{
    private readonly DamageCalculator _damageCalculator;

    public CombatResolver()
    {
        _damageCalculator = new DamageCalculator();
    }

    public AttackOutcome ResolveAttack(
        Unit attacker, 
        Unit target, 
        ElementType element, 
        int? skillPower = null)
    {
        string targetAffinityString = target.Affinity.GetAffinityFor(element.ToGameString());
        AffinityType affinityType = AffinityTypeExtensions.FromString(targetAffinityString);

        if (element.IsInstantKillElement())
        {
            return ResolveInstantKillAttack(attacker, target, affinityType, skillPower ?? 0);
        }

        return ResolveRegularAttack(attacker, target, element, affinityType, skillPower);
    }

    private AttackOutcome ResolveInstantKillAttack(
        Unit attacker,
        Unit target,
        AffinityType affinity,
        int skillPower)
    {
        IInstantKillStrategy strategy = InstantKillStrategyFactory.Create(affinity);
        bool isSuccessful = strategy.TryExecute(attacker, target, skillPower);
        TurnCost turnCost = isSuccessful ? strategy.GetSuccessTurnCost() : strategy.GetFailureTurnCost();

        bool isNullified = affinity == AffinityType.Null;
        bool isMissed = !isSuccessful && !isNullified && affinity != AffinityType.Repel;
        bool isRepelled = affinity == AffinityType.Repel;

        return new AttackOutcome(
            damageDealt: 0,
            affinityEffect: affinity,
            isRepelled: isRepelled,
            isDrained: false,
            isNullified: isNullified,
            isMissed: isMissed,
            isInstantKill: isSuccessful,
            attackerName: attacker.Name,
            turnCost: turnCost
        );
    }

    private AttackOutcome ResolveRegularAttack(
        Unit attacker,
        Unit target,
        ElementType element,
        AffinityType affinity,
        int? skillPower)
    {
        double baseDamage = _damageCalculator.CalculateBaseDamage(attacker, element, skillPower);
        IAffinityEffect affinityEffect = AffinityEffectFactory.Create(affinity);

        int calculatedDamage = affinityEffect.CalculateDamage(baseDamage);
        affinityEffect.ApplyEffect(attacker, target, calculatedDamage);
        TurnCost turnCost = affinityEffect.GetTurnCost(false);

        return new AttackOutcome(
            damageDealt: calculatedDamage,
            affinityEffect: affinity,
            isRepelled: affinity == AffinityType.Repel,
            isDrained: affinity == AffinityType.Drain,
            isNullified: affinity == AffinityType.Null,
            isMissed: false,
            isInstantKill: false,
            attackerName: attacker.Name,
            turnCost: turnCost
        );
    }
}

