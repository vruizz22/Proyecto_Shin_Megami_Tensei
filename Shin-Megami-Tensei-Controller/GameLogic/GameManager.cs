using Shin_Megami_Tensei.Data;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Presentation;
using Shin_Megami_Tensei.Domain.ValueObjects;

namespace Shin_Megami_Tensei.GameLogic;

public class GameManager
{
    private const string InvalidTeamMessage = "Archivo de equipos inválido";
    private const int MaxActionOptionsForSamurai = 6;
    private const int MaxActionOptionsForMonster = 4;
    
    private readonly IBattlePresenter _presenter;
    private readonly DataLoader _dataLoader;
    private readonly TeamParser _teamParser;
    private readonly RefactoredBattleEngine _battleEngine;
    private readonly BattleTurnManager _turnManager;

    private Team? _player1Team;
    private Team? _player2Team;
    private Team? _currentPlayerTeam;
    private Team? _opponentTeam;
    private bool _isPlayer1Turn = true;
    
    private int _player1SkillCounter = 0;
    private int _player2SkillCounter = 0;

    public GameManager(IBattlePresenter presenter)
    {
        _presenter = presenter;
        _dataLoader = new DataLoader();
        _teamParser = new TeamParser(_dataLoader);
        _battleEngine = new RefactoredBattleEngine();
        _turnManager = new BattleTurnManager();
    }

