# 📖 GLOSARIO Y JUSTIFICACIONES TÉCNICAS

## Glosario de Términos

### Patrones de Diseño

#### **Strategy Pattern**
**Definición**: Encapsula algoritmos intercambiables en clases separadas.

**Dónde se aplicó**:
- `IAffinityEffect`: Algoritmos de cálculo de daño y efectos por afinidad
- `IInstantKillStrategy`: Algoritmos de probabilidad de instant kill
- `ITargetSelector`: Algoritmos de selección de objetivos

**Por qué**: Elimina switch/if anidados, permite agregar nuevas strategies sin modificar código existente.

---

#### **Factory Method Pattern**
**Definición**: Delega creación de objetos a métodos especializados.

**Dónde se aplicó**:
- `AffinityEffectFactory.Create()`: Crea strategy según AffinityType
- `InstantKillStrategyFactory.Create()`: Crea strategy según AffinityType
- `TurnCost.ConsumeAll()`, `TurnCost.ConsumeOneOfEither()`: Factory methods para casos comunes

**Por qué**: Centraliza lógica de creación, facilita testing con mocks.

---

#### **Bridge Pattern**
**Definición**: Separa abstracción (qué hacer) de implementación (cómo hacerlo).

**Dónde se aplicó**:
- `IBattlePresenter` (abstracción) ↔ `ConsoleBattlePresenter` (implementación)
- `GameManager` usa `IBattlePresenter`, no conoce si es consola, GUI o web

**Por qué**: Desacopla vista de controlador, cumple MVC, facilita cambiar UI sin tocar lógica.

---

#### **Value Object Pattern**
**Definición**: Objetos inmutables definidos por sus valores, no identidad.

**Dónde se aplicó**:
- `TurnCost`: Encapsula (FullTurnsConsumed, BlinkingTurnsConsumed, BlinkingTurnsGained)
- `AttackOutcome`: Encapsula resultado de ataque (daño, efectos, turnos)

**Por qué**: 
- Reduce argumentos de métodos (era 4+, ahora 1)
- Inmutabilidad previene bugs
- Claridad semántica

---

#### **Adapter Pattern**
**Definición**: Convierte interfaz de una clase en otra compatible.

**Dónde se aplicó**:
- `RefactoredBattleEngine`: Adapta `CombatResolver` (nueva arquitectura) a `BattleEngine.AttackResult` (interfaz legacy)

**Por qué**: Permite usar nuevo código sin romper código existente que espera formato viejo.

---

### Principios SOLID

#### **S - Single Responsibility Principle**
> "Una clase debe tener una sola razón para cambiar"

**Violaciones en código original**:
- ❌ `GameManager`: 7 responsabilidades (carga, control, ejecución, UI, presentación, targeting, validación)
- ❌ `BattleEngine`: Cálculo + afinidades + instant kill

**Solución**:
- ✅ `DamageCalculator`: Solo calcula daño base
- ✅ `AffinityEffect`: Solo maneja efectos de afinidad
- ✅ `InstantKillStrategy`: Solo maneja instant kill
- ✅ `IBattlePresenter`: Solo presenta información
- ✅ `BattleTurnManager`: Solo gestiona turnos

**Beneficio**: Cambios localizados, fácil de entender, testeable.

---

#### **O - Open/Closed Principle**
> "Abierto para extensión, cerrado para modificación"

**Violación en código original**:
```csharp
// Para agregar "Absorb", hay que modificar:
public void ApplyAffinityEffects(...)
{
    switch (affinity)
    {
        case "Wk": ...
        case "Rs": ...
        case "Nu": ...
        case "Rp": ...
        case "Dr": ...
        // ❌ Hay que agregar case "Ab" aquí
    }
}
```

**Solución**:
```csharp
// Para agregar "Absorb", solo crear clase nueva:
public class AbsorbAffinityEffect : IAffinityEffect { }

// Y registrar en factory:
case AffinityType.Absorb => _absorb
```

**Beneficio**: Nuevas features sin tocar código existente = menos bugs.

---

#### **L - Liskov Substitution Principle**
> "Subtipos deben ser sustituibles por sus tipos base"

**Aplicación**:
- ✅ Cualquier `IAffinityEffect` puede reemplazar a otro sin romper `CombatResolver`
- ✅ `WeakAffinityEffect` se comporta como `IAffinityEffect` promete

**Verificación**:
```csharp
IAffinityEffect effect = GetAnyEffect(); // Puede ser cualquiera
int damage = effect.CalculateDamage(100); // ✅ Funciona con todas
effect.ApplyEffect(attacker, target, damage); // ✅ Funciona con todas
```

**Beneficio**: Polimorfismo confiable, sin efectos secundarios inesperados.

---

#### **I - Interface Segregation Principle**
> "No forzar a implementar métodos innecesarios"

**Aplicación**:
```csharp
// ✅ Interface pequeña y específica
public interface IAffinityEffect
{
    int CalculateDamage(double baseDamage);
    void ApplyEffect(Unit attacker, Unit target, int damage);
    TurnCost GetTurnCost(bool isMiss);
    bool CanMiss();
}

// ❌ Hubiera sido malo:
public interface IGiantCombatInterface
{
    // 20 métodos que no todos usan
}
```

