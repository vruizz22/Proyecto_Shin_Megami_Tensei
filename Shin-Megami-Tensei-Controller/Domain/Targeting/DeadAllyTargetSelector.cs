using Shin_Megami_Tensei.GameLogic;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Domain.Targeting;

public class DeadAllyTargetSelector : ITargetSelector
{
    public List<Unit> GetAvailableTargets(Unit actingUnit, Team allyTeam, Team enemyTeam)
    {
        var deadUnits = allyTeam.Reserve.Where(u => !u.IsAlive).ToList();
        
        // También incluir al samurai si está muerto
        var samurai = allyTeam.Board.FirstOrDefault(u => u != null && u is Samurai && !u.IsAlive);
        if (samurai != null)
        {
            deadUnits.Insert(0, samurai);
        }
        
        return deadUnits;
    }
}