    public void StartGame(string teamsFolder)
    {
        _dataLoader.LoadGameData();
        
        if (!LoadTeams(teamsFolder))
        {
            _presenter.ShowMessage(InvalidTeamMessage);
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
        _presenter.ShowMessage("Elige un archivo para cargar los equipos");
        for (int i = 0; i < teamFiles.Length; i++)
        {
            var fileName = Path.GetFileName(teamFiles[i]);
            _presenter.ShowMessage($"{i}: {fileName}");
        }
    }

    private int ReadTeamFileSelection()
    {
        var userInput = _presenter.ReadInput();
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
                if (!_turnManager.HasTurnsRemaining())
                    break;
                    
                ProcessPlayerTurn();
                
                if (IsGameOver())
                    break;
                    
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
        
        _presenter.ShowMessage("----------------------------------------");
        _presenter.ShowMessage($"Ronda de {samuraiName} ({playerLabel})");
    }

    private string GetCurrentPlayerLabel()
    {
        return _isPlayer1Turn ? "J1" : "J2";
    }

    private void ShowGameState()
    {
        _presenter.ShowMessage("----------------------------------------");
        DisplayBothTeamsState();
        DisplayTurnInformation();
        DisplayActionOrder();
    }

    private void DisplayBothTeamsState()
    {
        _presenter.ShowMessage(_player1Team!.GetFormattedBoardState("J1"));
        _presenter.ShowMessage(_player2Team!.GetFormattedBoardState("J2"));
    }

    private void DisplayTurnInformation()
    {
        _presenter.ShowMessage("----------------------------------------");
        _presenter.ShowMessage($"Full Turns: {_turnManager.GetFullTurns()}");
        _presenter.ShowMessage($"Blinking Turns: {_turnManager.GetBlinkingTurns()}");
    }

    private void DisplayActionOrder()
    {
        _presenter.ShowMessage("----------------------------------------");
        _presenter.ShowMessage("Orden:");
        var actionOrder = _turnManager.GetCurrentActionOrder();
        for (int i = 0; i < actionOrder.Count; i++)
        {
            _presenter.ShowMessage($"{i + 1}-{actionOrder[i].Name}");
        }
    }

    private void ProcessPlayerTurn()
    {
        if (IsGameOver())
            return;
        
        if (!_turnManager.HasTurnsRemaining())
            return;
        
        var activeUnits = _currentPlayerTeam!.GetActiveUnitsOnBoard();
        
        if (activeUnits.Count == 0)
            return;
            
        var actingUnit = _turnManager.GetNextActingUnit();
        
        if (actingUnit == null)
            return;
            
        if (!actingUnit.IsAlive)
            return;
        
        if (!activeUnits.Contains(actingUnit))
        {
            return;
        }

        _presenter.ShowMessage("----------------------------------------");
        ShowActionMenu(actingUnit);
        
        var action = GetPlayerAction(actingUnit);
        
        int actingUnitPosition = -1;
        for (int i = 0; i < _currentPlayerTeam!.Board.Length; i++)
        {
            if (_currentPlayerTeam.Board[i] == actingUnit)
            {
                actingUnitPosition = i;
                break;
            }
        }
        
        ExecuteAction(actingUnit, action);
        
        Unit? unitToMove = null;
        
        if (actingUnit.IsAlive && _currentPlayerTeam!.GetActiveUnitsOnBoard().Contains(actingUnit))
        {
            unitToMove = actingUnit;
        }
        else if (actingUnitPosition >= 0 && _currentPlayerTeam!.Board[actingUnitPosition] != null 
                 && _currentPlayerTeam.Board[actingUnitPosition] != actingUnit
                 && _currentPlayerTeam.Board[actingUnitPosition]!.IsAlive)
        {
            unitToMove = _currentPlayerTeam.Board[actingUnitPosition];
        }
        
        if (unitToMove != null)
        {
            _turnManager.MoveUnitToEndOfOrder(unitToMove);
        }
    }

    private void ShowActionMenu(Unit unit)
    {
        _presenter.ShowMessage($"Seleccione una acción para {unit.Name}");
        
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
        _presenter.ShowMessage("1: Atacar");
        _presenter.ShowMessage("2: Disparar");
        _presenter.ShowMessage("3: Usar Habilidad");
        _presenter.ShowMessage("4: Invocar");
        _presenter.ShowMessage("5: Pasar Turno");
        _presenter.ShowMessage("6: Rendirse");
    }

    private void DisplayMonsterActions()
    {
        _presenter.ShowMessage("1: Atacar");
        _presenter.ShowMessage("2: Usar Habilidad");
        _presenter.ShowMessage("3: Invocar");
        _presenter.ShowMessage("4: Pasar Turno");
    }

    private int GetPlayerAction(Unit unit)
    {
        while (true)
        {
            var userInput = _presenter.ReadInput();
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
                _presenter.ShowMessage("----------------------------------------");
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

        _presenter.ShowMessage("----------------------------------------");
        
        var attackResult = _battleEngine.ExecuteAttack(attacker, target, attackType, null);
        DisplayAttackResult(attacker, target, attackType, attackResult);
        
        var actualEffect = _turnManager.ConsumeTurns(attackResult.TurnEffect);
        DisplayTurnConsumption(actualEffect);
        
        HandleUnitDeath(target);
        
        if (attackResult.WasRepelled)
        {
            HandleUnitDeath(attacker);
        }
        
        return true;
    }

    private void DisplayAttackResult(Unit attacker, Unit target, string attackType, RefactoredBattleEngine.AttackResult result)
    {
        var actionVerb = GetAttackVerb(attackType);
        _presenter.ShowMessage($"{attacker.Name} {actionVerb} {target.Name}");
        
        DisplayAffinityMessage(target, result);
        DisplayDamageOrEffect(target, result, attacker);
        
        if (result.WasRepelled)
        {
            _presenter.ShowMessage($"{attacker.Name} termina con HP:{attacker.CurrentHP}/{attacker.BaseStats.HP}");
        }
        else
        {
            _presenter.ShowMessage($"{target.Name} termina con HP:{target.CurrentHP}/{target.BaseStats.HP}");
        }
    }

    private void DisplayAttackResultWithoutHP(Unit attacker, Unit target, string attackType, RefactoredBattleEngine.AttackResult result)
    {
        var actionVerb = GetAttackVerb(attackType);
        _presenter.ShowMessage($"{attacker.Name} {actionVerb} {target.Name}");
        
        DisplayAffinityMessage(target, result);
        DisplayDamageOrEffect(target, result, attacker);
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

    private void DisplayAffinityMessage(Unit target, RefactoredBattleEngine.AttackResult result)
    {
        if (result.Missed)
        {
            _presenter.ShowMessage($"{result.AttackerName} ha fallado el ataque");
            return;
        }

        if (result.InstantKill && result.WasRepelled)
        {
            return;
        }

        switch (result.AffinityEffect)
        {
            case "Wk":
                _presenter.ShowMessage($"{target.Name} es débil contra el ataque de {result.AttackerName}");
                break;
            case "Rs":
                if (!result.InstantKill && !result.Missed)
                {
                    _presenter.ShowMessage($"{target.Name} es resistente el ataque de {result.AttackerName}");
                }
                break;
            case "Nu":
                _presenter.ShowMessage($"{target.Name} bloquea el ataque de {result.AttackerName}");
                break;
        }
    }

    private void DisplayDamageOrEffect(Unit target, RefactoredBattleEngine.AttackResult result, Unit attacker)
    {
        if (result.Missed || result.WasNulled)
        {
            return;
        }

        if (result.InstantKill)
        {
            _presenter.ShowMessage($"{target.Name} ha sido eliminado");
        }
        else if (result.WasRepelled)
        {
            _presenter.ShowMessage($"{target.Name} devuelve {result.Damage} daño a {attacker.Name}");
        }
        else if (result.WasDrained)
        {
            _presenter.ShowMessage($"{target.Name} absorbe {result.Damage} daño");
        }
        else if (result.Damage > 0)
        {
            _presenter.ShowMessage($"{target.Name} recibe {result.Damage} de daño");
        }
    }

    private void DisplayTurnConsumption(TurnEffect effect)
    {
        _presenter.ShowMessage("----------------------------------------");
        _presenter.ShowMessage($"Se han consumido {effect.FullTurnsConsumed} Full Turn(s) y {effect.BlinkingTurnsConsumed} Blinking Turn(s)");
        _presenter.ShowMessage($"Se han obtenido {effect.BlinkingTurnsGained} Blinking Turn(s)");
    }

    private bool ExecuteSkillAction(Unit unit)
    {
        var usableSkills = unit.GetUsableSkills();
        
        _presenter.ShowMessage("----------------------------------------");
        _presenter.ShowMessage($"Seleccione una habilidad para que {unit.Name} use");
        
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
            _presenter.ShowMessage("1-Cancelar");
        }
        else
        {
            for (int i = 0; i < skills.Count; i++)
            {
                _presenter.ShowMessage($"{i + 1}-{skills[i].Name} MP:{skills[i].Cost}");
            }
            _presenter.ShowMessage($"{skills.Count + 1}-Cancelar");
        }
    }

    private Skill? ReadSkillSelection(List<Skill> skills)
    {
        var userInput = _presenter.ReadInput();
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
            if (skill.Name == "Sabbatma" || skill.Name == "Invitation")
            {
                return ExecuteSpecialSummonSkill(user, skill);
            }
            else if (skill.Effect.Contains("Heals HP") || skill.Effect.Contains("Fully heals HP") || skill.Effect.Contains("Greatly heals HP") || skill.Effect.Contains("Revive"))
            {
                return ExecuteHealSkill(user, skill);
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
        
        _presenter.ShowMessage("----------------------------------------");
        
        int hits = CalculateHits(skill.Hits);
        
        RefactoredBattleEngine.AttackResult? finalResult = null;
        Unit? finalAffectedUnit = null;
        
        for (int i = 0; i < hits; i++)
        {
            var attackResult = _battleEngine.ExecuteAttack(user, target, skill.Type, skill.Power);
            DisplayAttackResultWithoutHP(user, target, skill.Type, attackResult);
            
            finalResult = attackResult;
            finalAffectedUnit = attackResult.WasRepelled ? user : target;
        }
        
        if (finalResult != null && finalAffectedUnit != null)
        {
            _presenter.ShowMessage($"{finalAffectedUnit.Name} termina con HP:{finalAffectedUnit.CurrentHP}/{finalAffectedUnit.BaseStats.HP}");
        }
        
        IncrementSkillCounter();
        
        if (finalResult != null)
        {
            var actualEffect = _turnManager.ConsumeTurns(finalResult.TurnEffect);
            DisplayTurnConsumption(actualEffect);
        }
        
        HandleUnitDeath(target);
        
        if (finalResult != null && finalResult.WasRepelled)
        {
            HandleUnitDeath(user);
        }
        
        return true;
    }

    private bool ExecuteHealSkill(Unit user, Skill skill)
    {
        bool isRevive = skill.Effect.Contains("Revive");
        var target = SelectAllyTarget(user, isRevive);
        
        if (target == null)
            return false;

        user.ConsumeMP(skill.Cost);
        
        _presenter.ShowMessage("----------------------------------------");
        
        bool wasDeadBeforeRevive = false;
        bool targetWasOnBoard = false;
        
        if (isRevive && !target.IsAlive)
        {
            wasDeadBeforeRevive = true;
            targetWasOnBoard = _currentPlayerTeam!.Board.Contains(target);
            
            int healAmount = (int)Math.Floor(target.BaseStats.HP * (skill.Power / 100.0));
            target.Heal(healAmount);
            _presenter.ShowMessage($"{user.Name} revive a {target.Name}");
            _presenter.ShowMessage($"{target.Name} recibe {healAmount} de HP");
            
            if (targetWasOnBoard)
            {
                _turnManager.AddUnitToOrder(target);
            }
        }
        else if (!isRevive)
        {
            int healAmount = (int)Math.Floor(target.BaseStats.HP * (skill.Power / 100.0));
            target.Heal(healAmount);
            _presenter.ShowMessage($"{user.Name} cura a {target.Name}");
            _presenter.ShowMessage($"{target.Name} recibe {healAmount} de HP");
        }
        
        _presenter.ShowMessage($"{target.Name} termina con HP:{target.CurrentHP}/{target.BaseStats.HP}");
        
        IncrementSkillCounter();
        
        var turnEffect = new TurnEffect { FullTurnsConsumed = 1, BlinkingTurnsConsumed = 1, BlinkingTurnsGained = 0 };
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
        int healAmount = 0;
        
        if (wasDeadBeforeSummon && canRevive)
        {
            healAmount = (int)Math.Floor(monsterToSummon.BaseStats.HP * (skill.Power / 100.0));
            monsterToSummon.Heal(healAmount);
        }

        var replacedUnit = _currentPlayerTeam!.Board[position];
        
        _currentPlayerTeam!.InvokeMonsterToPosition(monsterToSummon, position);
        
        if (replacedUnit != null)
        {
            _turnManager.ReplaceUnitInOrder(replacedUnit, monsterToSummon);
        }
        else
        {
            _turnManager.AddUnitToOrder(monsterToSummon);
        }

        user.ConsumeMP(skill.Cost);
        
        _presenter.ShowMessage("----------------------------------------");
        _presenter.ShowMessage($"{monsterToSummon.Name} ha sido invocado");
        
        if (wasDeadBeforeSummon && canRevive)
        {
            _presenter.ShowMessage($"{user.Name} revive a {monsterToSummon.Name}");
            _presenter.ShowMessage($"{monsterToSummon.Name} recibe {healAmount} de HP");
            _presenter.ShowMessage($"{monsterToSummon.Name} termina con HP:{monsterToSummon.CurrentHP}/{monsterToSummon.BaseStats.HP}");
        }
        
        IncrementSkillCounter();
        
        var turnEffect = new TurnEffect { FullTurnsConsumed = 1, BlinkingTurnsConsumed = 1, BlinkingTurnsGained = 0 };
        var actualEffect = _turnManager.ConsumeTurns(turnEffect);
        DisplayTurnConsumption(actualEffect);
        
        return true;
    }

    private Unit? SelectAllyTarget(Unit actingUnit, bool onlyDead)
    {
        List<Unit> allies;
        
        if (onlyDead)
        {
            allies = _currentPlayerTeam!.Reserve.Where(u => !u.IsAlive).ToList();
            
            var samurai = _currentPlayerTeam!.Board.FirstOrDefault(u => u != null && u is Samurai && !u.IsAlive);
            if (samurai != null)
            {
                allies.Insert(0, samurai);
            }
        }
        else
        {
            allies = _currentPlayerTeam!.GetActiveUnitsOnBoard();
        }
        
        _presenter.ShowMessage("----------------------------------------");
        _presenter.ShowMessage($"Seleccione un objetivo para {actingUnit.Name}");
        
        for (int i = 0; i < allies.Count; i++)
        {
            var ally = allies[i];
            _presenter.ShowMessage($"{i + 1}-{ally.Name} HP:{ally.CurrentHP}/{ally.BaseStats.HP} MP:{ally.CurrentMP}/{ally.BaseStats.MP}");
        }
        _presenter.ShowMessage($"{allies.Count + 1}-Cancelar");
        
        var userInput = _presenter.ReadInput();
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
        
        _presenter.ShowMessage("----------------------------------------");
        _presenter.ShowMessage("Seleccione un monstruo para invocar");
        
        for (int i = 0; i < availableMonsters.Count; i++)
        {
            var monster = availableMonsters[i];
            _presenter.ShowMessage($"{i + 1}-{monster.Name} HP:{monster.CurrentHP}/{monster.BaseStats.HP} MP:{monster.CurrentMP}/{monster.BaseStats.MP}");
        }
        _presenter.ShowMessage($"{availableMonsters.Count + 1}-Cancelar");
        
        var userInput = _presenter.ReadInput();
        if (!int.TryParse(userInput, out int selection))
            return null;

        if (selection < 1 || selection > availableMonsters.Count)
            return null;

        return availableMonsters[selection - 1];
    }

    private int SelectPositionToSummon()
    {
        _presenter.ShowMessage("----------------------------------------");
        _presenter.ShowMessage("Seleccione una posición para invocar");
        
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
                _presenter.ShowMessage($"{i + 1}-Vacío (Puesto {position + 1})");
            }
            else
            {
                _presenter.ShowMessage($"{i + 1}-{unit.Name} HP:{unit.CurrentHP}/{unit.BaseStats.HP} MP:{unit.CurrentMP}/{unit.BaseStats.MP} (Puesto {position + 1})");
            }
        }
        _presenter.ShowMessage($"{availablePositions.Count + 1}-Cancelar");
        
        var userInput = _presenter.ReadInput();
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

        var replacedUnit = _currentPlayerTeam!.Board[position];
        
        _currentPlayerTeam!.InvokeMonsterToPosition(monsterToSummon, position);
        
        if (replacedUnit != null)
        {
            _turnManager.ReplaceUnitInOrder(replacedUnit, monsterToSummon);
        }
        else
        {
            _turnManager.AddUnitToOrder(monsterToSummon);
        }
        
        _presenter.ShowMessage("----------------------------------------");
        _presenter.ShowMessage($"{monsterToSummon.Name} ha sido invocado");
        
        bool hadBlinkingTurn = _turnManager.GetBlinkingTurns() > 0;
        
        var turnEffect = new TurnEffect 
        { 
            FullTurnsConsumed = 1, 
            BlinkingTurnsConsumed = 1, 
            BlinkingTurnsGained = hadBlinkingTurn ? 0 : 1
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
        
        _turnManager.ReplaceUnitInOrder(currentMonster, monsterToSummon);
        
        _presenter.ShowMessage("----------------------------------------");
        _presenter.ShowMessage($"{monsterToSummon.Name} ha sido invocado");
        
        bool hadBlinkingTurn = _turnManager.GetBlinkingTurns() > 0;
        
        var turnEffect = new TurnEffect 
        { 
            FullTurnsConsumed = 1, 
            BlinkingTurnsConsumed = 1, 
            BlinkingTurnsGained = hadBlinkingTurn ? 0 : 1
        };
        
        var actualEffect = _turnManager.ConsumeTurns(turnEffect);
        DisplayTurnConsumption(actualEffect);
        return true;
    }

    private bool ExecutePassTurnAction()
    {
        bool hadBlinkingTurn = _turnManager.GetBlinkingTurns() > 0;
        
        var turnEffect = new TurnEffect 
        { 
            FullTurnsConsumed = 1, 
            BlinkingTurnsConsumed = 1, 
            BlinkingTurnsGained = hadBlinkingTurn ? 0 : 1
        };
        
        var actualEffect = _turnManager.ConsumeTurns(turnEffect);
        DisplayTurnConsumption(actualEffect);
        return true;
    }

    private bool ExecuteSurrenderAction(Unit surrenderingUnit)
    {
        var playerLabel = GetCurrentPlayerLabel();
        _presenter.ShowMessage("----------------------------------------");
        _presenter.ShowMessage($"{surrenderingUnit.Name} ({playerLabel}) se rinde");
        
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
        
        _presenter.ShowMessage("----------------------------------------");
        _presenter.ShowMessage($"Seleccione un objetivo para {actingUnit.Name}");
        
        DisplayTargetOptions(targets);
        
        return ReadTargetSelection(targets);
    }

    private void DisplayTargetOptions(List<Unit> targets)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            _presenter.ShowMessage($"{i + 1}-{target.Name} HP:{target.CurrentHP}/{target.BaseStats.HP} MP:{target.CurrentMP}/{target.BaseStats.MP}");
        }
        _presenter.ShowMessage($"{targets.Count + 1}-Cancelar");
    }

    private Unit? ReadTargetSelection(List<Unit> targets)
    {
        var userInput = _presenter.ReadInput();
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
                if (_player1Team!.Board.Contains(unit))
                {
                    _player1Team.RemoveUnitFromBoard(unit);
                }
                else if (_player2Team!.Board.Contains(unit))
                {
                    _player2Team.RemoveUnitFromBoard(unit);
                }
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
        _presenter.ShowMessage("----------------------------------------");
        string winner = DetermineWinner();
        _presenter.ShowMessage($"Ganador: {winner}");
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

    private int GetCurrentSkillCounter()
    {
        return _isPlayer1Turn ? _player1SkillCounter : _player2SkillCounter;
    }

    private void IncrementSkillCounter()
    {
        if (_isPlayer1Turn)
            _player1SkillCounter++;
        else
            _player2SkillCounter++;
    }

    private int CalculateHits(string hitsString)
    {
        if (int.TryParse(hitsString, out int simpleHits))
        {
            return simpleHits;
        }

        var parts = hitsString.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[0], out int minHits) && int.TryParse(parts[1], out int maxHits))
        {
            int k = GetCurrentSkillCounter();
            int offset = k % (maxHits - minHits + 1);
            return minHits + offset;
        }

        return 1;
    }
}
