namespace Shin_Megami_Tensei.Domain.Enums;

public enum ElementType
{
    Physical,
    Gun,
    Fire,
    Ice,
    Electric,
    Force,
    Light,
    Dark,
    Almighty
}

public static class ElementTypeExtensions
{
    public static ElementType FromString(string element)
    {
        return element switch
        {
            "Phys" => ElementType.Physical,
            "Gun" => ElementType.Gun,
            "Fire" => ElementType.Fire,
            "Ice" => ElementType.Ice,
            "Elec" => ElementType.Electric,
            "Force" => ElementType.Force,
            "Light" => ElementType.Light,
            "Dark" => ElementType.Dark,
            "Almighty" => ElementType.Almighty,
            _ => ElementType.Physical
        };
    }

    public static string ToGameString(this ElementType element)
    {
        return element switch
        {
            ElementType.Physical => "Phys",
            ElementType.Gun => "Gun",
            ElementType.Fire => "Fire",
            ElementType.Ice => "Ice",
            ElementType.Electric => "Elec",
            ElementType.Force => "Force",
            ElementType.Light => "Light",
            ElementType.Dark => "Dark",
            ElementType.Almighty => "Almighty",
            _ => "Phys"
        };
    }

    public static bool IsInstantKillElement(this ElementType element)
    {
        return element == ElementType.Light || element == ElementType.Dark;
    }
}

