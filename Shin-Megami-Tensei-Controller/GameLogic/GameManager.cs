using Shin_Megami_Tensei.Data;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei.GameLogic;

public class GameManager
{
    private readonly View _view;
    private readonly DataLoader _dataLoader;
    private readonly TeamParser _teamParser;
    private readonly BattleEngine _battleEngine;
    private readonly TurnManager _turnManager;

    private Team? _player1Team;
    private Team? _player2Team;
    private Team? _currentPlayerTeam;
    private Team? _opponentTeam;
    private bool _isPlayer1Turn = true;

    public GameManager(View view)
    {
        _view = view;
        _dataLoader = new DataLoader();
        _teamParser = new TeamParser(_dataLoader);
        _battleEngine = new BattleEngine();
        _turnManager = new TurnManager();
    }

    public void StartGame(string teamsFolder)
    {
        _dataLoader.LoadGameData();
        
        if (!LoadTeams(teamsFolder))
        {
            _view.WriteLine("Archivo de equipos no válido");
            return;
        }

        InitializeBattle();
        RunGameLoop();
    }

    private bool LoadTeams(string teamsFolder)
    {
        var teamFiles = Directory.GetFiles(teamsFolder, "*.txt").OrderBy(f => f).ToArray();
        
        _view.WriteLine("Elige un archivo para cargar los equipos");
        for (int i = 0; i < teamFiles.Length; i++)
        {
            var fileName = Path.GetFileName(teamFiles[i]);
            _view.WriteLine($"{i}: {fileName}");
        }

        var input = _view.ReadLine();
        if (!int.TryParse(input, out int selectedIndex) || selectedIndex < 0 || selectedIndex >= teamFiles.Length)
        {
            return false;
        }

        try
        {
            var selectedFile = teamFiles[selectedIndex];
            (_player1Team, _player2Team) = _teamParser.ParseTeamsFromFile(selectedFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void InitializeBattle()
    {
        _currentPlayerTeam = _player1Team;
        _opponentTeam = _player2Team;
        _isPlayer1Turn = true;
    }

    private void RunGameLoop()
    {
        while (!IsGameOver())
        {
            StartPlayerRound();
            
            while (_turnManager.HasTurnsRemaining() && !IsGameOver())
            {
                ProcessPlayerTurn();
            }
            
            SwitchToNextPlayer();
        }
        
        DeclareWinner();
    }

    private void StartPlayerRound()
    {
        _turnManager.InitializePlayerTurn(_currentPlayerTeam!);
        
        var playerLabel = _isPlayer1Turn ? "J1" : "J2";
        var samuraiName = _currentPlayerTeam!.Samurai.Name;
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Ronda de {samuraiName} ({playerLabel})");
        ShowGameState();
    }

    private void ShowGameState()
    {
        _view.WriteLine("----------------------------------------");
        
        // Mostrar el estado del tablero de ambos equipos
        var player1Label = "J1";
        var player2Label = "J2";
        
        _view.WriteLine(_player1Team!.GetFormattedBoardState(player1Label));
        _view.WriteLine(_player2Team!.GetFormattedBoardState(player2Label));
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Full Turns: {_turnManager.FullTurns}");
        _view.WriteLine($"Blinking Turns: {_turnManager.BlinkingTurns}");
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine("Orden:");
        var actionOrder = _turnManager.GetCurrentActionOrder();
        for (int i = 0; i < actionOrder.Count; i++)
        {
            _view.WriteLine($"{i + 1}-{actionOrder[i].Name}");
        }
    }

    private void ProcessPlayerTurn()
    {
        var actingUnit = _turnManager.GetNextActingUnit();
        if (actingUnit == null || !actingUnit.IsAlive)
            return;

        _view.WriteLine("----------------------------------------");
        ShowActionMenu(actingUnit);
        
        var action = GetPlayerAction(actingUnit);
        ExecuteAction(actingUnit, action);
    }

    private void ShowActionMenu(Unit unit)
    {
        _view.WriteLine($"Seleccione una acción para {unit.Name}");
        
        if (unit is Samurai)
        {
            _view.WriteLine("1: Atacar");
            _view.WriteLine("2: Disparar");
            _view.WriteLine("3: Usar Habilidad");
            _view.WriteLine("4: Invocar");
            _view.WriteLine("5: Pasar Turno");
            _view.WriteLine("6: Rendirse");
        }
        else
        {
            _view.WriteLine("1: Atacar");
            _view.WriteLine("2: Usar Habilidad");
            _view.WriteLine("3: Invocar");
            _view.WriteLine("4: Pasar Turno");
        }
    }

    private int GetPlayerAction(Unit unit)
    {
        while (true)
        {
            var input = _view.ReadLine();
            if (int.TryParse(input, out int action))
            {
                if (unit is Samurai && action >= 1 && action <= 6)
                    return action;
                if (unit is Monster && action >= 1 && action <= 4)
                    return action;
            }
            
            // Input inválido, mostrar menú de nuevo
            ShowActionMenu(unit);
        }
    }

    private void ExecuteAction(Unit actingUnit, int action)
    {
        switch (action)
        {
            case 1: // Atacar
                ExecuteAttackAction(actingUnit, "Phys");
                break;
            case 2: // Disparar (solo para Samurai) o Usar Habilidad (para Monster)
                if (actingUnit is Samurai)
                    ExecuteAttackAction(actingUnit, "Gun");
                else
                    ExecuteSkillAction(actingUnit);
                break;
            case 3: // Usar Habilidad (para Samurai) o Invocar (para Monster)
                if (actingUnit is Samurai)
                    ExecuteSkillAction(actingUnit);
                else
                    ExecuteInvokeAction(actingUnit);
                break;
            case 4: // Invocar (para Samurai) o Pasar Turno (para Monster)
                if (actingUnit is Samurai)
                    ExecuteInvokeAction(actingUnit);
                else
                    ExecutePassTurnAction();
                break;
            case 5: // Pasar Turno (solo para Samurai)
                ExecutePassTurnAction();
                break;
            case 6: // Rendirse (solo para Samurai)
                ExecuteSurrenderAction(actingUnit);
                break;
        }
    }

    private void ExecuteAttackAction(Unit attacker, string attackType)
    {
        var target = SelectTarget();
        if (target == null) 
        {
            // Usuario canceló, mostrar menú de acciones de nuevo
            ShowActionMenu(attacker);
            var newAction = GetPlayerAction(attacker);
            ExecuteAction(attacker, newAction);
            return;
        }

        _view.WriteLine("----------------------------------------");
        
        var actionName = attackType == "Gun" ? "dispara a" : "ataca a";
        _view.WriteLine($"{attacker.Name} {actionName} {target.Name}");
        
        var damage = _battleEngine.CalculateDamage(attacker, target, attackType);
        target.TakeDamage(damage);
        
        _view.WriteLine($"{target.Name} recibe {damage} de daño");
        _view.WriteLine($"{target.Name} termina con HP:{target.CurrentHP}/{target.BaseStats.HP}");
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine("Se han consumido 1 Full Turn(s) y 0 Blinking Turn(s)");
        _view.WriteLine("Se han obtenido 0 Blinking Turn(s)");
        
        _turnManager.ConsumeBasicActionTurn();
        
        HandleUnitDeath(target);
    }

    private void ExecuteSkillAction(Unit unit)
    {
        var usableSkills = unit.GetUsableSkills();
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Seleccione una habilidad para que {unit.Name} use");
        
        if (usableSkills.Count == 0)
        {
            _view.WriteLine("1-Cancelar");
        }
        else
        {
            for (int i = 0; i < usableSkills.Count; i++)
            {
                var skill = usableSkills[i];
                _view.WriteLine($"{i + 1}-{skill.Name} MP:{skill.Cost}");
            }
            _view.WriteLine($"{usableSkills.Count + 1}-Cancelar");
        }
        
        // Para E1, siempre cancelar el uso de habilidades
        var input = _view.ReadLine();
        // Regresar al menú de acciones ya que las habilidades no están implementadas en E1
        ShowActionMenu(unit);
        var newAction = GetPlayerAction(unit);
        ExecuteAction(unit, newAction);
    }

    private void ExecuteInvokeAction(Unit unit)
    {
        // Para E1, esta acción no está implementada
        // Consumir turno y continuar
        _turnManager.ConsumeBasicActionTurn();
    }

    private void ExecutePassTurnAction()
    {
        // Para E1, esta acción no está implementada
        // Consumir turno y continuar
        _turnManager.ConsumeBasicActionTurn();
    }

    private void ExecuteSurrenderAction(Unit surrenderingUnit)
    {
        var playerLabel = _isPlayer1Turn ? "J1" : "J2";
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"{surrenderingUnit.Name} ({playerLabel}) se rinde");
        _view.WriteLine("----------------------------------------");
        
        // Terminar el juego inmediatamente
        DeclareWinner();
    }

    private Unit? SelectTarget()
    {
        var targets = _opponentTeam!.GetActiveUnitsOnBoard();
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Seleccione un objetivo para {_turnManager.GetCurrentActionOrder().First().Name}");
        
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            _view.WriteLine($"{i + 1}-{target.Name} HP:{target.CurrentHP}/{target.BaseStats.HP} MP:{target.CurrentMP}/{target.BaseStats.MP}");
        }
        _view.WriteLine($"{targets.Count + 1}-Cancelar");
        
        var input = _view.ReadLine();
        if (int.TryParse(input, out int selection))
        {
            if (selection >= 1 && selection <= targets.Count)
                return targets[selection - 1];
            if (selection == targets.Count + 1)
                return null; // Cancelado
        }
        
        return null; // Input inválido, tratar como cancelar
    }

    private void HandleUnitDeath(Unit unit)
    {
        if (!unit.IsAlive)
        {
            _turnManager.RemoveUnitFromOrder(unit);
            
            if (unit is Monster)
            {
                // Remover monstruo del tablero y enviarlo a la reserva
                _opponentTeam!.RemoveUnitFromBoard(unit);
            }
            // El samurai permanece en el tablero aún cuando esté muerto
        }
    }

    private void SwitchToNextPlayer()
    {
        _isPlayer1Turn = !_isPlayer1Turn;
        (_currentPlayerTeam, _opponentTeam) = (_opponentTeam, _currentPlayerTeam);
    }

    private bool IsGameOver()
    {
        return !_player1Team!.HasActiveUnits() || !_player2Team!.HasActiveUnits();
    }

    private void DeclareWinner()
    {
        string winner;
        
        if (!_player1Team!.HasActiveUnits())
        {
            winner = $"{_player2Team!.Samurai.Name} (J2)";
        }
        else
        {
            winner = $"{_player1Team.Samurai.Name} (J1)";
        }
        
        _view.WriteLine($"Ganador: {winner}");
    }
}
