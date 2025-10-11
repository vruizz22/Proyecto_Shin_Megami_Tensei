using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.GameLogic;

public class TurnManager
{
    public class TurnEffect
    {
        public int FullTurnsConsumed { get; set; }
        public int BlinkingTurnsConsumed { get; set; }
        public int BlinkingTurnsGained { get; set; }
        public bool ConsumeAllTurns { get; set; }
    }

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
        // Si no hay unidades en la cola de acción, no hay nada que hacer
        if (_actionOrder.Count == 0)
            return null;
            
        // Buscar la siguiente unidad viva
        int attempts = 0;
        while (_actionOrder.Count > 0 && attempts < _actionOrder.Count)
        {
            var unit = _actionOrder.Dequeue();
            if (unit.IsAlive)
            {
                // Poner la unidad de vuelta al final de la cola solo si sigue viva
                _actionOrder.Enqueue(unit);
                return unit;
            }
            attempts++;
        }
        
        // Si llegamos aquí, no hay unidades vivas
        return null;
    }

    public bool HasTurnsRemaining()
    {
        // Verificar si hay turnos disponibles Y unidades vivas que puedan actuar
        bool hasTurns = FullTurns > 0 || BlinkingTurns > 0;
        bool hasAliveUnits = _actionOrder.Any(unit => unit.IsAlive);
        
        return hasTurns && hasAliveUnits;
    }

    public void ConsumeTurns(TurnEffect effect)
    {
        if (effect.ConsumeAllTurns)
        {
            FullTurns = 0;
            BlinkingTurns = 0;
            return;
        }

        int blinkingToConsume = effect.BlinkingTurnsConsumed;
        int fullToConsume = effect.FullTurnsConsumed;

        if (blinkingToConsume > 0)
        {
            int consumed = Math.Min(BlinkingTurns, blinkingToConsume);
            BlinkingTurns -= consumed;
            blinkingToConsume -= consumed;
            
            if (blinkingToConsume > 0)
            {
                int fromFull = Math.Min(FullTurns, blinkingToConsume);
                FullTurns -= fromFull;
            }
        }

        if (fullToConsume > 0)
        {
            int consumed = Math.Min(FullTurns, fullToConsume);
            FullTurns -= consumed;
        }

        BlinkingTurns += effect.BlinkingTurnsGained;
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

    public void AddUnitToOrder(Unit unit)
    {
        var currentOrder = _actionOrder.ToList();
        currentOrder.Add(unit);
        _actionOrder = new Queue<Unit>(currentOrder);
    }

    public void ReplaceUnitInOrder(Unit oldUnit, Unit newUnit)
    {
        var currentOrder = _actionOrder.ToList();
        int index = currentOrder.IndexOf(oldUnit);
        if (index >= 0)
        {
            currentOrder[index] = newUnit;
        }
        _actionOrder = new Queue<Unit>(currentOrder);
    }
}
