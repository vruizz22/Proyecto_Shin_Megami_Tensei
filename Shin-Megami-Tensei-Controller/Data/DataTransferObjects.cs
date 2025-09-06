namespace Shin_Megami_Tensei.Data;

public class SamuraiDto
{
    public string name { get; set; } = "";
    public StatsDto stats { get; set; } = new();
    public AffinityDto affinity { get; set; } = new();
}

public class MonsterDto
{
    public string name { get; set; } = "";
    public StatsDto stats { get; set; } = new();
    public AffinityDto affinity { get; set; } = new();
    public List<string> skills { get; set; } = new();
}

public class SkillDto
{
    public string name { get; set; } = "";
    public string type { get; set; } = "";
    public int cost { get; set; }
    public int power { get; set; }
    public string target { get; set; } = "";
    public string hits { get; set; } = "";
    public string effect { get; set; } = "";
}

public class StatsDto
{
    public int HP { get; set; }
    public int MP { get; set; }
    public int Str { get; set; }
    public int Skl { get; set; }
    public int Mag { get; set; }
    public int Spd { get; set; }
    public int Lck { get; set; }
}

public class AffinityDto
{
    public string Phys { get; set; } = "-";
    public string Gun { get; set; } = "-";
    public string Fire { get; set; } = "-";
    public string Ice { get; set; } = "-";
    public string Elec { get; set; } = "-";
    public string Force { get; set; } = "-";
    public string Light { get; set; } = "-";
    public string Dark { get; set; } = "-";
}
