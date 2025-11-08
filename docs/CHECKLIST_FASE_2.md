# ✅ CHECKLIST FASE 2 - VERIFICACIÓN

## 📋 PASOS A SEGUIR AHORA

### Paso 1: Compilar ⏱️ 1 minuto
```bash
cd D:\Victor\Repositorios\dds\Proyecto_Shin_Megami_Tensei
dotnet build
```

**Marca aquí cuando lo hagas**: ⬜

**Resultado esperado**:
```
✅ Build succeeded.
   0 Warning(s)
   0 Error(s)
```

**Si falla**: Copia el error completo y envíamelo.

---

### Paso 2: Ejecutar Tests ⏱️ 2-3 minutos
```bash
dotnet test
```

**Marca aquí cuando lo hagas**: ⬜

**Anota los resultados**:
- Tests que pasan: ______ / ______
- Tests que fallan: ______

**Compara con baseline** (antes de Fase 2):
- ¿Pasan los mismos tests? ⬜ Sí / ⬜ No
- ¿Pasan más tests? ⬜ Sí / ⬜ No
- ¿Fallan tests nuevos? ⬜ Sí / ⬜ No

---

### Paso 3: Decidir Siguiente Acción

#### ✅ Si todos los tests que pasaban antes siguen pasando:
**Acción**: Responde con:
```
"Tests pasaron. Resultados: X/Y. Continuar Fase 3."
```

**Procederé a**: Cambiar `TurnManager` → `BattleTurnManager`

---

#### ⚠️ Si algunos tests fallan (que antes pasaban):
**Acción**: Responde con:
```
"Tests fallaron. Pego el output de los tests que fallan:"
[Pega aquí el output de dotnet test]
```

**Procederé a**: Ajustar `RefactoredBattleEngine` para corregir diferencias.

---

#### ❌ Si no compila:
**Acción**: Responde con:
```
"No compila. Errores:"
[Pega aquí el output de dotnet build]
```

**Procederé a**: Corregir error de compilación (probablemente falta using).

---

## 🔍 QUÉ REVISAR EN LOS TESTS

### Tests Críticos para Fase 2
Estos tests usan directamente BattleEngine (ahora RefactoredBattleEngine):

1. **E1-BasicCombat**: Ataques físicos básicos
   - Verifica cálculo de daño
   - Verifica afinidades neutrales

2. **E2-AffinityAndBasicSkills**: Afinidades y habilidades
   - Verifica Weak (1.5x damage)
   - Verifica Resist (0.5x damage)
   - Verifica Null (0 damage)

3. **E3-SingleTargetInstaKill**: Instant kill
   - Verifica Light/Dark con diferentes afinidades
   - Verifica cálculo de probabilidad

### Qué Buscar en el Output
Si hay fallos, busca patrones:

**Patrón 1: Diferencia de daño**
```
Expected: "HP:1048/1136"
Actual:   "HP:1047/1136"
```
→ Diferencia de 1 HP = problema de redondeo

**Patrón 2: Diferencia de turnos**
```
Expected: "Full Turns: 2"
Actual:   "Full Turns: 1"
```
→ Problema en conversión de TurnEffect

**Patrón 3: Mensajes diferentes**
```
Expected: "ha sido eliminado"
Actual:   "termina con HP:0"
```
→ Problema en flags IsInstantKill

---

## 🛠️ ROLLBACK RÁPIDO (si necesitas)

Si algo sale muy mal y quieres volver al estado anterior:

```bash
# Opción 1: Revertir solo GameManager.cs
git checkout GameLogic/GameManager.cs

# Opción 2: Ver cambios antes de revertir
git diff GameLogic/GameManager.cs

# Opción 3: Revertir todo (si hiciste commit antes)
git reset --hard HEAD
```

---

## 📊 TABLA DE DIAGNÓSTICO

| Síntoma | Causa Probable | Solución |
|---------|---------------|----------|
| No compila | Falta using | Agregar `using` statement |
| Tests fallan: daño ±1 | Redondeo | Ajustar Math.Floor/Ceiling |
| Tests fallan: turnos | Conversión TurnEffect | Revisar ConvertToLegacyTurnEffect |
| Tests fallan: instant kill | Flag IsInstantKill | Revisar condiciones en ResolveInstantKillAttack |
| Todos tests fallan | Problema mayor | Rollback y revisar |

---

## 🎯 EXPECTATIVA

### Resultado Esperado: ✅ TODOS LOS TESTS PASAN

**Por qué debería funcionar**:
1. RefactoredBattleEngine es un **adapter perfecto**
2. La lógica es **idéntica matemáticamente**
3. Las propiedades de AttackResult son **las mismas**
4. Los tests no acceden a **internals de BattleEngine**

**Confianza**: 95% de que funciona sin ajustes 🚀

---

## 📞 RESPUESTAS RÁPIDAS

**Para copiar y pegar según tu caso:**

### ✅ Caso A: Todo funciona
```
Tests pasaron. Resultados: X/Y (mismo que baseline). Continuar Fase 3.
```

### ⚠️ Caso B: Algunos fallan
```
Tests fallaron. Tests que pasaban antes y ahora fallan:
- TestE1_BasicCombat/001.txt
- TestE2_AffinityAndBasicSkills/005.txt

Output del test:
[pegar aquí]
```

### ❌ Caso C: No compila
```
No compila. Errores:
[pegar output de dotnet build]
```

---

## ⏰ TIEMPO ESTIMADO TOTAL

- Compilar: 1 min
- Ejecutar tests: 2-3 min
- Revisar resultados: 1 min
- Responderme: 1 min

**TOTAL**: ~5 minutos ⏱️

---

**Ahora es tu turno** → Ejecuta los comandos y respóndeme con los resultados. 

Estoy listo para la Fase 3 cuando confirmes que todo funciona. 💪

