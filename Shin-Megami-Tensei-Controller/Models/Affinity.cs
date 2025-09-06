namespace Shin_Megami_Tensei.Models;

public class Affinity
{
    public string Phys { get; set; } = "-";
    public string Gun { get; set; } = "-";
    public string Fire { get; set; } = "-";
    public string Ice { get; set; } = "-";
    public string Elec { get; set; } = "-";
    public string Force { get; set; } = "-";
    public string Light { get; set; } = "-";
    public string Dark { get; set; } = "-";

    public string GetAffinityFor(string element)
    {
        return element switch
        {
            "Phys" => Phys,
            "Gun" => Gun,
            "Fire" => Fire,
            "Ice" => Ice,
            "Elec" => Elec,
            "Force" => Force,
            "Light" => Light,
            "Dark" => Dark,
            _ => "-"
        };
    }
}
