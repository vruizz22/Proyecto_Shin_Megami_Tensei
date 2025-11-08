using Shin_Megami_Tensei.Domain.Constants;
using Shin_Megami_Tensei.Domain.Enums;
using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.GameLogic;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei.Presentation;

public class ConsoleBattlePresenter : IBattlePresenter
{
    private readonly View _view;

    public ConsoleBattlePresenter(View view)
    {
        _view = view;
    }

    public void ShowSeparator()
    {
        _view.WriteLine(GameConstants.Messages.Separator);
    }

    public void ShowMessage(string message)
    {
        _view.WriteLine(message);
    }

    public void ShowRoundHeader(string samuraiName, string playerLabel)
    {
        ShowSeparator();
        _view.WriteLine($"Ronda de {samuraiName} ({playerLabel})");
    }

    public void ShowTeamsState(Team player1Team, Team player2Team)
    {
        ShowSeparator();
        _view.WriteLine(player1Team.GetFormattedBoardState("J1"));
        _view.WriteLine(player2Team.GetFormattedBoardState("J2"));
    }

    public void ShowTurnInformation(int fullTurns, int blinkingTurns)
    {
        ShowSeparator();
        _view.WriteLine($"Full Turns: {fullTurns}");
        _view.WriteLine($"Blinking Turns: {blinkingTurns}");
    }

    public void ShowActionOrder(List<Unit> actionOrder)
    {
        ShowSeparator();
        _view.WriteLine("Orden:");
        for (int i = 0; i < actionOrder.Count; i++)
        {
            _view.WriteLine($"{i + 1}-{actionOrder[i].Name}");
        }
    }

