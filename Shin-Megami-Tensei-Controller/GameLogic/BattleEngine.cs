using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.GameLogic;

public class BattleEngine
{
    public int CalculateDamage(Unit attacker, Unit target, string attackType, int? skillPower = null)
    {
        int attackStat = attacker.GetAttackStat(attackType);
        double baseDamage;

        if (skillPower.HasValue)
        {
            // Daño de habilidad: sqrt(stat * skillPower)
            baseDamage = Math.Sqrt(attackStat * skillPower.Value);
        }
        else
        {
            // Daño de ataque básico: stat * modificador * 0.0114
            int modifier = attackType == "Gun" ? 80 : 54; // 80 para Gun (Disparar), 54 para Phys (Atacar)
            baseDamage = attackStat * modifier * 0.0114;
        }

        // Aplicar modificadores de afinidad (para E1, solo neutral está implementado)
        string targetAffinity = target.Affinity.GetAffinityFor(attackType);
        double affinityMultiplier = GetAffinityMultiplier(targetAffinity);
        
        double finalDamage = baseDamage * affinityMultiplier;
        
        // Truncar a entero (Math.Floor)
        return Math.Max(0, (int)Math.Floor(finalDamage));
    }

    private double GetAffinityMultiplier(string affinity)
    {
        return affinity switch
        {
            "-" => 1.0,      // Neutral
            "Wk" => 1.5,     // Débil
            "Rs" => 0.5,     // Resistente
            "Nu" => 0.0,     // Nulo
            _ => 1.0         // Por defecto neutral para E1
        };
    }

    public void ExecuteAttack(Unit attacker, Unit target, string attackType, int? skillPower = null)
    {
        int damage = CalculateDamage(attacker, target, attackType, skillPower);
        target.TakeDamage(damage);
    }

    public bool ShouldUnitDie(Unit unit)
    {
        return unit.CurrentHP <= 0;
    }
}
