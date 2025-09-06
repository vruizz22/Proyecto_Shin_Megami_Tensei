﻿using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.GameLogic;

public class Team
{
    public string PlayerName { get; private set; }
    public Samurai Samurai { get; private set; }
    public List<Monster> Monsters { get; private set; }
    public List<Unit> Reserve { get; private set; }
    public Unit?[] Board { get; private set; } // 4 posiciones

    public Team(string playerName, Samurai samurai, List<Monster> monsters)
    {
        PlayerName = playerName;
        Samurai = samurai;
        Monsters = monsters;
        Reserve = new List<Unit>();
        Board = new Unit?[4];
        
        ValidateTeam();
        SetupBoard();
    }

    private void ValidateTeam()
    {
        // Validar que hay exactamente un samurai (ya garantizado por constructor)
        
        // Validar que no hay más de 7 monstruos
        if (Monsters.Count > 7)
            throw new InvalidOperationException("Un equipo no puede tener más de 7 monstruos");

        // Validar que no hay más de 8 unidades en total
        if (1 + Monsters.Count > 8)
            throw new InvalidOperationException("Un equipo no puede tener más de 8 unidades");

        // Validar que no hay monstruos duplicados
        var monsterNames = Monsters.Select(m => m.Name).ToList();
        if (monsterNames.Count != monsterNames.Distinct().Count())
            throw new InvalidOperationException("Un equipo no puede tener monstruos duplicados");

        // Validar habilidades del samurai
        if (Samurai.Skills.Count > 8)
            throw new InvalidOperationException("Un samurai no puede tener más de 8 habilidades");

        var skillNames = Samurai.Skills.Select(s => s.Name).ToList();
        if (skillNames.Count != skillNames.Distinct().Count())
            throw new InvalidOperationException("Un samurai no puede tener habilidades duplicadas");
    }

    private void SetupBoard()
    {
        // El samurai siempre va en la posición 0 (más a la izquierda)
        Board[0] = Samurai;

        // Los primeros 3 monstruos van en las posiciones 1, 2, 3
        for (int i = 0; i < Math.Min(3, Monsters.Count); i++)
        {
            Board[i + 1] = Monsters[i];
        }

        // Los monstruos restantes van a la reserva
        for (int i = 3; i < Monsters.Count; i++)
        {
            Reserve.Add(Monsters[i]);
        }
    }

    public List<Unit> GetAliveUnitsOnBoard()
    {
        return Board.Where(unit => unit != null && unit.IsAlive).ToList()!;
    }

    public List<Unit> GetActiveUnitsOnBoard()
    {
        // Para samurai muerto, sigue en el tablero pero no puede actuar
        var activeUnits = new List<Unit>();
        
        for (int i = 0; i < Board.Length; i++)
        {
            var unit = Board[i];
            if (unit != null && unit.IsAlive)
            {
                activeUnits.Add(unit);
            }
        }
        
        return activeUnits;
    }

    public bool HasActiveUnits()
    {
        return GetActiveUnitsOnBoard().Any();
    }

    public void RemoveUnitFromBoard(Unit unit)
    {
        for (int i = 0; i < Board.Length; i++)
        {
            if (Board[i] == unit)
            {
                // Si es un monstruo muerto, va a la reserva y se libera el espacio
                if (unit is Monster && !unit.IsAlive)
                {
                    Reserve.Add(unit);
                    Board[i] = null;
                }
                // Si es un samurai muerto, permanece en el tablero
                break;
            }
        }
    }

    public string GetFormattedBoardState(string teamLabel)
    {
        var result = $"Equipo de {PlayerName} ({teamLabel})\n";
        
        for (int i = 0; i < 4; i++)
        {
            var position = (char)('A' + i);
            var unit = Board[i];
            
            if (unit == null)
            {
                result += $"{position}-\n";
            }
            else
            {
                result += $"{position}-{unit.Name} HP:{unit.CurrentHP}/{unit.BaseStats.HP} MP:{unit.CurrentMP}/{unit.BaseStats.MP}\n";
            }
        }
        
        return result.TrimEnd('\n');
    }
}
