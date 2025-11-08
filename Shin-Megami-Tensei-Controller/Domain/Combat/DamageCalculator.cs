using Shin_Megami_Tensei.Domain.Constants;
using Shin_Megami_Tensei.Domain.Enums;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Combat;

public class DamageCalculator
{
    public double CalculateBaseDamage(Unit attacker, ElementType element, int? skillPower)
    {
        int attackStat = GetAttackStat(attacker, element);

        if (skillPower.HasValue)
        {
            return Math.Sqrt(attackStat * skillPower.Value);
        }

        double modifier = element == ElementType.Gun 
            ? GameConstants.Combat.BaseGunModifier 
            : GameConstants.Combat.BaseAttackModifier;

        return attackStat * modifier * GameConstants.Combat.DamageConstant;
    }

    private int GetAttackStat(Unit attacker, ElementType element)
    {
        return element switch
        {
            ElementType.Physical => attacker.BaseStats.Str,
            ElementType.Gun => attacker.BaseStats.Skl,
            ElementType.Fire or ElementType.Ice or ElementType.Electric or ElementType.Force or ElementType.Almighty 
                => attacker.BaseStats.Mag,
            _ => attacker.BaseStats.Str
        };
    }
}

