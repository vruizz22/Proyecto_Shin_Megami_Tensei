using System.Text.Json;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Data;

public class DataLoader
{
    private readonly Dictionary<string, Samurai> _samuraiDatabase = new();
    private readonly Dictionary<string, Monster> _monsterDatabase = new();
    private readonly Dictionary<string, Skill> _skillDatabase = new();

    public void LoadGameData()
    {
        LoadSkills();
        LoadSamurai();
        LoadMonsters();
    }

    private void LoadSkills()
    {
        string skillsJson = File.ReadAllText("data/skills.json");
        var skillDtos = JsonSerializer.Deserialize<List<SkillDto>>(skillsJson) ?? new List<SkillDto>();

        foreach (var dto in skillDtos)
        {
            var skill = new Skill(dto.name, dto.type, dto.cost, dto.power, dto.target, dto.hits, dto.effect);
            _skillDatabase[dto.name] = skill;
        }
    }

    private void LoadSamurai()
    {
        string samuraiJson = File.ReadAllText("data/samurai.json");
        var samuraiDtos = JsonSerializer.Deserialize<List<SamuraiDto>>(samuraiJson) ?? new List<SamuraiDto>();

        foreach (var dto in samuraiDtos)
        {
            var stats = new Stats(dto.stats.HP, dto.stats.MP, dto.stats.Str, dto.stats.Skl, dto.stats.Mag, dto.stats.Spd, dto.stats.Lck);
            var affinity = ConvertAffinityDto(dto.affinity);
            var samurai = new Samurai(dto.name, stats, affinity);
            _samuraiDatabase[dto.name] = samurai;
        }
    }

    private void LoadMonsters()
    {
        string monstersJson = File.ReadAllText("data/monsters.json");
        var monsterDtos = JsonSerializer.Deserialize<List<MonsterDto>>(monstersJson) ?? new List<MonsterDto>();

        foreach (var dto in monsterDtos)
        {
            var stats = new Stats(dto.stats.HP, dto.stats.MP, dto.stats.Str, dto.stats.Skl, dto.stats.Mag, dto.stats.Spd, dto.stats.Lck);
            var affinity = ConvertAffinityDto(dto.affinity);
            var skills = dto.skills.Select(skillName => GetSkill(skillName)).Where(skill => skill != null).ToList()!;
            var monster = new Monster(dto.name, stats, affinity, skills);
            _monsterDatabase[dto.name] = monster;
        }
    }

    private Affinity ConvertAffinityDto(AffinityDto dto)
    {
        return new Affinity
        {
            Phys = dto.Phys,
            Gun = dto.Gun,
            Fire = dto.Fire,
            Ice = dto.Ice,
            Elec = dto.Elec,
            Force = dto.Force,
            Light = dto.Light,
            Dark = dto.Dark
        };
    }

    public Samurai? GetSamurai(string name)
    {
        if (_samuraiDatabase.TryGetValue(name, out var samurai))
        {
            // Retornar una copia para evitar modificar el original
            var stats = new Stats(samurai.BaseStats.HP, samurai.BaseStats.MP, samurai.BaseStats.Str, 
                                 samurai.BaseStats.Skl, samurai.BaseStats.Mag, samurai.BaseStats.Spd, samurai.BaseStats.Lck);
            var affinity = new Affinity
            {
                Phys = samurai.Affinity.Phys,
                Gun = samurai.Affinity.Gun,
                Fire = samurai.Affinity.Fire,
                Ice = samurai.Affinity.Ice,
                Elec = samurai.Affinity.Elec,
                Force = samurai.Affinity.Force,
                Light = samurai.Affinity.Light,
                Dark = samurai.Affinity.Dark
            };
            return new Samurai(samurai.Name, stats, affinity);
        }
        return null;
    }

    public Monster? GetMonster(string name)
    {
        if (_monsterDatabase.TryGetValue(name, out var monster))
        {
            // Retornar una copia para evitar modificar el original
            var stats = new Stats(monster.BaseStats.HP, monster.BaseStats.MP, monster.BaseStats.Str, 
                                 monster.BaseStats.Skl, monster.BaseStats.Mag, monster.BaseStats.Spd, monster.BaseStats.Lck);
            var affinity = new Affinity
            {
                Phys = monster.Affinity.Phys,
                Gun = monster.Affinity.Gun,
                Fire = monster.Affinity.Fire,
                Ice = monster.Affinity.Ice,
                Elec = monster.Affinity.Elec,
                Force = monster.Affinity.Force,
                Light = monster.Affinity.Light,
                Dark = monster.Affinity.Dark
            };
            var skills = monster.Skills.Select(s => new Skill(s.Name, s.Type, s.Cost, s.Power, s.Target, s.Hits, s.Effect)).ToList();
            return new Monster(monster.Name, stats, affinity, skills);
        }
        return null;
    }

    public Skill? GetSkill(string name)
    {
        if (_skillDatabase.TryGetValue(name, out var skill))
        {
            return new Skill(skill.Name, skill.Type, skill.Cost, skill.Power, skill.Target, skill.Hits, skill.Effect);
        }
        return null;
    }
}
