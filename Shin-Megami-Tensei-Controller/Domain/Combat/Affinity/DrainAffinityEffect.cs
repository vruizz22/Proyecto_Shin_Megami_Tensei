using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Combat.Affinity;

public class DrainAffinityEffect : IAffinityEffect
{
    public int CalculateDamage(double baseDamage)
    {
        return (int)Math.Floor(baseDamage);
    }

    public void ApplyEffect(Unit attacker, Unit target, int calculatedDamage)
    {
        target.Heal(calculatedDamage);
    }

    public TurnCost GetTurnCost(bool isMiss)
    {
        return TurnCost.ConsumeAll();
    }

    public bool CanMiss() => false;
}

