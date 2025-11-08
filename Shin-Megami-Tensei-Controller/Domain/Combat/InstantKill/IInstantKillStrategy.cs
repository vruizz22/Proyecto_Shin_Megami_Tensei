using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Combat.InstantKill;

public interface IInstantKillStrategy
{
    bool TryExecute(Unit attacker, Unit target, int skillPower);
    TurnCost GetSuccessTurnCost();
    TurnCost GetFailureTurnCost();
}

