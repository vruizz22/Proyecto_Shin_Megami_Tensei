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
            
        // Obtener las unidades activas en el tablero
        var activeUnits = _currentTeam?.GetActiveUnitsOnBoard() ?? new List<Unit>();
        
        // Primero, limpiar la cola de unidades que no pueden actuar
        var validUnits = new List<Unit>();
        while (_actionOrder.Count > 0)
        {
            var unit = _actionOrder.Dequeue();
            // Solo mantener unidades que estén vivas Y en el tablero activo
            if (unit.IsAlive && activeUnits.Contains(unit))
            {
                validUnits.Add(unit);
            }
        }
        
        // Reconstruir la cola con solo las unidades válidas
        _actionOrder = new Queue<Unit>(validUnits);
        
        // Si no hay unidades válidas, retornar null
        if (_actionOrder.Count == 0)
            return null;
            
        // Obtener la siguiente unidad pero NO moverla al final aún
        // Se moverá después de que actúe
        var actingUnit = _actionOrder.Dequeue();
        
        return actingUnit;
    }
    
    public void MoveUnitToEndOfOrder(Unit unit)
    {
        // Agregar la unidad al final de la cola después de que haya actuado
        _actionOrder.Enqueue(unit);
    }

    public bool HasTurnsRemaining()
    {
        // Verificar si hay turnos disponibles
        bool hasTurns = FullTurns > 0 || BlinkingTurns > 0;
        
        if (!hasTurns)
            return false;
        
        // Si no hay unidades en la cola, no hay turnos disponibles
        if (_actionOrder.Count == 0)
            return false;
        
        // Verificar si hay unidades vivas Y en el tablero activo que puedan actuar
        if (_currentTeam != null)
        {
            var activeUnits = _currentTeam.GetActiveUnitsOnBoard();
            // Hacer una copia de la cola para no modificarla
            var queueCopy = _actionOrder.ToList();
            return queueCopy.Any(unit => unit.IsAlive && activeUnits.Contains(unit));
        }
        
        return false;
    }

    public TurnEffect ConsumeTurns(TurnEffect effect)
    {
        var actualEffect = new TurnEffect();
        
        if (effect.ConsumeAllTurns)
        {
            actualEffect.FullTurnsConsumed = FullTurns;
            actualEffect.BlinkingTurnsConsumed = BlinkingTurns;
            FullTurns = 0;
            BlinkingTurns = 0;
            return actualEffect;
        }

        // Caso especial: Pasar Turno / Invocar (Samurai)
        // cuando se pide consumir 1 Full + 1 Blinking + otorgar 1 Blinking
        // Significa: "consume 1 Blinking si hay (y no otorga nada), sino consume 1 Full y otorga 1 Blinking"
        if (effect.FullTurnsConsumed == 1 && effect.BlinkingTurnsConsumed == 1 && effect.BlinkingTurnsGained == 1)
        {
            if (BlinkingTurns > 0)
            {
                BlinkingTurns -= 1;
                actualEffect.BlinkingTurnsConsumed = 1;
                actualEffect.FullTurnsConsumed = 0;
                actualEffect.BlinkingTurnsGained = 0;
            }
            else if (FullTurns > 0)
            {
                FullTurns -= 1;
                BlinkingTurns += 1;
                actualEffect.FullTurnsConsumed = 1;
                actualEffect.BlinkingTurnsConsumed = 0;
                actualEffect.BlinkingTurnsGained = 1;
            }
            return actualEffect;
        }

        // Caso especial: cuando se pide consumir 1 Full + 1 Blinking (sin otorgar)
        // Significa: "consume 1 Blinking si hay, sino consume 1 Full"
        if (effect.FullTurnsConsumed == 1 && effect.BlinkingTurnsConsumed == 1 && effect.BlinkingTurnsGained == 0)
        {
            if (BlinkingTurns > 0)
            {
                BlinkingTurns -= 1;
                actualEffect.BlinkingTurnsConsumed = 1;
                actualEffect.FullTurnsConsumed = 0;
            }
            else if (FullTurns > 0)
            {
                FullTurns -= 1;
                actualEffect.FullTurnsConsumed = 1;
                actualEffect.BlinkingTurnsConsumed = 0;
            }
            actualEffect.BlinkingTurnsGained = 0;
            return actualEffect;
        }

        // Caso especial: Weak - consume 1 Full Turn y otorga 1 Blinking Turn
        // Si no hay Full Turn, consume 1 Blinking Turn sin otorgar nada
        if (effect.FullTurnsConsumed == 1 && effect.BlinkingTurnsConsumed == 0 && effect.BlinkingTurnsGained == 1)
        {
            if (FullTurns > 0)
            {
                FullTurns -= 1;
                BlinkingTurns += 1;
                actualEffect.FullTurnsConsumed = 1;
                actualEffect.BlinkingTurnsConsumed = 0;
                actualEffect.BlinkingTurnsGained = 1;
            }
            else if (BlinkingTurns > 0)
            {
                BlinkingTurns -= 1;
                actualEffect.BlinkingTurnsConsumed = 1;
                actualEffect.FullTurnsConsumed = 0;
                actualEffect.BlinkingTurnsGained = 0;
            }
            return actualEffect;
        }

        // Primero consumir Blinking Turns
        if (effect.BlinkingTurnsConsumed > 0)
        {
            int consumedBlinking = Math.Min(BlinkingTurns, effect.BlinkingTurnsConsumed);
            BlinkingTurns -= consumedBlinking;
            actualEffect.BlinkingTurnsConsumed = consumedBlinking;
            
            int remainingToConsume = effect.BlinkingTurnsConsumed - consumedBlinking;
            if (remainingToConsume > 0)
            {
                int consumedFull = Math.Min(FullTurns, remainingToConsume);
                FullTurns -= consumedFull;
                actualEffect.FullTurnsConsumed += consumedFull;
            }
        }

        // Luego consumir Full Turns
        if (effect.FullTurnsConsumed > 0)
        {
            int consumed = Math.Min(FullTurns, effect.FullTurnsConsumed);
            FullTurns -= consumed;
            actualEffect.FullTurnsConsumed += consumed;
        }

        // Finalmente agregar Blinking Turns ganados
        BlinkingTurns += effect.BlinkingTurnsGained;
        actualEffect.BlinkingTurnsGained = effect.BlinkingTurnsGained;
        
        return actualEffect;
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
            // Reemplazar la unidad antigua con la nueva en la misma posición
            currentOrder[index] = newUnit;
        }
        _actionOrder = new Queue<Unit>(currentOrder);
    }
}