    public void ShowActionMenu(Unit unit, bool isSamurai)
    {
        _view.WriteLine(string.Format(GameConstants.Messages.SelectAction, unit.Name));
        
        if (isSamurai)
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

    public void ShowAttackResult(Unit attacker, Unit target, string attackType, AttackOutcome outcome, bool showFinalHP)
    {
        string actionVerb = GetAttackVerb(attackType);
        _view.WriteLine($"{attacker.Name} {actionVerb} {target.Name}");
        
        ShowAffinityMessage(target, outcome);
        ShowDamageOrEffect(target, outcome, attacker);
        
        if (showFinalHP)
        {
            if (outcome.IsRepelled)
            {
                _view.WriteLine($"{attacker.Name} termina con HP:{attacker.CurrentHP}/{attacker.BaseStats.HP}");
            }
            else
            {
                _view.WriteLine($"{target.Name} termina con HP:{target.CurrentHP}/{target.BaseStats.HP}");
            }
        }
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

    private void ShowAffinityMessage(Unit target, AttackOutcome outcome)
    {
        if (outcome.IsMissed)
        {
            _view.WriteLine($"{outcome.AttackerName} ha fallado el ataque");
            return;
        }

        if (outcome.IsInstantKill && outcome.IsRepelled)
        {
            return;
        }

        string affinityString = outcome.AffinityEffect.ToDisplayString();
        
        switch (affinityString)
        {
            case "Wk":
                _view.WriteLine($"{target.Name} es débil contra el ataque de {outcome.AttackerName}");
                break;
            case "Rs":
                if (!outcome.IsInstantKill && !outcome.IsMissed)
                {
                    _view.WriteLine($"{target.Name} es resistente el ataque de {outcome.AttackerName}");
                }
                break;
            case "Nu":
                _view.WriteLine($"{target.Name} bloquea el ataque de {outcome.AttackerName}");
                break;
        }
    }

    private void ShowDamageOrEffect(Unit target, AttackOutcome outcome, Unit attacker)
    {
        if (outcome.IsMissed || outcome.IsNullified)
        {
            return;
        }

        if (outcome.IsInstantKill)
        {
            _view.WriteLine($"{target.Name} ha sido eliminado");
        }
        else if (outcome.IsRepelled)
        {
            _view.WriteLine($"{target.Name} devuelve {outcome.DamageDealt} daño a {attacker.Name}");
        }
        else if (outcome.IsDrained)
        {
            _view.WriteLine($"{target.Name} absorbe {outcome.DamageDealt} daño");
        }
        else if (outcome.DamageDealt > 0)
        {
            _view.WriteLine($"{target.Name} recibe {outcome.DamageDealt} de daño");
        }
    }

    public void ShowTurnConsumption(TurnCost actualEffect)
    {
        ShowSeparator();
        _view.WriteLine($"Se han consumido {actualEffect.FullTurnsConsumed} Full Turn(s) y {actualEffect.BlinkingTurnsConsumed} Blinking Turn(s)");
        _view.WriteLine($"Se han obtenido {actualEffect.BlinkingTurnsGained} Blinking Turn(s)");
    }

    public void ShowWinner(string winnerName, string playerLabel)
    {
        ShowSeparator();
        _view.WriteLine(string.Format(GameConstants.Messages.Winner, $"{winnerName} ({playerLabel})"));
    }

    public void ShowTeamFiles(string[] teamFiles)
    {
        _view.WriteLine(GameConstants.Messages.ChooseTeamFile);
        for (int i = 0; i < teamFiles.Length; i++)
        {
            var fileName = Path.GetFileName(teamFiles[i]);
            _view.WriteLine($"{i}: {fileName}");
        }
    }

    public void ShowSkillOptions(List<Skill> skills)
    {
        if (skills.Count == 0)
        {
            _view.WriteLine($"1-{GameConstants.Messages.Cancel}");
        }
        else
        {
            for (int i = 0; i < skills.Count; i++)
            {
                _view.WriteLine($"{i + 1}-{skills[i].Name} MP:{skills[i].Cost}");
            }
            _view.WriteLine($"{skills.Count + 1}-{GameConstants.Messages.Cancel}");
        }
    }

    public void ShowTargetOptions(List<Unit> targets, string actingUnitName, bool includeCancel)
    {
        ShowSeparator();
        _view.WriteLine(string.Format(GameConstants.Messages.SelectTarget, actingUnitName));
        
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            _view.WriteLine($"{i + 1}-{target.Name} HP:{target.CurrentHP}/{target.BaseStats.HP} MP:{target.CurrentMP}/{target.BaseStats.MP}");
        }
        
        if (includeCancel)
        {
            _view.WriteLine($"{targets.Count + 1}-{GameConstants.Messages.Cancel}");
        }
    }

    public void ShowMonsterOptions(List<Monster> monsters)
    {
        ShowSeparator();
        _view.WriteLine(GameConstants.Messages.SelectMonster);
        
        for (int i = 0; i < monsters.Count; i++)
        {
            var monster = monsters[i];
            _view.WriteLine($"{i + 1}-{monster.Name} HP:{monster.CurrentHP}/{monster.BaseStats.HP} MP:{monster.CurrentMP}/{monster.BaseStats.MP}");
        }
        _view.WriteLine($"{monsters.Count + 1}-{GameConstants.Messages.Cancel}");
    }

    public void ShowPositionOptions(List<(int position, Unit? unit)> positions)
    {
        ShowSeparator();
        _view.WriteLine(GameConstants.Messages.SelectPosition);
        
        for (int i = 0; i < positions.Count; i++)
        {
            var (position, unit) = positions[i];
            if (unit == null)
            {
                _view.WriteLine($"{i + 1}-Vacío (Puesto {position + 1})");
            }
            else
            {
                _view.WriteLine($"{i + 1}-{unit.Name} HP:{unit.CurrentHP}/{unit.BaseStats.HP} MP:{unit.CurrentMP}/{unit.BaseStats.MP} (Puesto {position + 1})");
            }
        }
        _view.WriteLine($"{positions.Count + 1}-{GameConstants.Messages.Cancel}");
    }

    public string ReadInput()
    {
        return _view.ReadLine();
    }
}

