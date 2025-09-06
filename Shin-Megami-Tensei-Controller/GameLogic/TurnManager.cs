using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.GameLogic;

public class TurnManager
{
    public int FullTurns { get; private set; }
    public int BlinkingTurns { get; private set; }
    private Queue<Unit> _actionOrder = new();
    private Team? _currentTeam;

    public void InitializePlayerTurn(Team team)
    {
        _currentTeam = team;
        var activeUnits = team.GetActiveUnitsOnBoard();
        FullTurns = activeUnits.Count;
        BlinkingTurns = 0;
        
        SetupActionOrder(activeUnits);
    }

    private void SetupActionOrder(List<Unit> units)
    {
        // Ordenar por velocidad descendente, luego por posición en el tablero (izquierda a derecha) para empates
        var orderedUnits = units
            .OrderByDescending(u => u.BaseStats.Spd)
            .ThenBy(u => GetBoardPosition(u))
            .ToList();

        _actionOrder = new Queue<Unit>(orderedUnits);
    }

    private int GetBoardPosition(Unit unit)
    {
        // Obtener la posición real del tablero
        if (_currentTeam != null)
        {
            for (int i = 0; i < _currentTeam.Board.Length; i++)
            {
                if (_currentTeam.Board[i] == unit)
                {
                    return i;
                }
            }
        }
        
        // Fallback: usar hash del nombre
        return unit.Name.GetHashCode();
    }

    public Unit? GetNextActingUnit()
    {
        while (_actionOrder.Count > 0)
        {
            var unit = _actionOrder.Dequeue();
            if (unit.IsAlive)
            {
                // Poner la unidad de vuelta al final de la cola
                _actionOrder.Enqueue(unit);
                return unit;
            }
        }
        return null;
    }

    public bool HasTurnsRemaining()
    {
        return FullTurns > 0 || BlinkingTurns > 0;
    }

    public void ConsumeBasicActionTurn()
    {
        // Para E1, solo implementamos el consumo básico de turnos
        // Para ataques en escenarios neutrales, consumir 1 Full Turn
        if (BlinkingTurns > 0)
        {
            BlinkingTurns--;
        }
        else if (FullTurns > 0)
        {
            FullTurns--;
        }
    }

    public List<Unit> GetCurrentActionOrder()
    {
        return _actionOrder.ToList();
    }

    public void RemoveUnitFromOrder(Unit unit)
    {
        var newOrder = _actionOrder.Where(u => u != unit).ToList();
        _actionOrder = new Queue<Unit>(newOrder);
    }

    public void ResetActionOrder(List<Unit> units)
    {
        SetupActionOrder(units);
    }
}
