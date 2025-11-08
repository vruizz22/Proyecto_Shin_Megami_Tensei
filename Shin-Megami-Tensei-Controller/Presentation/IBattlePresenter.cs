using Shin_Megami_Tensei.Domain.ValueObjects;
using Shin_Megami_Tensei.GameLogic;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Presentation;

public interface IBattlePresenter
{
    void ShowSeparator();
    void ShowMessage(string message);
    void ShowRoundHeader(string samuraiName, string playerLabel);
    void ShowTeamsState(Team player1Team, Team player2Team);
    void ShowTurnInformation(int fullTurns, int blinkingTurns);
    void ShowActionOrder(List<Unit> actionOrder);
    void ShowActionMenu(Unit unit, bool isSamurai);
    void ShowAttackResult(Unit attacker, Unit target, string attackType, AttackOutcome outcome, bool showFinalHP);
    void ShowTurnConsumption(TurnCost actualEffect);
    void ShowWinner(string winnerName, string playerLabel);
    void ShowTeamFiles(string[] teamFiles);
    void ShowSkillOptions(List<Skill> skills);
    void ShowTargetOptions(List<Unit> targets, string actingUnitName, bool includeCancel);
    void ShowMonsterOptions(List<Monster> monsters);
    void ShowPositionOptions(List<(int position, Unit? unit)> positions);
    string ReadInput();
}

