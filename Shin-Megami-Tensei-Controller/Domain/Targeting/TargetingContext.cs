namespace Shin_Megami_Tensei.Domain.Targeting;

using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.GameLogic;

public class TargetingContext
{
    public Unit Attacker { get; }
    public Team AttackerTeam { get; }
    public Team DefenderTeam { get; }
    public bool IncludeAttacker { get; }

    public TargetingContext(Unit attacker, Team attackerTeam, Team defenderTeam, bool includeAttacker = false)
    {
        Attacker = attacker;
        AttackerTeam = attackerTeam;
        DefenderTeam = defenderTeam;
        IncludeAttacker = includeAttacker;
    }

    public List<Unit> GetOrderedTargets()
    {
        var targets = new List<Unit>();
        
        // 1. Tablero del oponente (de izquierda a derecha)
        foreach (var unit in DefenderTeam.Board)
        {
            if (unit != null && unit.IsAlive)
            {
                targets.Add(unit);
            }
        }
        
        // 2. Reserva del oponente (orden en el archivo)
        foreach (var unit in DefenderTeam.Reserve)
        {
            if (unit != null && unit.IsAlive)
            {
                targets.Add(unit);
            }
        }
        
        // 3. Tablero del atacante (de izquierda a derecha, excluyendo atacante)
        foreach (var unit in AttackerTeam.Board)
        {
            if (unit != null && unit != Attacker && unit.IsAlive)
            {
                targets.Add(unit);
            }
        }
        
        // 4. Reserva del atacante (orden en el archivo)
        foreach (var unit in AttackerTeam.Reserve)
        {
            if (unit != null && unit != Attacker && unit.IsAlive)
            {
                targets.Add(unit);
            }
        }
        
        // 5. Atacante (si está incluido)
        if (IncludeAttacker && Attacker.IsAlive)
        {
            targets.Add(Attacker);
        }
        
        return targets;
    }

    public List<Unit> GetAllyTargets()
    {
        var targets = new List<Unit>();
        
        // Tablero del atacante (de izquierda a derecha)
        foreach (var unit in AttackerTeam.Board)
        {
            if (unit != null && unit.IsAlive)
            {
                targets.Add(unit);
            }
        }
        
        // Reserva del atacante (orden en el archivo)
        foreach (var unit in AttackerTeam.Reserve)
        {
            if (unit != null && unit.IsAlive)
            {
                targets.Add(unit);
            }
        }
        
        return targets;
    }
}

