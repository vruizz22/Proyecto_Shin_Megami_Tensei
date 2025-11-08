﻿﻿using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.GameLogic;

public class BattleTurnManager
{
    private int _fullTurns;
    private int _blinkingTurns;
    private Queue<Unit> _actionOrder = new();
    private Team? _currentTeam;

    public int GetFullTurns() => _fullTurns;
    public int GetBlinkingTurns() => _blinkingTurns;
    
    public void InitializePlayerTurn(Team team)
    {
        _currentTeam = team;
        var activeUnits = team.GetActiveUnitsOnBoard();
        _fullTurns = activeUnits.Count;
        _blinkingTurns = 0;
        
        SetupActionOrder(activeUnits);
    }

    private void SetupActionOrder(List<Unit> units)
    {
        var orderedUnits = units
            .OrderByDescending(u => u.BaseStats.Spd)
            .ThenBy(u => GetBoardPosition(u))
            .ToList();

        _actionOrder = new Queue<Unit>(orderedUnits);
    }

    private int GetBoardPosition(Unit unit)
    {
        if (_currentTeam != null)
        {
            for (int i = 0; i < _currentTeam.Board.Length; i++)
            {
                if (_currentTeam.Board[i] == unit)
                    return i;
            }
        }
        
        return unit.Name.GetHashCode();
    }

    public Unit? GetNextActingUnit()
    {
        if (!HasValidUnits())
            return null;
            
        CleanInvalidUnitsFromQueue();
        
        if (_actionOrder.Count == 0)
            return null;
            
        return _actionOrder.Dequeue();
    }
    
    private bool HasValidUnits()
    {
        if (_actionOrder.Count == 0)
            return false;
        
        if (_currentTeam == null)
            return false;
            
        var activeUnits = _currentTeam.GetActiveUnitsOnBoard();
        return _actionOrder.Any(unit => IsUnitValid(unit, activeUnits));
    }

    private void CleanInvalidUnitsFromQueue()
    {
        var activeUnits = _currentTeam?.GetActiveUnitsOnBoard() ?? new List<Unit>();
        var validUnits = new List<Unit>();
        
        while (_actionOrder.Count > 0)
        {
            var unit = _actionOrder.Dequeue();
            if (IsUnitValid(unit, activeUnits))
            {
                validUnits.Add(unit);
            }
        }
        
        _actionOrder = new Queue<Unit>(validUnits);
    }

    private bool IsUnitValid(Unit unit, List<Unit> activeUnits)
    {
        return unit.IsAlive && activeUnits.Contains(unit);
    }
    
    public void MoveUnitToEndOfOrder(Unit unit)
    {
        _actionOrder.Enqueue(unit);
    }

    public bool HasTurnsRemaining()
    {
        if (!HasAnyTurns())
            return false;
        
        if (_actionOrder.Count == 0)
            return false;
        
        return HasValidUnits();
    }

    private bool HasAnyTurns()
    {
        return _fullTurns > 0 || _blinkingTurns > 0;
    }

    public TurnCost ConsumeTurns(TurnCost cost)
    {
        if (cost.ConsumeAllTurns)
        {
            return ConsumeAllTurns();
        }

        if (IsPassTurnOrInvokePattern(cost))
        {
            return HandlePassTurnOrInvokeConsumption(cost);
        }

        if (IsSkillOrActionPattern(cost))
        {
            return HandleSkillOrActionConsumption();
        }

        if (IsWeakAffinityPattern(cost))
        {
            return HandleWeakAffinityConsumption();
        }

        return HandleStandardConsumption(cost);
    }

    private bool IsPassTurnOrInvokePattern(TurnCost cost)
    {
        return cost.FullTurnsConsumed == 1 && 
               cost.BlinkingTurnsConsumed == 1 && 
               cost.BlinkingTurnsGained == 1;
    }

    private bool IsSkillOrActionPattern(TurnCost cost)
    {
        return cost.FullTurnsConsumed == 1 && 
               cost.BlinkingTurnsConsumed == 1 && 
               cost.BlinkingTurnsGained == 0;
    }

