using Shin_Megami_Tensei.GameLogic;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Targeting;

public class EnemyTargetSelector : ITargetSelector
{
    public List<Unit> GetAvailableTargets(Unit actingUnit, Team allyTeam, Team enemyTeam)
    {
        return enemyTeam.GetActiveUnitsOnBoard();
    }
}

