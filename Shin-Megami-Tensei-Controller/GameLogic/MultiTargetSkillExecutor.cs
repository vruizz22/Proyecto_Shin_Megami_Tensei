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
        bool hasDrained = false;
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
                
                if (attackResult.WasDrained)
                {
                    hasDrained = true;
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
        if (hasRepelled || hasDrained)
        {
            // Repel/Drain tiene la máxima prioridad - consume todos los turnos
            // NO importa si hay Weak, Repel/Drain SIEMPRE consume todos los turnos
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
        int totalHits,
        int skillCounter)
    {
        var result = new MultiTargetSkillExecutionResult();
        bool hasWeak = false;
        bool hasRepelled = false;
        bool hasDrained = false;
        bool hasNulled = false;
        bool hasMissed = false;

        // Algoritmo Multi: determinar qué unidades atacar y en qué orden
        var targetsToHit = SelectMultiTargets(allPossibleTargets, totalHits, skillCounter);
        
        foreach (var target in targetsToHit)
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
            
            if (attackResult.WasDrained)
            {
                hasDrained = true;
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

        // Determinar efecto de turno basado en resultados
        // La prioridad de afinidades es: Repel/Drain > Null > Miss > Weak > Neutral/Resist
        if (hasRepelled || hasDrained)
        {
            // Repel/Drain tiene la máxima prioridad - consume todos los turnos
            // NO importa si hay Weak, Repel/Drain SIEMPRE consume todos los turnos
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

    /// <summary>
    /// Implementa el algoritmo Multi del enunciado para seleccionar objetivos
    /// </summary>
    private List<Unit> SelectMultiTargets(List<Unit> availableTargets, int totalHits, int skillCounter)
    {
        var result = new List<Unit>();
        
        if (availableTargets.Count == 0 || totalHits == 0)
            return result;
        
        int A = availableTargets.Count;
        int K = skillCounter;
        int i = K % A;
        
        // Determinar dirección: derecha si i es par, izquierda si es impar
        bool moveRight = (i % 2 == 0);
        
        // Seleccionar objetivos
        int currentIndex = i;
        for (int hit = 0; hit < totalHits; hit++)
        {
            result.Add(availableTargets[currentIndex]);
            
            // Mover al siguiente objetivo si aún quedan hits
            if (hit < totalHits - 1)
            {
                if (moveRight)
                {
                    currentIndex++;
                    if (currentIndex >= A)
                        currentIndex = 0; // Envolver al inicio
                }
                else
                {
                    currentIndex--;
                    if (currentIndex < 0)
                        currentIndex = A - 1; // Envolver al final
                }
            }
        }
        
        return result;
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
