using Shin_Megami_Tensei.Domain.Constants;
using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Combat.Affinity;

public class WeakAffinityEffect : IAffinityEffect
{
    public int CalculateDamage(double baseDamage)
    {
        return (int)Math.Floor(baseDamage * GameConstants.Combat.WeakMultiplier);
    }

    public void ApplyEffect(Unit attacker, Unit target, int calculatedDamage)
    {
        target.TakeDamage(calculatedDamage);
    }

    public TurnCost GetTurnCost(bool isMiss)
    {
        if (isMiss)
            return TurnCost.ConsumeOneOfEither();
        
        return new TurnCost(fullTurnsConsumed: 1, blinkingTurnsConsumed: 0, blinkingTurnsGained: 1);
    }

    public bool CanMiss() => false;
}

