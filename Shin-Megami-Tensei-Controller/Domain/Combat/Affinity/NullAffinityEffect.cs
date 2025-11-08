using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Combat.Affinity;

public class NullAffinityEffect : IAffinityEffect
{
    public int CalculateDamage(double baseDamage)
    {
        return 0;
    }

    public void ApplyEffect(Unit attacker, Unit target, int calculatedDamage)
    {
        // Sin efecto
    }

    public TurnCost GetTurnCost(bool isMiss)
    {
        return new TurnCost(fullTurnsConsumed: 0, blinkingTurnsConsumed: 2, blinkingTurnsGained: 0);
    }

    public bool CanMiss() => false;
}

