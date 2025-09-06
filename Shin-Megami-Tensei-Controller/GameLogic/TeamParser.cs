using Shin_Megami_Tensei.Data;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.GameLogic;

public class TeamParser
{
    private readonly DataLoader _dataLoader;

    public TeamParser(DataLoader dataLoader)
    {
        _dataLoader = dataLoader;
    }

    public (Team player1Team, Team player2Team) ParseTeamsFromFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        
        var player1Units = new List<string>();
        var player2Units = new List<string>();
        var currentPlayer = 0;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            if (trimmedLine == "Player 1 Team")
            {
                currentPlayer = 1;
                continue;
            }
            
            if (trimmedLine == "Player 2 Team")
            {
                currentPlayer = 2;
                continue;
            }

            if (!string.IsNullOrEmpty(trimmedLine))
            {
                if (currentPlayer == 1)
                    player1Units.Add(trimmedLine);
                else if (currentPlayer == 2)
                    player2Units.Add(trimmedLine);
            }
        }

        var team1 = ParseTeam("J1", player1Units);
        var team2 = ParseTeam("J2", player2Units);

        return (team1, team2);
    }

    private Team ParseTeam(string playerLabel, List<string> unitLines)
    {
        Samurai? samurai = null;
        var monsters = new List<Monster>();

        foreach (var unitLine in unitLines)
        {
            if (unitLine.StartsWith("[Samurai]"))
            {
                samurai = ParseSamurai(unitLine);
            }
            else
            {
                var monster = ParseMonster(unitLine);
                if (monster != null)
                    monsters.Add(monster);
            }
        }

        if (samurai == null)
            throw new InvalidOperationException("El equipo debe tener exactamente un samurai");

        return new Team(GetPlayerName(samurai.Name), samurai, monsters);
    }

    private Samurai ParseSamurai(string samuraiLine)
    {
        // Formato: [Samurai] Nombre (Habilidad1,Habilidad2,...)
        var parts = samuraiLine.Substring("[Samurai] ".Length).Split('(');
        var samuraiName = parts[0].Trim();
        
        var samurai = _dataLoader.GetSamurai(samuraiName);
        if (samurai == null)
            throw new InvalidOperationException($"Samurai '{samuraiName}' no encontrado en la base de datos");

        // Parsear habilidades si están presentes
        if (parts.Length > 1)
        {
            var skillsPart = parts[1].TrimEnd(')');
            var skillNames = skillsPart.Split(',').Select(s => s.Trim()).ToList();
            
            foreach (var skillName in skillNames)
            {
                var skill = _dataLoader.GetSkill(skillName);
                if (skill != null)
                {
                    samurai.AddSkill(skill);
                }
            }
        }

        return samurai;
    }

    private Monster? ParseMonster(string monsterLine)
    {
        var monsterName = monsterLine.Trim();
        return _dataLoader.GetMonster(monsterName);
    }

    private string GetPlayerName(string samuraiName)
    {
        // Extraer nombre del jugador del nombre del samurai para propósitos de visualización
        return samuraiName;
    }

    public bool IsValidTeamFile(string filePath)
    {
        try
        {
            ParseTeamsFromFile(filePath);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