    private bool IsWeakAffinityPattern(TurnCost cost)
    {
        return cost.FullTurnsConsumed == 1 && 
               cost.BlinkingTurnsConsumed == 0 && 
               cost.BlinkingTurnsGained == 1;
    }

    private TurnCost ConsumeAllTurns()
    {
        var consumed = new TurnCost(_fullTurns, _blinkingTurns, 0);
        _fullTurns = 0;
        _blinkingTurns = 0;
        return consumed;
    }

    private TurnCost HandlePassTurnOrInvokeConsumption(TurnCost cost)
    {
        if (_blinkingTurns > 0)
        {
            _blinkingTurns--;
            return new TurnCost(0, 1, 0);
        }
        
        if (_fullTurns > 0)
        {
            _fullTurns--;
            _blinkingTurns++;
            return new TurnCost(1, 0, 1);
        }
        
        return new TurnCost(0, 0, 0);
    }

    private TurnCost HandleSkillOrActionConsumption()
    {
        if (_blinkingTurns > 0)
        {
            _blinkingTurns--;
            return new TurnCost(0, 1, 0);
        }
        
        if (_fullTurns > 0)
        {
            _fullTurns--;
            return new TurnCost(1, 0, 0);
        }
        
        return new TurnCost(0, 0, 0);
    }

    private TurnCost HandleWeakAffinityConsumption()
    {
        if (_fullTurns > 0)
        {
            _fullTurns--;
            _blinkingTurns++;
            return new TurnCost(1, 0, 1);
        }
        
        if (_blinkingTurns > 0)
        {
            _blinkingTurns--;
            return new TurnCost(0, 1, 0);
        }
        
        return new TurnCost(0, 0, 0);
    }

    private TurnCost HandleStandardConsumption(TurnCost cost)
    {
        var actualCost = new TurnCost();
        
        if (cost.BlinkingTurnsConsumed > 0)
        {
            int consumedBlinking = Math.Min(_blinkingTurns, cost.BlinkingTurnsConsumed);
            _blinkingTurns -= consumedBlinking;
            actualCost = new TurnCost(actualCost.FullTurnsConsumed, consumedBlinking, actualCost.BlinkingTurnsGained);
            
            int remainingToConsume = cost.BlinkingTurnsConsumed - consumedBlinking;
            if (remainingToConsume > 0)
            {
                int consumedFull = Math.Min(_fullTurns, remainingToConsume);
                _fullTurns -= consumedFull;
                actualCost = new TurnCost(actualCost.FullTurnsConsumed + consumedFull, actualCost.BlinkingTurnsConsumed, actualCost.BlinkingTurnsGained);
            }
        }

        if (cost.FullTurnsConsumed > 0)
        {
            int consumed = Math.Min(_fullTurns, cost.FullTurnsConsumed);
            _fullTurns -= consumed;
            actualCost = new TurnCost(actualCost.FullTurnsConsumed + consumed, actualCost.BlinkingTurnsConsumed, actualCost.BlinkingTurnsGained);
        }

        _blinkingTurns += cost.BlinkingTurnsGained;
        return new TurnCost(actualCost.FullTurnsConsumed, actualCost.BlinkingTurnsConsumed, cost.BlinkingTurnsGained);
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
        _actionOrder.Enqueue(unit);
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

    // Método de compatibilidad: acepta TurnEffect y retorna TurnEffect
    public TurnEffect ConsumeTurns(TurnEffect effect)
    {
        // Convertir TurnEffect a TurnCost
        var turnCost = new TurnCost(
            effect.FullTurnsConsumed,
            effect.BlinkingTurnsConsumed,
            effect.BlinkingTurnsGained,
            effect.ConsumeAllTurns
        );

        // Consumir usando el método principal
        var resultCost = ConsumeTurns(turnCost);

        // Convertir resultado de vuelta a TurnEffect
        return new TurnEffect
        {
            FullTurnsConsumed = resultCost.FullTurnsConsumed,
            BlinkingTurnsConsumed = resultCost.BlinkingTurnsConsumed,
            BlinkingTurnsGained = resultCost.BlinkingTurnsGained,
            ConsumeAllTurns = resultCost.ConsumeAllTurns
        };
    }
}

