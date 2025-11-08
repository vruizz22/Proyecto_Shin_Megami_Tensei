using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Combat.Affinity;

public class NeutralAffinityEffect : IAffinityEffect
{
    public int CalculateDamage(double baseDamage)
    {
        return (int)Math.Floor(baseDamage);
    }

    public void ApplyEffect(Unit attacker, Unit target, int calculatedDamage)
    {
        target.TakeDamage(calculatedDamage);
    }

    public TurnCost GetTurnCost(bool isMiss)
    {
        if (isMiss)
            return TurnCost.ConsumeOneOfEither();
        
        return TurnCost.ConsumeOneOfEither();
    }

    public bool CanMiss() => false;
}

