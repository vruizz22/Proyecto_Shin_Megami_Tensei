namespace Shin_Megami_Tensei.Domain.Combat;

using Models;
using System;

public class StatDrainEffect
{
    public int HPDrained { get; }
    public int MPDrained { get; }
    public bool DrainsHP { get; }
    public bool DrainsMP { get; }

    public StatDrainEffect(int hpDrained, int mpDrained, bool drainsHP, bool drainsMP)
    {
        HPDrained = hpDrained;
        MPDrained = mpDrained;
        DrainsHP = drainsHP;
        DrainsMP = drainsMP;
    }

    public static StatDrainEffect CalculateDrain(Unit attacker, Unit target, int damage, string effectType)
    {
        bool drainsHP = effectType.Contains("HP");
        bool drainsMP = effectType.Contains("MP");
        
        int hpDrained = 0;
        int mpDrained = 0;

        if (drainsHP)
        {
            hpDrained = Math.Min(damage, target.CurrentHP);
            int hpToRestore = Math.Min(hpDrained, attacker.BaseStats.HP - attacker.CurrentHP);
            attacker.Heal(hpToRestore);
        }

        if (drainsMP)
        {
            mpDrained = Math.Min(damage, target.CurrentMP);
            int mpToRestore = Math.Min(mpDrained, attacker.BaseStats.MP - attacker.CurrentMP);
            attacker.RestoreMP(mpToRestore);
            target.ConsumeMP(mpDrained);
        }

        return new StatDrainEffect(hpDrained, mpDrained, drainsHP, drainsMP);
    }
}

