using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.GameLogic;

public class BattleEngine
{
    private const double BaseAttackModifier = 54.0;
    private const double BaseGunModifier = 80.0;
    private const double DamageConstant = 0.0114;
    private const double WeakMultiplier = 1.5;
    private const double ResistMultiplier = 0.5;

    public class AttackResult
    {
        public int Damage { get; set; }
        public string AffinityEffect { get; set; } = "-";
        public bool WasRepelled { get; set; }
        public bool WasDrained { get; set; }
        public bool WasNulled { get; set; }
        public bool Missed { get; set; }
        public bool InstantKill { get; set; }
        public string AttackerName { get; set; } = "";
        public TurnManager.TurnEffect TurnEffect { get; set; } = new();
    }

    public AttackResult ExecuteAttack(Unit attacker, Unit target, string attackType, int? skillPower = null)
    {
        var result = new AttackResult { AttackerName = attacker.Name };
        
        string targetAffinity = target.Affinity.GetAffinityFor(attackType);
        result.AffinityEffect = targetAffinity;

        bool isLightOrDark = attackType == "Light" || attackType == "Dark";

        if (isLightOrDark)
        {
            return ExecuteInstantKillAttack(attacker, target, targetAffinity, skillPower ?? 0);
        }

        int baseDamage = CalculateBaseDamage(attacker, attackType, skillPower);
        ApplyAffinityEffects(attacker, target, targetAffinity, baseDamage, result);
        result.TurnEffect = CalculateTurnEffect(targetAffinity, result.Missed);

        return result;
    }

    private AttackResult ExecuteInstantKillAttack(Unit attacker, Unit target, string affinity, int skillPower)
    {
        var result = new AttackResult 
        { 
            AttackerName = attacker.Name,
            AffinityEffect = affinity 
        };

        switch (affinity)
        {
            case "Nu":
                result.WasNulled = true;
                result.TurnEffect = new TurnManager.TurnEffect 
                { 
                    FullTurnsConsumed = 2, 
                    BlinkingTurnsConsumed = 2, 
                    BlinkingTurnsGained = 0 
                };
                break;

            case "Wk":
                result.InstantKill = true;
                target.TakeDamage(target.CurrentHP);
                result.TurnEffect = new TurnManager.TurnEffect 
                { 
                    FullTurnsConsumed = 1, 
                    BlinkingTurnsConsumed = 0, 
                    BlinkingTurnsGained = 1 
                };
                break;

            case "Rs":
                bool resistSuccess = attacker.BaseStats.Lck + skillPower >= 2 * target.BaseStats.Lck;
                if (resistSuccess)
                {
                    result.InstantKill = true;
                    target.TakeDamage(target.CurrentHP);
                }
                else
                {
                    result.Missed = true;
                }
                result.TurnEffect = new TurnManager.TurnEffect 
                { 
                    FullTurnsConsumed = 1, 
                    BlinkingTurnsConsumed = 0, 
                    BlinkingTurnsGained = 0 
                };
                break;

            case "Rp":
                result.InstantKill = true;
                result.WasRepelled = true;
                attacker.TakeDamage(attacker.CurrentHP);
                result.TurnEffect = new TurnManager.TurnEffect 
                { 
                    ConsumeAllTurns = true 
                };
                break;

            default: // Neutral
                bool neutralSuccess = attacker.BaseStats.Lck + skillPower >= target.BaseStats.Lck;
                if (neutralSuccess)
                {
                    result.InstantKill = true;
                    target.TakeDamage(target.CurrentHP);
                }
                else
                {
                    result.Missed = true;
                }
                result.TurnEffect = new TurnManager.TurnEffect 
                { 
                    FullTurnsConsumed = 1, 
                    BlinkingTurnsConsumed = 0, 
                    BlinkingTurnsGained = 0 
                };
                break;
        }

        return result;
    }

