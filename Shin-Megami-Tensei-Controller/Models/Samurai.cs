namespace Shin_Megami_Tensei.Models;

public class Samurai : Unit
{
    public Samurai(string name, Stats stats, Affinity affinity) : base(name, stats, affinity)
    {
    }

    public void AddSkill(Skill skill)
    {
        if (Skills.Count >= 8)
            throw new InvalidOperationException("Un samurai no puede tener más de 8 habilidades");
        
        if (Skills.Any(s => s.Name == skill.Name))
            throw new InvalidOperationException("Un samurai no puede tener habilidades duplicadas");
        
        Skills.Add(skill);
    }

    public bool CanAddSkill(Skill skill)
    {
        return Skills.Count < 8 && !Skills.Any(s => s.Name == skill.Name);
    }
}