**Beneficio**: Clases solo implementan lo que necesitan.

---

#### **D - Dependency Inversion Principle**
> "Depender de abstracciones, no de concreciones"

**Violación en código original**:
```csharp
// ❌ GameManager depende de clase concreta View
private readonly View _view;
```

**Solución**:
```csharp
// ✅ GameManager depende de abstracción
private readonly IBattlePresenter _presenter;

// Puede ser ConsoleBattlePresenter, WebBattlePresenter, MockPresenter...
```

**Beneficio**: Fácil cambiar implementación, testeable con mocks.

---

### Clean Code

#### **Nombres Descriptivos**

**Reglas aplicadas**:
1. **Clases**: Sustantivo que describe su propósito
2. **Métodos**: Verbo que describe su acción
3. **Booleanos**: Prefijos Is/Has/Can/Should
4. **Métodos de retorno**: Get/Fetch/Retrieve (consistente)

**Cambios realizados**:
| Antes (❌) | Después (✅) | Razón |
|-----------|-------------|-------|
| `HandleSecondAction()` | `ExecuteGunAttackOrSkillAction()` | Más específico |
| `WasNulled` | `IsNullified` | Presente, no pasado |
| `LoadTeams()` | `TryLoadTeams()` | Indica que puede fallar |
| `ReadSkillSelection()` | `GetSelectedSkill()` | Consistencia en Get |

---

#### **Funciones Pequeñas**

**Regla**: Máximo 30 líneas ideal, nunca > 60.

**Aplicación**:
| Método | Antes | Después |
|--------|-------|---------|
| `ExecuteInstantKillAttack` | 134 líneas | 20 líneas |
| `ProcessPlayerTurn` | 52 líneas | < 30 líneas |
| `ApplyAffinityEffects` | 45 líneas | Reemplazado por 6 clases de < 15 líneas c/u |

**Técnica**: Extract Method + Strategy Pattern

---

#### **Argumentos Limitados**

**Regla**: Ideal 1-2, máximo 3, nunca > 3.

**Problemas originales**:
```csharp
// ❌ 4 argumentos
ExecuteInstantKillAttack(Unit attacker, Unit target, string affinity, int skillPower)

// ❌ 5 argumentos
ApplyAffinityEffects(Unit attacker, Unit target, string affinity, double baseDamage, AttackResult result)
```

**Solución**:
```csharp
// ✅ 3 argumentos + Value Object retornado
AttackOutcome ResolveAttack(Unit attacker, Unit target, ElementType element, int? skillPower = null)

// AttackOutcome encapsula 9 valores de retorno
```

**Técnica**: Parameter Object (Value Object)

---

#### **Indentación Máxima 2 Niveles**

**Problema original**:
```csharp
public AttackResult ExecuteInstantKillAttack(...)
{
    switch (affinity)  // Nivel 1
    {
        case "Nu":  // Nivel 2
            result.WasNulled = true;  // Nivel 3
            result.TurnEffect = new TurnManager.TurnEffect  // Nivel 3
            {
                FullTurnsConsumed = 0,  // Nivel 4 ❌
            };
            break;
    }
}
```

**Solución**:
```csharp
public class NullInstantKillStrategy : IInstantKillStrategy
{
    public bool TryExecute(Unit attacker, Unit target, int skillPower)  // Nivel 0
    {
        return false;  // Nivel 1 ✅
    }

    public TurnCost GetSuccessTurnCost()  // Nivel 0
    {
        return new TurnCost(0, 2, 0);  // Nivel 1 ✅
    }
}
```

**Técnica**: Extract Class + Polimorfismo elimina switches anidados

---

#### **Condiciones Encapsuladas**

**Problema original**:
```csharp
// ❌ Condición compleja en línea
if (attackType == "Light" || attackType == "Dark")
```

**Solución**:
```csharp
// ✅ Encapsulada en método semántico
public static bool IsInstantKillElement(this ElementType element)
{
    return element == ElementType.Light || element == ElementType.Dark;
}

// Uso:
if (element.IsInstantKillElement())
```

**Beneficio**: Claridad de intención, reusabilidad.

---

### Métricas de Código

#### **Complejidad Ciclomática**
**Definición**: Número de caminos independientes en el código.

**Fórmula**: `CC = E - N + 2P` (aristas - nodos + 2*componentes)

**Antes**: `ExecuteInstantKillAttack` tenía CC ≈ 15 (muy alta)
**Después**: Cada strategy tiene CC ≈ 2-3 (baja)

**Beneficio**: Más fácil de testear, menos bugs.

---

#### **Acoplamiento (Coupling)**
**Definición**: Grado de dependencia entre módulos.

**Antes**: `GameManager` acoplado a `View`, `BattleEngine`, `TurnManager`, modelos...
**Después**: `GameManager` acoplado a `IBattlePresenter`, `CombatResolver` (abstracciones)

**Métrica**: 
- Alto acoplamiento: Cambio en A rompe B, C, D...
- Bajo acoplamiento: Cambio en A no afecta a B, C, D

