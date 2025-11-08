namespace Shin_Megami_Tensei.GameLogic;

using Models;
using Domain.ValueObjects;
using Domain.Combat;
using System.Collections.Generic;
using System.Linq;

public class MultiTargetSkillExecutionResult
{
    public List<SingleTargetResult> TargetResults { get; set; } = new();
    public TurnEffect CombinedTurnEffect { get; set; } = new();
    public List<Unit> AffectedUnits { get; set; } = new();
}

public class SingleTargetResult
{
    public Unit Target { get; set; } = null!;
    public RefactoredBattleEngine.AttackResult AttackResult { get; set; } = null!;
    public StatDrainEffect? DrainEffect { get; set; }
}

public class MultiTargetSkillExecutor
{
    private readonly RefactoredBattleEngine _battleEngine;

    public MultiTargetSkillExecutor(RefactoredBattleEngine battleEngine)
    {
        _battleEngine = battleEngine;
    }

    public MultiTargetSkillExecutionResult ExecuteOnAllTargets(
        Unit attacker,
        List<Unit> targets,
        Skill skill,
        int hitsPerTarget = 1)
    {
        var result = new MultiTargetSkillExecutionResult();
        var turnEffectToUse = new TurnEffect { FullTurnsConsumed = 1, BlinkingTurnsConsumed = 1, BlinkingTurnsGained = 0 };
        bool hasWeak = false;
        bool hasRepelled = false;

        foreach (var target in targets)
        {
            if (!target.IsAlive)
                continue;

            for (int i = 0; i < hitsPerTarget; i++)
            {
                var attackResult = _battleEngine.ExecuteAttack(attacker, target, skill.Type, skill.Power);
                
                var targetResult = new SingleTargetResult
                {
                    Target = target,
                    AttackResult = attackResult
                };

                // Handle drain effects for Almighty type
                if (skill.Type == "Almighty" && skill.Effect != null)
                {
                    if (skill.Effect.Contains("drains"))
                    {
                        string drainType = GetDrainType(skill.Effect);
                        targetResult.DrainEffect = StatDrainEffect.CalculateDrain(
                            attacker,
                            target,
                            attackResult.Damage,
                            drainType);
                    }
                }

                result.TargetResults.Add(targetResult);

                if (attackResult.TurnEffect.BlinkingTurnsGained > 0)
                {
                    hasWeak = true;
                }

                if (attackResult.WasRepelled)
                {
                    hasRepelled = true;
                }
            }
        }

        // Determinar el efecto de turno basado en los resultados
        if (hasRepelled && !hasWeak)
        {
            result.CombinedTurnEffect = new TurnEffect
            {
                FullTurnsConsumed = 0,
                BlinkingTurnsConsumed = 0,
                BlinkingTurnsGained = 0,
                ConsumeAllTurns = true
            };
        }
        else
        {
            result.CombinedTurnEffect = turnEffectToUse;
        }

        result.AffectedUnits = targets.Where(t => t.IsAlive || result.TargetResults.Any(r => r.Target == t)).ToList();

        return result;
    }

    public MultiTargetSkillExecutionResult ExecuteOnMultipleTargets(
        Unit attacker,
        List<Unit> allPossibleTargets,
        Skill skill,
        int totalHits)
    {
        var result = new MultiTargetSkillExecutionResult();
        var turnEffectToUse = new TurnEffect { FullTurnsConsumed = 1, BlinkingTurnsConsumed = 1, BlinkingTurnsGained = 0 };
        bool hasWeak = false;
        bool hasRepelled = false;

        var hitDistribution = DistributeHits(allPossibleTargets.Count, totalHits);
        
        for (int targetIndex = 0; targetIndex < allPossibleTargets.Count && targetIndex < hitDistribution.Count; targetIndex++)
        {
            var target = allPossibleTargets[targetIndex];
            int hitsForThisTarget = hitDistribution[targetIndex];

            for (int hitIndex = 0; hitIndex < hitsForThisTarget; hitIndex++)
            {
                var attackResult = _battleEngine.ExecuteAttack(attacker, target, skill.Type, skill.Power);
                
                var targetResult = new SingleTargetResult
                {
                    Target = target,
                    AttackResult = attackResult
                };

                // Manejar efectos de drenaje
                if (skill.Type == "Almighty" && skill.Effect != null && skill.Effect.Contains("drains"))
                {
                    string drainType = GetDrainType(skill.Effect);
                    targetResult.DrainEffect = StatDrainEffect.CalculateDrain(
                        attacker,
                        target,
                        attackResult.Damage,
                        drainType);
                }

                result.TargetResults.Add(targetResult);

                if (attackResult.TurnEffect.BlinkingTurnsGained > 0)
                {
                    hasWeak = true;
                }

                if (attackResult.WasRepelled)
                {
                    hasRepelled = true;
                }
            }
        }

        if (hasRepelled && !hasWeak)
        {
            result.CombinedTurnEffect = new TurnEffect
            {
                FullTurnsConsumed = 0,
                BlinkingTurnsConsumed = 0,
                BlinkingTurnsGained = 0,
                ConsumeAllTurns = true
            };
        }
        else
        {
            result.CombinedTurnEffect = turnEffectToUse;
        }

        result.AffectedUnits = allPossibleTargets;

        return result;
    }

    private List<int> DistributeHits(int targetCount, int totalHits)
    {
        var distribution = new List<int>();
        
        if (targetCount == 0)
            return distribution;

        int hitsPerTarget = totalHits / targetCount;
        int remainder = totalHits % targetCount;

        for (int i = 0; i < targetCount; i++)
        {
            int hits = hitsPerTarget;
            if (i < remainder)
            {
                hits++;
            }
            distribution.Add(hits);
        }

        return distribution;
    }

    private string GetDrainType(string effect)
    {
        if (effect.Contains("HP/MP") || effect.Contains("HP and MP"))
            return "HP/MP";
        if (effect.Contains("HP"))
            return "HP";
        if (effect.Contains("MP"))
            return "MP";
        return "";
    }
}