    private int CalculateBaseDamage(Unit attacker, string attackType, int? skillPower)
    {
        int attackStat = GetAttackStat(attacker, attackType);
        double baseDamage;

        if (skillPower.HasValue)
        {
            baseDamage = Math.Sqrt(attackStat * skillPower.Value);
        }
        else
        {
            double modifier = attackType == "Gun" ? BaseGunModifier : BaseAttackModifier;
            baseDamage = attackStat * modifier * DamageConstant;
        }

        return (int)Math.Floor(baseDamage);
    }

    private int GetAttackStat(Unit attacker, string attackType)
    {
        return attackType switch
        {
            "Phys" => attacker.BaseStats.Str,
            "Gun" => attacker.BaseStats.Skl,
            "Fire" or "Ice" or "Elec" or "Force" or "Almighty" => attacker.BaseStats.Mag,
            _ => attacker.BaseStats.Str
        };
    }

    private void ApplyAffinityEffects(Unit attacker, Unit target, string affinity, int baseDamage, AttackResult result)
    {
        switch (affinity)
        {
            case "-":
                ApplyNeutralDamage(target, baseDamage, result);
                break;
            case "Wk":
                ApplyWeakDamage(target, baseDamage, result);
                break;
            case "Rs":
                ApplyResistDamage(target, baseDamage, result);
                break;
            case "Nu":
                ApplyNullEffect(result);
                break;
            case "Rp":
                ApplyRepelEffect(attacker, baseDamage, result);
                break;
            case "Dr":
                ApplyDrainEffect(target, baseDamage, result);
                break;
            default:
                ApplyNeutralDamage(target, baseDamage, result);
                break;
        }
    }

    private void ApplyNeutralDamage(Unit target, int baseDamage, AttackResult result)
    {
        int finalDamage = baseDamage;
        target.TakeDamage(finalDamage);
        result.Damage = finalDamage;
    }

    private void ApplyWeakDamage(Unit target, int baseDamage, AttackResult result)
    {
        int finalDamage = (int)Math.Floor(baseDamage * WeakMultiplier);
        target.TakeDamage(finalDamage);
        result.Damage = finalDamage;
    }

    private void ApplyResistDamage(Unit target, int baseDamage, AttackResult result)
    {
        int finalDamage = (int)Math.Floor(baseDamage * ResistMultiplier);
        target.TakeDamage(finalDamage);
        result.Damage = finalDamage;
    }

    private void ApplyNullEffect(AttackResult result)
    {
        result.Damage = 0;
        result.WasNulled = true;
    }

    private void ApplyRepelEffect(Unit attacker, int baseDamage, AttackResult result)
    {
        int reflectedDamage = baseDamage;
        attacker.TakeDamage(reflectedDamage);
        result.Damage = reflectedDamage;
        result.WasRepelled = true;
    }

    private void ApplyDrainEffect(Unit target, int baseDamage, AttackResult result)
    {
        int drainAmount = baseDamage;
        target.Heal(drainAmount);
        result.Damage = drainAmount;
        result.WasDrained = true;
    }

    private TurnManager.TurnEffect CalculateTurnEffect(string affinity, bool missed)
    {
        if (missed)
        {
            return new TurnManager.TurnEffect 
            { 
                FullTurnsConsumed = 1, 
                BlinkingTurnsConsumed = 1, 
                BlinkingTurnsGained = 0 
            };
        }

        return affinity switch
        {
            "Rp" or "Dr" => new TurnManager.TurnEffect 
            { 
                ConsumeAllTurns = true 
            },
            "Nu" => new TurnManager.TurnEffect 
            { 
                FullTurnsConsumed = 2, 
                BlinkingTurnsConsumed = 2, 
                BlinkingTurnsGained = 0 
            },
            "Wk" => new TurnManager.TurnEffect 
            { 
                FullTurnsConsumed = 1, 
                BlinkingTurnsConsumed = 0, 
                BlinkingTurnsGained = 1 
            },
            _ => new TurnManager.TurnEffect 
            { 
                FullTurnsConsumed = 1, 
                BlinkingTurnsConsumed = 0, 
                BlinkingTurnsGained = 0 
            }
        };
    }
}
