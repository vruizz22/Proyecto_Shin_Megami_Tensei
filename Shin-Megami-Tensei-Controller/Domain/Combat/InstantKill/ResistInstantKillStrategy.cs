using Shin_Megami_Tensei.Domain.Constants;
using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Combat.InstantKill;

public class ResistInstantKillStrategy : IInstantKillStrategy
{
    public bool TryExecute(Unit attacker, Unit target, int skillPower)
    {
        bool isSuccessful = CalculateSuccess(attacker, target, skillPower);
        
        if (isSuccessful)
        {
            target.TakeDamage(target.CurrentHP);
        }
        
        return isSuccessful;
    }

    private bool CalculateSuccess(Unit attacker, Unit target, int skillPower)
    {
        return attacker.BaseStats.Lck + skillPower >= 
               GameConstants.Combat.InstantKillLuckMultiplier * target.BaseStats.Lck;
    }

    public TurnCost GetSuccessTurnCost()
    {
        return TurnCost.ConsumeOneOfEither();
    }

    public TurnCost GetFailureTurnCost()
    {
        return TurnCost.ConsumeOneOfEither();
    }
}

