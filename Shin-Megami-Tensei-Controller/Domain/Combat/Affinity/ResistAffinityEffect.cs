using Shin_Megami_Tensei.Domain.Constants;
using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Combat.Affinity;

public class ResistAffinityEffect : IAffinityEffect
{
    public int CalculateDamage(double baseDamage)
    {
        return (int)Math.Floor(baseDamage * GameConstants.Combat.ResistMultiplier);
    }

    public void ApplyEffect(Unit attacker, Unit target, int calculatedDamage)
    {
        target.TakeDamage(calculatedDamage);
    }

    public TurnCost GetTurnCost(bool isMiss)
    {
        return TurnCost.ConsumeOneOfEither();
    }

    public bool CanMiss() => false;
}

