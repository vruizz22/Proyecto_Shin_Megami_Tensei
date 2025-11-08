using Shin_Megami_Tensei.Domain.Enums;

namespace Shin_Megami_Tensei.Domain.ValueObjects;

public readonly struct AttackOutcome
{
    public int DamageDealt { get; }
    public AffinityType AffinityEffect { get; }
    public bool IsRepelled { get; }
    public bool IsDrained { get; }
    public bool IsNullified { get; }
    public bool IsMissed { get; }
    public bool IsInstantKill { get; }
    public string AttackerName { get; }
    public TurnCost TurnCost { get; }

    public AttackOutcome(
        int damageDealt,
        AffinityType affinityEffect,
        bool isRepelled,
        bool isDrained,
        bool isNullified,
        bool isMissed,
        bool isInstantKill,
        string attackerName,
        TurnCost turnCost)
    {
        DamageDealt = damageDealt;
        AffinityEffect = affinityEffect;
        IsRepelled = isRepelled;
        IsDrained = isDrained;
        IsNullified = isNullified;
        IsMissed = isMissed;
        IsInstantKill = isInstantKill;
        AttackerName = attackerName;
        TurnCost = turnCost;
    }

    public bool HasAnySpecialEffect() => IsRepelled || IsDrained || IsNullified || IsMissed || IsInstantKill;
}

