using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Combat.InstantKill;

public class RepelInstantKillStrategy : IInstantKillStrategy
{
    public bool TryExecute(Unit attacker, Unit target, int skillPower)
    {
        attacker.TakeDamage(attacker.CurrentHP);
        return true;
    }

    public TurnCost GetSuccessTurnCost()
    {
        return TurnCost.ConsumeAll();
    }

    public TurnCost GetFailureTurnCost()
    {
        return TurnCost.ConsumeAll();
    }
}

