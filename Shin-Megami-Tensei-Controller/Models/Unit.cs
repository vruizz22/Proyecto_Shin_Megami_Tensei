namespace Shin_Megami_Tensei.Models;

public abstract class Unit
{
    public string Name { get; set; }
    public Stats BaseStats { get; set; }
    public int CurrentHP { get; set; }
    public int CurrentMP { get; set; }
    public Affinity Affinity { get; set; }
    public List<Skill> Skills { get; set; }

    protected Unit(string name, Stats stats, Affinity affinity)
    {
        Name = name;
        BaseStats = stats;
        CurrentHP = stats.HP;
        CurrentMP = stats.MP;
        Affinity = affinity;
        Skills = new List<Skill>();
    }

    public bool IsAlive => CurrentHP > 0;

    public void TakeDamage(int damage)
    {
        CurrentHP = Math.Max(0, CurrentHP - damage);
    }

    public void Heal(int amount)
    {
        CurrentHP = Math.Min(BaseStats.HP, CurrentHP + amount);
    }

    public void ConsumeMP(int amount)
    {
        CurrentMP = Math.Max(0, CurrentMP - amount);
    }

    public int GetAttackStat(string damageType)
    {
        return damageType switch
        {
            "Phys" => BaseStats.Str,
            "Gun" => BaseStats.Skl,
            "Fire" or "Ice" or "Elec" or "Force" or "Almighty" => BaseStats.Mag,
            _ => BaseStats.Str
        };
    }

    public List<Skill> GetUsableSkills()
    {
        return Skills.Where(skill => skill.CanBeUsedBy(this)).ToList();
    }
}
