namespace Shin_Megami_Tensei.Models;

public class Monster : Unit
{
    public Monster(string name, Stats stats, Affinity affinity, List<Skill> predefinedSkills) : base(name, stats, affinity)
    {
        Skills = predefinedSkills ?? new List<Skill>();
    }
}
