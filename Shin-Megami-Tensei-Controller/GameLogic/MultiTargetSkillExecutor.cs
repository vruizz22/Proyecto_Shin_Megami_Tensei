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
        bool hasWeak = false;
        bool hasRepelled = false;
        bool hasNulled = false;
        bool hasMissed = false;

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

                // Manejar efectos de drenaje para tipo Almighty
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

                if (attackResult.WasNulled)
                {
                    hasNulled = true;
                }
                
                if (attackResult.Missed)
                {
                    hasMissed = true;
                }
            }
        }

        // Determinar efecto de turno basado en resultados
        // La prioridad de afinidades es: Repel/Drain > Null > Miss > Weak > Neutral/Resist
        if (hasRepelled)
        {
            // Repel/Drain tiene la máxima prioridad - consume todos los turnos
            // NO importa si hay Weak, Repel SIEMPRE consume todos los turnos
            result.CombinedTurnEffect = new TurnEffect
            {
                FullTurnsConsumed = 0,
                BlinkingTurnsConsumed = 0,
                BlinkingTurnsGained = 0,
                ConsumeAllTurns = true
            };
        }
        else if (hasNulled)
        {
            // Null tiene la mayor prioridad (después de Repel/Drain)
            // Consume 2 Blinking Turns (o Full Turns si no hay Blinking disponibles)
            // Los Weak NO otorgan Blinking Turns cuando hay Null
            result.CombinedTurnEffect = new TurnEffect
            {
                FullTurnsConsumed = 0,
                BlinkingTurnsConsumed = 2,
                BlinkingTurnsGained = 0
            };
        }
        else if (hasMissed)
        {
            // Miss tiene prioridad sobre Weak
            // Consume 1 Blinking Turn y NO otorga Blinking Turns aunque haya Weak
            result.CombinedTurnEffect = new TurnEffect
            {
                FullTurnsConsumed = 0,
                BlinkingTurnsConsumed = 1,
                BlinkingTurnsGained = 0
            };
        }
        else if (hasWeak)
        {
            // Weak: consume 1 Full Turn y otorga 1 Blinking Turn
            // Si no hay Full Turns, consume 1 Blinking Turn
            result.CombinedTurnEffect = new TurnEffect
            {
                FullTurnsConsumed = 1,
                BlinkingTurnsConsumed = 0,
                BlinkingTurnsGained = 1
            };
        }
        else
        {
            // Neutral/Resist: consume 1 Blinking Turn, si no hay consume 1 Full Turn
            result.CombinedTurnEffect = new TurnEffect
            {
                FullTurnsConsumed = 1,
                BlinkingTurnsConsumed = 1,
                BlinkingTurnsGained = 0
            };
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
        bool hasWeak = false;
        bool hasRepelled = false;
        bool hasNulled = false;
        bool hasMissed = false;

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

                if (attackResult.WasNulled)
                {
                    hasNulled = true;
                }
                
                if (attackResult.Missed)
                {
                    hasMissed = true;
                }
            }
        }

        // Determinar efecto de turno basado en resultados
        // La prioridad de afinidades es: Repel/Drain > Null > Miss > Weak > Neutral/Resist
        if (hasRepelled)
        {
            // Repel/Drain tiene la máxima prioridad - consume todos los turnos
            // NO importa si hay Weak, Repel SIEMPRE consume todos los turnos
            result.CombinedTurnEffect = new TurnEffect
            {
                FullTurnsConsumed = 0,
                BlinkingTurnsConsumed = 0,
                BlinkingTurnsGained = 0,
                ConsumeAllTurns = true
            };
        }
        else if (hasNulled)
        {
            // Null tiene la mayor prioridad (después de Repel/Drain)
            // Consume 2 Blinking Turns (o Full Turns si no hay Blinking disponibles)
            // Los Weak NO otorgan Blinking Turns cuando hay Null
            result.CombinedTurnEffect = new TurnEffect
            {
                FullTurnsConsumed = 0,
                BlinkingTurnsConsumed = 2,
                BlinkingTurnsGained = 0
            };
        }
        else if (hasMissed)
        {
            // Miss tiene prioridad sobre Weak
            // Consume 1 Blinking Turn y NO otorga Blinking Turns aunque haya Weak
            result.CombinedTurnEffect = new TurnEffect
            {
                FullTurnsConsumed = 0,
                BlinkingTurnsConsumed = 1,
                BlinkingTurnsGained = 0
            };
        }
        else if (hasWeak)
        {
            // Weak: consume 1 Full Turn y otorga 1 Blinking Turn
            // Si no hay Full Turns, consume 1 Blinking Turn
            result.CombinedTurnEffect = new TurnEffect
            {
                FullTurnsConsumed = 1,
                BlinkingTurnsConsumed = 0,
                BlinkingTurnsGained = 1
            };
        }
        else
        {
            // Neutral/Resist: consume 1 Blinking Turn, si no hay consume 1 Full Turn
            result.CombinedTurnEffect = new TurnEffect
            {
                FullTurnsConsumed = 1,
                BlinkingTurnsConsumed = 1,
                BlinkingTurnsGained = 0
            };
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
