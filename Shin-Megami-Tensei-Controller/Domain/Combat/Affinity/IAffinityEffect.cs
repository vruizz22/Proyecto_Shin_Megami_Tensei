using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Combat.Affinity;

public interface IAffinityEffect
{
    int CalculateDamage(double baseDamage);
    void ApplyEffect(Unit attacker, Unit target, int calculatedDamage);
    TurnCost GetTurnCost(bool isMiss);
    bool CanMiss();
}

