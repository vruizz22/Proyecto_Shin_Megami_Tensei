using Shin_Megami_Tensei.Domain.Enums;

namespace Shin_Megami_Tensei.Domain.Combat.InstantKill;

public class InstantKillStrategyFactory
{
    private static readonly WeakInstantKillStrategy _weak = new();
    private static readonly ResistInstantKillStrategy _resist = new();
    private static readonly NeutralInstantKillStrategy _neutral = new();
    private static readonly RepelInstantKillStrategy _repel = new();
    private static readonly NullInstantKillStrategy _null = new();

    public static IInstantKillStrategy Create(AffinityType affinityType)
    {
        return affinityType switch
        {
            AffinityType.Weak => _weak,
            AffinityType.Resist => _resist,
            AffinityType.Repel => _repel,
            AffinityType.Null => _null,
            _ => _neutral
        };
    }
}

