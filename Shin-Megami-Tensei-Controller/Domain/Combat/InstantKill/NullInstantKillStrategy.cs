using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Combat.InstantKill;

public class NullInstantKillStrategy : IInstantKillStrategy
{
    public bool TryExecute(Unit attacker, Unit target, int skillPower)
    {
        return false;
    }

    public TurnCost GetSuccessTurnCost()
    {
        return new TurnCost(fullTurnsConsumed: 0, blinkingTurnsConsumed: 2, blinkingTurnsGained: 0);
    }

    public TurnCost GetFailureTurnCost()
    {
        return new TurnCost(fullTurnsConsumed: 0, blinkingTurnsConsumed: 2, blinkingTurnsGained: 0);
    }
}

