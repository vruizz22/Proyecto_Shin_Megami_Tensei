using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Combat.InstantKill;

public class WeakInstantKillStrategy : IInstantKillStrategy
{
    public bool TryExecute(Unit attacker, Unit target, int skillPower)
    {
        target.TakeDamage(target.CurrentHP);
        return true;
    }

    public TurnCost GetSuccessTurnCost()
    {
        return new TurnCost(fullTurnsConsumed: 1, blinkingTurnsConsumed: 0, blinkingTurnsGained: 1);
    }

    public TurnCost GetFailureTurnCost()
    {
        return TurnCost.ConsumeOneOfEither();
    }
}

