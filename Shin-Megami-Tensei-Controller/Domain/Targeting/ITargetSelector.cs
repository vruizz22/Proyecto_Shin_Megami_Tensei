using Shin_Megami_Tensei.GameLogic;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Targeting;

public interface ITargetSelector
{
    List<Unit> GetAvailableTargets(Unit actingUnit, Team allyTeam, Team enemyTeam);
}

