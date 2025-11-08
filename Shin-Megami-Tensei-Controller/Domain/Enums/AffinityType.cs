namespace Shin_Megami_Tensei.Domain.Enums;

public enum AffinityType
{
    Neutral,  // "-"
    Weak,     // "Wk"
    Resist,   // "Rs"
    Null,     // "Nu"
    Repel,    // "Rp"
    Drain     // "Dr"
}

public static class AffinityTypeExtensions
{
    public static AffinityType FromString(string affinity)
    {
        return affinity switch
        {
            "Wk" => AffinityType.Weak,
            "Rs" => AffinityType.Resist,
            "Nu" => AffinityType.Null,
            "Rp" => AffinityType.Repel,
            "Dr" => AffinityType.Drain,
            _ => AffinityType.Neutral
        };
    }

    public static string ToDisplayString(this AffinityType type)
    {
        return type switch
        {
            AffinityType.Weak => "Wk",
            AffinityType.Resist => "Rs",
            AffinityType.Null => "Nu",
            AffinityType.Repel => "Rp",
            AffinityType.Drain => "Dr",
            _ => "-"
        };
    }
}