**Solución**: Dependency Inversion

---

#### **Cohesión (Cohesion)**
**Definición**: Grado en que elementos de un módulo están relacionados.

**Alta cohesión (✅)**:
- `DamageCalculator`: Todo relacionado con cálculo de daño
- `AffinityEffect`: Todo relacionado con efectos de afinidad

**Baja cohesión (❌)**:
- `GameManager` original: Carga archivos + control de flujo + ejecución + presentación (no relacionados)

**Métrica**: 
- Alta cohesión: Cambio en requisitos afecta solo una clase
- Baja cohesión: Cambio en requisitos afecta múltiples partes de una clase

---

## Respuestas a Preguntas Frecuentes de Defensa

### "¿Por qué Strategy y no Template Method?"

**Respuesta**:
- Template Method es útil cuando hay un algoritmo con pasos fijos pero variaciones.
- Strategy es útil cuando hay algoritmos completamente diferentes.
- Afinidades tienen lógica completamente diferente (Weak multiplica, Null ignora, Repel refleja).
- **Conclusión**: Strategy es más apropiado.

**Ejemplo donde sí usaría Template Method**:
```csharp
abstract class SkillExecutor
{
    // Template Method
    public void Execute()
    {
        ConsumeMP();      // Común
        SelectTarget();   // Común
        ApplyEffect();    // ← Varía (abstract)
        ShowResult();     // Común
    }

    protected abstract void ApplyEffect();
}
```

---

### "¿Por qué Value Object y no simplemente una clase?"

**Respuesta**:
- Value Objects son **inmutables** (readonly struct)
- Se comparan por **valor**, no por referencia
- No tienen identidad propia

**Ejemplo**:
```csharp
var cost1 = new TurnCost(1, 0, 1);
var cost2 = new TurnCost(1, 0, 1);
// cost1 == cost2 → true (mismo valor)

// Si fuera clase:
// cost1 == cost2 → false (diferentes referencias) ❌
```

**Beneficio**: 
- Previene modificación accidental
- Semánticamente correcto (un costo no "es" algo, "vale" algo)

---

### "¿Por qué no usar herencia en lugar de interfaces?"

**Respuesta**:
- C# no tiene herencia múltiple
- Interfaces son más flexibles
- Composición > Herencia (principio de diseño)

**Ejemplo**:
```csharp
// ✅ Con interfaces
public class CombatResolverWithLogging : ICombatResolver
{
    private ICombatResolver _inner; // Composición
}

// ❌ Con herencia
public class CombatResolverWithLogging : CombatResolver
{
    // Estoy atado a implementación de CombatResolver
    // No puedo decorar fácilmente
}
```

---

### "¿Esto no es over-engineering?"

**Respuesta**:
- **NO**, porque:
  1. Cumple requisitos de la pauta (SOLID, patrones, Clean Code)
  2. Escala bien (proyecto puede crecer)
  3. Ya tenías el problema (God Object de 814 líneas)
  4. Facilita testing (cada clase se testea independiente)

- **Sería over-engineering si**:
  1. Proyecto es de 200 líneas y agrego 50 clases
  2. Agrego patrones que no resuelven problemas reales
  3. Complico sin beneficio

**Justificación con pauta**:
- Cap 6 pide polimorfismo: +0.8 puntos
- Cap 6 pide Open/Closed: Requisito punto base
- Cap 10 pide SRP: +0.2 puntos
- MVC separado: +0.1 puntos

**Total beneficio**: ~+2.0 puntos por aplicar estos patrones

---

### "¿Cómo teseo esto?"

**Respuesta**: Es MÁS FÁCIL testear que antes.

**Ejemplo antes (difícil)**:
```csharp
// Para testear afinidad Weak, tengo que:
// 1. Crear GameManager completo
// 2. Mockear View
// 3. Cargar equipos
// 4. Simular combate entero
// 5. Verificar resultado final
```

**Ejemplo después (fácil)**:
```csharp
[Test]
public void WeakAffinity_MultipliesDamageBy1_5()
{
    // Arrange
    var effect = new WeakAffinityEffect();
    double baseDamage = 100;

    // Act
    int result = effect.CalculateDamage(baseDamage);

    // Assert
    Assert.Equal(150, result);
}
```

**Beneficio**: Unit tests simples, sin dependencias, rápidos.

---

## Checklist para Defensa

Antes de la presentación, asegúrate de poder explicar:

- [ ] Qué es Strategy Pattern y por qué lo usaste
- [ ] Qué es Open/Closed Principle y cómo lo cumples
- [ ] Por qué TurnCost es Value Object y no clase normal
- [ ] Cómo IBattlePresenter desacopla vista de controlador (Bridge)
- [ ] Qué problemas tenía ExecuteInstantKillAttack original
- [ ] Cómo agregarias una nueva afinidad ahora vs antes
- [ ] Por qué esto NO es over-engineering
- [ ] Cómo esto facilita testing

---

**Última actualización**: 2025-M11-08  
**Propósito**: Preparación para defensa oral y evaluación técnica

