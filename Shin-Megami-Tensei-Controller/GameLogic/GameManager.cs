using Shin_Megami_Tensei.Data;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei.GameLogic;

public class GameManager
{
    private const string InvalidTeamMessage = "Archivo de equipos inválido";
    private const int MaxActionOptionsForSamurai = 6;
    private const int MaxActionOptionsForMonster = 4;
    
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
            _view.WriteLine(InvalidTeamMessage);
            return;
        }

        InitializeBattle();
        RunGameLoop();
    }

    private bool LoadTeams(string teamsFolder)
    {
        var teamFiles = Directory.GetFiles(teamsFolder, "*.txt").OrderBy(f => f).ToArray();
        
        DisplayTeamFiles(teamFiles);

        var selectedFileIndex = ReadTeamFileSelection();
        if (!IsValidFileSelection(selectedFileIndex, teamFiles.Length))
        {
            return false;
        }

        return TryParseTeamsFromFile(teamFiles[selectedFileIndex]);
    }

    private void DisplayTeamFiles(string[] teamFiles)
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");
        for (int i = 0; i < teamFiles.Length; i++)
        {
            var fileName = Path.GetFileName(teamFiles[i]);
            _view.WriteLine($"{i}: {fileName}");
        }
    }

    private int ReadTeamFileSelection()
    {
        var userInput = _view.ReadLine();
        int.TryParse(userInput, out int selectedIndex);
        return selectedIndex;
    }

    private bool IsValidFileSelection(int selectedIndex, int totalFiles)
    {
        return selectedIndex >= 0 && selectedIndex < totalFiles;
    }

    private bool TryParseTeamsFromFile(string selectedFile)
    {
        try
        {
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
            
            while (HasTurnsAndAliveUnits())
            {
                ProcessPlayerTurn();
                
                if (IsGameOver())
                    break;
                    
                // Mostrar el estado del juego después de cada turno si aún hay turnos
                if (HasTurnsAndAliveUnits())
                {
                    ShowGameState();
                }
            }
            
            if (IsGameOver())
                break;
                
            SwitchToNextPlayer();
        }
        
        DeclareWinner();
    }

    private bool HasTurnsAndAliveUnits()
    {
        return _turnManager.HasTurnsRemaining() && !IsGameOver();
    }

    private void StartPlayerRound()
    {
        _turnManager.InitializePlayerTurn(_currentPlayerTeam!);
        DisplayRoundHeader();
        ShowGameState();
    }

    private void DisplayRoundHeader()
    {
        var playerLabel = GetCurrentPlayerLabel();
        var samuraiName = _currentPlayerTeam!.Samurai.Name;
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Ronda de {samuraiName} ({playerLabel})");
    }

    private string GetCurrentPlayerLabel()
    {
        return _isPlayer1Turn ? "J1" : "J2";
    }

    private void ShowGameState()
    {
        _view.WriteLine("----------------------------------------");
        DisplayBothTeamsState();
        DisplayTurnInformation();
        DisplayActionOrder();
    }

    private void DisplayBothTeamsState()
    {
        _view.WriteLine(_player1Team!.GetFormattedBoardState("J1"));
        _view.WriteLine(_player2Team!.GetFormattedBoardState("J2"));
    }

    private void DisplayTurnInformation()
    {
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Full Turns: {_turnManager.FullTurns}");
        _view.WriteLine($"Blinking Turns: {_turnManager.BlinkingTurns}");
    }

    private void DisplayActionOrder()
    {
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
        // Verificar si el juego terminó antes de procesar el turno
        if (IsGameOver())
            return;
            
        var actingUnit = _turnManager.GetNextActingUnit();
        if (actingUnit == null || !actingUnit.IsAlive)
        {
            // Si no hay unidades vivas que puedan actuar, terminar el turno
            return;
        }

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
            DisplaySamuraiActions();
        }
        else
        {
            DisplayMonsterActions();
        }
    }

    private void DisplaySamuraiActions()
    {
        _view.WriteLine("1: Atacar");
        _view.WriteLine("2: Disparar");
        _view.WriteLine("3: Usar Habilidad");
        _view.WriteLine("4: Invocar");
        _view.WriteLine("5: Pasar Turno");
        _view.WriteLine("6: Rendirse");
    }

    private void DisplayMonsterActions()
    {
        _view.WriteLine("1: Atacar");
        _view.WriteLine("2: Usar Habilidad");
        _view.WriteLine("3: Invocar");
        _view.WriteLine("4: Pasar Turno");
    }

    private int GetPlayerAction(Unit unit)
    {
        while (true)
        {
            var userInput = _view.ReadLine();
            if (IsValidActionForUnit(userInput, unit, out int action))
            {
                return action;
            }
            
            ShowActionMenu(unit);
        }
    }

    private bool IsValidActionForUnit(string input, Unit unit, out int action)
    {
        if (!int.TryParse(input, out action))
            return false;

        int maxAction = unit is Samurai ? MaxActionOptionsForSamurai : MaxActionOptionsForMonster;
        return action >= 1 && action <= maxAction;
    }

    private void ExecuteAction(Unit actingUnit, int action)
    {
        bool actionExecuted = false;
        
        while (!actionExecuted)
        {
            actionExecuted = TryExecuteSpecificAction(actingUnit, action);
            
            if (!actionExecuted)
            {
                _view.WriteLine("----------------------------------------");
                ShowActionMenu(actingUnit);
                action = GetPlayerAction(actingUnit);
            }
        }
    }

    private bool TryExecuteSpecificAction(Unit actingUnit, int action)
    {
        return action switch
        {
            1 => ExecuteAttackAction(actingUnit, "Phys"),
            2 => HandleSecondAction(actingUnit),
            3 => HandleThirdAction(actingUnit),
            4 => HandleFourthAction(actingUnit),
            5 => ExecutePassTurnAction(),
            6 => ExecuteSurrenderAction(actingUnit),
            _ => false
        };
    }

    private bool HandleSecondAction(Unit actingUnit)
    {
        return actingUnit is Samurai 
            ? ExecuteAttackAction(actingUnit, "Gun") 
            : ExecuteSkillAction(actingUnit);
    }

    private bool HandleThirdAction(Unit actingUnit)
    {
        return actingUnit is Samurai 
            ? ExecuteSkillAction(actingUnit) 
            : ExecuteInvokeAction(actingUnit);
    }

    private bool HandleFourthAction(Unit actingUnit)
    {
        return actingUnit is Samurai 
            ? ExecuteInvokeAction(actingUnit) 
            : ExecutePassTurnAction();
    }

    private bool ExecuteAttackAction(Unit attacker, string attackType)
    {
        var target = SelectTarget(attacker);
        if (target == null) 
        {
            return false;
        }

        _view.WriteLine("----------------------------------------");
        
        var attackResult = _battleEngine.ExecuteAttack(attacker, target, attackType, null);
        DisplayAttackResult(attacker, target, attackType, attackResult);
        
        var actualEffect = _turnManager.ConsumeTurns(attackResult.TurnEffect);
        DisplayTurnConsumption(actualEffect);
        
        HandleUnitDeath(target);
        return true;
    }

    private void DisplayAttackResult(Unit attacker, Unit target, string attackType, BattleEngine.AttackResult result)
    {
        var actionVerb = GetAttackVerb(attackType);
        _view.WriteLine($"{attacker.Name} {actionVerb} {target.Name}");
        
        DisplayAffinityMessage(target, result);
        DisplayDamageOrEffect(target, result);
        
        _view.WriteLine($"{target.Name} termina con HP:{target.CurrentHP}/{target.BaseStats.HP}");
    }

    private string GetAttackVerb(string attackType)
    {
        return attackType switch
        {
            "Gun" => "dispara a",
            "Fire" => "lanza fuego a",
            "Ice" => "lanza hielo a",
            "Elec" => "lanza electricidad a",
            "Force" => "lanza viento a",
            "Light" => "ataca con luz a",
            "Dark" => "ataca con oscuridad a",
            _ => "ataca a"
        };
    }

    private void DisplayAffinityMessage(Unit target, BattleEngine.AttackResult result)
    {
        if (result.Missed)
        {
            _view.WriteLine($"{result.AttackerName} ha fallado el ataque");
            return;
        }

        if (result.InstantKill && result.WasRepelled)
        {
            return;
        }

        switch (result.AffinityEffect)
        {
            case "Wk":
                _view.WriteLine($"{target.Name} es débil contra el ataque de {result.AttackerName}");
                break;
            case "Rs":
                if (!result.InstantKill && !result.Missed)
                {
                    _view.WriteLine($"{target.Name} es resistente el ataque de {result.AttackerName}");
                }
                break;
            case "Nu":
                _view.WriteLine($"{target.Name} bloquea el ataque de {result.AttackerName}");
                break;
        }
    }

    private void DisplayDamageOrEffect(Unit target, BattleEngine.AttackResult result)
    {
        if (result.Missed || result.WasNulled)
        {
            return;
        }

        if (result.InstantKill)
        {
            _view.WriteLine($"{target.Name} ha sido eliminado");
        }
        else if (result.WasRepelled)
        {
            _view.WriteLine($"{target.Name} devuelve {result.Damage} daño a {result.AttackerName}");
        }
        else if (result.WasDrained)
        {
            _view.WriteLine($"{target.Name} absorbe {result.Damage} daño");
        }
        else if (result.Damage > 0)
        {
            _view.WriteLine($"{target.Name} recibe {result.Damage} de daño");
        }
    }

    private void DisplayTurnConsumption(TurnManager.TurnEffect effect)
    {
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Se han consumido {effect.FullTurnsConsumed} Full Turn(s) y {effect.BlinkingTurnsConsumed} Blinking Turn(s)");
        _view.WriteLine($"Se han obtenido {effect.BlinkingTurnsGained} Blinking Turn(s)");
    }

    private bool ExecuteSkillAction(Unit unit)
    {
        var usableSkills = unit.GetUsableSkills();
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Seleccione una habilidad para que {unit.Name} use");
        
        DisplaySkillOptions(usableSkills);
        
        var selectedSkill = ReadSkillSelection(usableSkills);
        if (selectedSkill == null)
        {
            return false;
        }

        return ExecuteSelectedSkill(unit, selectedSkill);
    }

    private void DisplaySkillOptions(List<Skill> skills)
    {
        if (skills.Count == 0)
        {
            _view.WriteLine("1-Cancelar");
        }
        else
        {
            for (int i = 0; i < skills.Count; i++)
            {
                _view.WriteLine($"{i + 1}-{skills[i].Name} MP:{skills[i].Cost}");
            }
            _view.WriteLine($"{skills.Count + 1}-Cancelar");
        }
    }

    private Skill? ReadSkillSelection(List<Skill> skills)
    {
        var userInput = _view.ReadLine();
        if (!int.TryParse(userInput, out int selection))
            return null;

        int cancelOption = skills.Count == 0 ? 1 : skills.Count + 1;
        if (selection == cancelOption || selection < 1 || selection > skills.Count)
            return null;

        return skills[selection - 1];
    }

    private bool ExecuteSelectedSkill(Unit user, Skill skill)
    {
        if (skill.Target == "Single")
        {
            return ExecuteSkillOnSingleTarget(user, skill);
        }
        else if (skill.Target == "Ally")
        {
            if (skill.Effect.Contains("Heals HP") || skill.Effect.Contains("Revive"))
            {
                return ExecuteHealSkill(user, skill);
            }
            else if (skill.Name == "Sabbatma" || skill.Name == "Invitation")
            {
                return ExecuteSpecialSummonSkill(user, skill);
            }
        }
        
        return false;
    }

    private bool ExecuteSkillOnSingleTarget(Unit user, Skill skill)
    {
        var target = SelectTarget(user);
        if (target == null)
            return false;

        user.ConsumeMP(skill.Cost);
        
        _view.WriteLine("----------------------------------------");
        
        var attackResult = _battleEngine.ExecuteAttack(user, target, skill.Type, skill.Power);
        DisplayAttackResult(user, target, skill.Type, attackResult);
        
        var actualEffect = _turnManager.ConsumeTurns(attackResult.TurnEffect);
        DisplayTurnConsumption(actualEffect);
        
        HandleUnitDeath(target);
        return true;
    }

    private bool ExecuteHealSkill(Unit user, Skill skill)
    {
        bool isRevive = skill.Effect.Contains("Revive");
        var target = SelectAllyTarget(user, isRevive);
        
        if (target == null)
            return false;

        user.ConsumeMP(skill.Cost);
        
        _view.WriteLine("----------------------------------------");
        
        if (isRevive && !target.IsAlive)
        {
            int healAmount = skill.Power;
            target.Heal(healAmount);
            _view.WriteLine($"{user.Name} revive a {target.Name}");
            _view.WriteLine($"{target.Name} recibe {healAmount} de HP");
        }
        else if (!isRevive)
        {
            int healAmount = (int)Math.Floor(target.BaseStats.HP * (skill.Power / 100.0));
            target.Heal(healAmount);
            _view.WriteLine($"{user.Name} cura a {target.Name}");
            _view.WriteLine($"{target.Name} recibe {healAmount} de HP");
        }
        
        _view.WriteLine($"{target.Name} termina con HP:{target.CurrentHP}/{target.BaseStats.HP}");
        
        // Habilidad de curación: consume 1 Blinking si hay, sino consume 1 Full
        var turnEffect = new TurnManager.TurnEffect { FullTurnsConsumed = 1, BlinkingTurnsConsumed = 1, BlinkingTurnsGained = 0 };
        var actualEffect = _turnManager.ConsumeTurns(turnEffect);
        DisplayTurnConsumption(actualEffect);
        
        return true;
    }

    private bool ExecuteSpecialSummonSkill(Unit user, Skill skill)
    {
        bool canRevive = skill.Name == "Invitation";
        var monsterToSummon = SelectMonsterFromReserve(canRevive);
        
        if (monsterToSummon == null)
            return false;

        var position = SelectPositionToSummon();
        if (position == -1)
            return false;

        bool wasDeadBeforeSummon = !monsterToSummon.IsAlive;
        
        if (wasDeadBeforeSummon && canRevive)
        {
            monsterToSummon.Heal(skill.Power);
        }

        _currentPlayerTeam!.InvokeMonsterToPosition(monsterToSummon, position);
        _turnManager.AddUnitToOrder(monsterToSummon);

        user.ConsumeMP(skill.Cost);
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"{monsterToSummon.Name} ha sido invocado");
        
        if (wasDeadBeforeSummon && canRevive)
        {
            _view.WriteLine($"{user.Name} revive a {monsterToSummon.Name}");
            _view.WriteLine($"{monsterToSummon.Name} recibe {skill.Power} de HP");
            _view.WriteLine($"{monsterToSummon.Name} termina con HP:{monsterToSummon.CurrentHP}/{monsterToSummon.BaseStats.HP}");
        }
        
        // Habilidad especial: consume 1 Blinking si hay, sino consume 1 Full
        var turnEffect = new TurnManager.TurnEffect { FullTurnsConsumed = 1, BlinkingTurnsConsumed = 1, BlinkingTurnsGained = 0 };
        var actualEffect = _turnManager.ConsumeTurns(turnEffect);
        DisplayTurnConsumption(actualEffect);
        
        return true;
    }

    private Unit? SelectAllyTarget(Unit actingUnit, bool onlyDead)
    {
        var allies = onlyDead 
            ? _currentPlayerTeam!.Reserve.Where(u => !u.IsAlive).ToList()
            : _currentPlayerTeam!.GetActiveUnitsOnBoard();
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Seleccione un objetivo para {actingUnit.Name}");
        
        for (int i = 0; i < allies.Count; i++)
        {
            var ally = allies[i];
            _view.WriteLine($"{i + 1}-{ally.Name} HP:{ally.CurrentHP}/{ally.BaseStats.HP} MP:{ally.CurrentMP}/{ally.BaseStats.MP}");
        }
        _view.WriteLine($"{allies.Count + 1}-Cancelar");
        
        var userInput = _view.ReadLine();
        if (!int.TryParse(userInput, out int selection))
            return null;

        if (selection < 1 || selection > allies.Count)
            return null;

        return allies[selection - 1];
    }

    private Monster? SelectMonsterFromReserve(bool includeDeadMonsters)
    {
        var availableMonsters = includeDeadMonsters 
            ? _currentPlayerTeam!.Reserve.OfType<Monster>().ToList()
            : _currentPlayerTeam!.Reserve.OfType<Monster>().Where(m => m.IsAlive).ToList();
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine("Seleccione un monstruo para invocar");
        
        for (int i = 0; i < availableMonsters.Count; i++)
        {
            var monster = availableMonsters[i];
            _view.WriteLine($"{i + 1}-{monster.Name} HP:{monster.CurrentHP}/{monster.BaseStats.HP} MP:{monster.CurrentMP}/{monster.BaseStats.MP}");
        }
        _view.WriteLine($"{availableMonsters.Count + 1}-Cancelar");
        
        var userInput = _view.ReadLine();
        if (!int.TryParse(userInput, out int selection))
            return null;

        if (selection < 1 || selection > availableMonsters.Count)
            return null;

        return availableMonsters[selection - 1];
    }

    private int SelectPositionToSummon()
    {
        _view.WriteLine("----------------------------------------");
        _view.WriteLine("Seleccione una posición para invocar");
        
        var availablePositions = new List<(int position, Unit? unit)>();
        
        for (int i = 1; i < _currentPlayerTeam!.Board.Length; i++)
        {
            availablePositions.Add((i, _currentPlayerTeam.Board[i]));
        }
        
        for (int i = 0; i < availablePositions.Count; i++)
        {
            var (position, unit) = availablePositions[i];
            if (unit == null)
            {
                _view.WriteLine($"{i + 1}-Vacío (Puesto {position + 1})");
            }
            else
            {
                _view.WriteLine($"{i + 1}-{unit.Name} HP:{unit.CurrentHP}/{unit.BaseStats.HP} MP:{unit.CurrentMP}/{unit.BaseStats.MP} (Puesto {position + 1})");
            }
        }
        _view.WriteLine($"{availablePositions.Count + 1}-Cancelar");
        
        var userInput = _view.ReadLine();
        if (!int.TryParse(userInput, out int selection))
            return -1;

        if (selection < 1 || selection > availablePositions.Count)
            return -1;

        return availablePositions[selection - 1].position;
    }

    private bool ExecuteInvokeAction(Unit unit)
    {
        if (unit is Samurai)
        {
            return ExecuteSamuraiInvoke();
        }
        else
        {
            return ExecuteMonsterInvoke(unit);
        }
    }

    private bool ExecuteSamuraiInvoke()
    {
        var monsterToSummon = SelectMonsterFromReserve(false);
        if (monsterToSummon == null)
            return false;

        var position = SelectPositionToSummon();
        if (position == -1)
            return false;

        // Guardar la unidad que está siendo reemplazada (si existe)
        var replacedUnit = _currentPlayerTeam!.Board[position];
        
        _currentPlayerTeam!.InvokeMonsterToPosition(monsterToSummon, position);
        
        // Si se reemplazó una unidad, la nueva toma su lugar en el orden
        if (replacedUnit != null)
        {
            _turnManager.ReplaceUnitInOrder(replacedUnit, monsterToSummon);
        }
        else
        {
            // Si se invocó a un puesto vacío, se agrega al final del orden
            _turnManager.AddUnitToOrder(monsterToSummon);
        }
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"{monsterToSummon.Name} ha sido invocado");
        
        // Invocar (Samurai): consume 1 Blinking si hay, sino consume 1 Full y otorga 1 Blinking
        var turnEffect = new TurnManager.TurnEffect 
        { 
            FullTurnsConsumed = 1, 
            BlinkingTurnsConsumed = 1, 
            BlinkingTurnsGained = 1 
        };
        
        var actualEffect = _turnManager.ConsumeTurns(turnEffect);
        DisplayTurnConsumption(actualEffect);
        return true;
    }

    private bool ExecuteMonsterInvoke(Unit currentMonster)
    {
        var monsterToSummon = SelectMonsterFromReserve(false);
        if (monsterToSummon == null)
            return false;

        _currentPlayerTeam!.ReplaceMonsterInBoard(currentMonster, monsterToSummon);
        
        // El monstruo invocado va al final del orden (no reemplaza al que invocó)
        _turnManager.AddUnitToOrder(monsterToSummon);
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"{monsterToSummon.Name} ha sido invocado");
        
        // Invocar (Monstruo): consume 1 Blinking si hay, sino consume 1 Full y otorga 1 Blinking
        var turnEffect = new TurnManager.TurnEffect 
        { 
            FullTurnsConsumed = 1, 
            BlinkingTurnsConsumed = 1, 
            BlinkingTurnsGained = 1 
        };
        
        var actualEffect = _turnManager.ConsumeTurns(turnEffect);
        DisplayTurnConsumption(actualEffect);
        return true;
    }

    private bool ExecutePassTurnAction()
    {
        // Pasar Turno: consume 1 Blinking si hay, sino consume 1 Full y otorga 1 Blinking
        var turnEffect = new TurnManager.TurnEffect 
        { 
            FullTurnsConsumed = 1, 
            BlinkingTurnsConsumed = 1, 
            BlinkingTurnsGained = 1 
        };
        
        var actualEffect = _turnManager.ConsumeTurns(turnEffect);
        DisplayTurnConsumption(actualEffect);
        return true;
    }

    private bool ExecuteSurrenderAction(Unit surrenderingUnit)
    {
        var playerLabel = GetCurrentPlayerLabel();
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"{surrenderingUnit.Name} ({playerLabel}) se rinde");
        
        KillAllUnitsInTeam(_currentPlayerTeam!);
        
        return true;
    }

    private void KillAllUnitsInTeam(Team team)
    {
        team.Samurai.TakeDamage(team.Samurai.CurrentHP);
        foreach (var unit in team.GetActiveUnitsOnBoard().ToList())
        {
            if (unit != team.Samurai)
            {
                unit.TakeDamage(unit.CurrentHP);
            }
        }
    }

    private Unit? SelectTarget(Unit actingUnit)
    {
        var targets = _opponentTeam!.GetActiveUnitsOnBoard();
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Seleccione un objetivo para {actingUnit.Name}");
        
        DisplayTargetOptions(targets);
        
        return ReadTargetSelection(targets);
    }

    private void DisplayTargetOptions(List<Unit> targets)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            _view.WriteLine($"{i + 1}-{target.Name} HP:{target.CurrentHP}/{target.BaseStats.HP} MP:{target.CurrentMP}/{target.BaseStats.MP}");
        }
        _view.WriteLine($"{targets.Count + 1}-Cancelar");
    }

    private Unit? ReadTargetSelection(List<Unit> targets)
    {
        var userInput = _view.ReadLine();
        if (!int.TryParse(userInput, out int selection))
            return null;

        if (selection >= 1 && selection <= targets.Count)
            return targets[selection - 1];

        return null;
    }

    private void HandleUnitDeath(Unit unit)
    {
        if (!unit.IsAlive)
        {
            _turnManager.RemoveUnitFromOrder(unit);
            
            if (unit is Monster)
            {
                _opponentTeam!.RemoveUnitFromBoard(unit);
            }
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
        _view.WriteLine("----------------------------------------");
        string winner = DetermineWinner();
        _view.WriteLine($"Ganador: {winner}");
    }

    private string DetermineWinner()
    {
        if (!_player1Team!.HasActiveUnits())
        {
            return $"{_player2Team!.Samurai.Name} (J2)";
        }
        else
        {
            return $"{_player1Team.Samurai.Name} (J1)";
        }
    }
}
