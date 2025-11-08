using Shin_Megami_Tensei.Domain.Enums;

namespace Shin_Megami_Tensei.Domain.Combat.Affinity;

public class AffinityEffectFactory
{
    private static readonly NeutralAffinityEffect _neutral = new();
    private static readonly WeakAffinityEffect _weak = new();
    private static readonly ResistAffinityEffect _resist = new();
    private static readonly NullAffinityEffect _null = new();
    private static readonly RepelAffinityEffect _repel = new();
    private static readonly DrainAffinityEffect _drain = new();

    public static IAffinityEffect Create(AffinityType affinityType)
    {
        return affinityType switch
        {
            AffinityType.Weak => _weak,
            AffinityType.Resist => _resist,
            AffinityType.Null => _null,
            AffinityType.Repel => _repel,
            AffinityType.Drain => _drain,
            _ => _neutral
        };
    }
}

