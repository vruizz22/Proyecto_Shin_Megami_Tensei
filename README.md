# 🎮 Proyecto Shin Megami Tensei - Guía Completa de Desarrollo

## 📋 Tabla de Contenidos

- [📖 Introducción](#-introducción)
- [🎯 Objetivos del Proyecto](#-objetivos-del-proyecto)
- [🏗️ Arquitectura del Proyecto](#️-arquitectura-del-proyecto)
- [⚙️ Configuración del Entorno](#️-configuración-del-entorno)
- [🚀 Primeros Pasos](#-primeros-pasos)
- [📁 Estructura de Archivos](#-estructura-de-archivos)
- [🎲 Reglas del Juego](#-reglas-del-juego)
- [🔧 Entrega 1 (E1) - Combat System](#-entrega-1-e1---combat-system)
- [🧪 Testing y Debugging](#-testing-y-debugging)
- [📝 Flujo de Trabajo](#-flujo-de-trabajo)
- [🎭 Datos del Juego](#-datos-del-juego)
- [🔍 Debugging Tips](#-debugging-tips)
- [📚 Referencias](#-referencias)

---

## 📖 Introducción

**Shin Megami Tensei** es una implementación en C# .NET 8 de un sistema de combate simplificado basado en el famoso JRPG desarrollado por Atlus. Este proyecto forma parte del curso **IIC2113 Diseño Detallado de Software** de la Pontificia Universidad Católica de Chile.

### 🎮 ¿Qué es Shin Megami Tensei?

- **Serie**: JRPG de la franquicia Megami Tensei
- **Desarrollador**: Atlus
- **Primer juego**: Digital Devil Story: Megami Tensei (1987)
- **Serie principal**: Inició con Shin Megami Tensei (1992)
- **Reconocimiento**: Conocido por la serie Persona y el personaje Joker en Super Smash Bros.

---

## 🎯 Objetivos del Proyecto

### 🎲 Sistema de Combate Press Turn

Implementar una versión simplificada del sistema de combate de **Shin Megami Tensei IV** donde:

- Dos equipos se enfrentan en combate por turnos
- Los jugadores explotan las debilidades de sus rivales
- Se evita que el oponente explote nuestras debilidades
- El objetivo es derrotar al equipo enemigo

### 🏆 Meta Principal

Crear un juego funcional que demuestre:

- **Diseño Orientado a Objetos** sólido
- **Patrones de Diseño** apropiados
- **Arquitectura limpia** y mantenible
- **Testing exhaustivo** y automatizado

---

## 🏗️ Arquitectura del Proyecto

### 📦 Estructura de Solución (.NET 8)

```
Shin-Megami-Tensei.sln
├── 🎮 Shin-Megami-Tensei-Controller/    # Lógica principal del juego
├── 👁️ Shin-Megami-Tensei-View/         # Interfaz de usuario y vistas
├── 🧪 Shin-Megami-Tensei.Tests/        # Pruebas unitarias
├── 📊 data/                            # Datos del juego (JSON)
└── 🧪 test/                           # Casos de prueba
```

### 🔧 Tecnologías Utilizadas

- **Framework**: .NET 8
- **Lenguaje**: C#
- **IDE**: JetBrains Rider
- **Testing**: Framework de testing de .NET
- **Arquitectura**: MVC (Model-View-Controller)

---

## ⚙️ Configuración del Entorno

### 📋 Prerrequisitos

1. **JetBrains Rider** instalado
2. **.NET 8 SDK** instalado
3. **Git** configurado

### 🔧 Setup Inicial

1. **Clonar/Abrir el proyecto en Rider**:

   ```bash
   cd /home/vruizz22/repositorios/dds/Proyecto_Shin_Megami_Tensei
   rider .
   ```

2. **Verificar que .NET 8 esté instalado**:

   ```bash
   dotnet --version
   # Debe mostrar 8.x.x
   ```

3. **Restaurar dependencias**:

   ```bash
   dotnet restore
   ```

4. **Compilar la solución**:

   ```bash
   dotnet build
   ```

### 🎯 Configuración de Rider

- **Target Framework**: .NET 8
- **Language**: C# 12.0
- **Project Structure**: Solution con múltiples proyectos
- **Testing Framework**: MSTest/NUnit (según configuración)

---

## 🚀 Primeros Pasos

### 1️⃣ Explorar la Estructura

```bash
# Ver la estructura principal
tree -d -L 2

# Explorar los datos del juego
ls -la data/
cat data/monsters.json | head -20
cat data/samurai.json | head -20
cat data skills.json | head -20
```

### 2️⃣ Ejecutar el Programa Principal

```bash
cd Shin-Megami-Tensei-Controller
dotnet run
```

### 3️⃣ Ejecutar Tests

```bash
cd Shin-Megami-Tensei.Tests
dotnet test
```

---

## 📁 Estructura de Archivos

### 🎮 **Shin-Megami-Tensei-Controller/**

- `Program.cs` - Punto de entrada del programa
- `Game.cs` - Lógica principal del juego
- **Responsabilidad**: Coordinar el flujo del juego y manejar la lógica principal

### 👁️ **Shin-Megami-Tensei-View/**

- `ConsoleLib/` - Librería de vistas para consola
  - `AbstractView.cs` - Clase base para vistas
  - `ConsoleView.cs` - Vista principal de consola
  - `ManualTestingView.cs` - Vista para testing manual
  - `TestingView.cs` - Vista para testing automatizado
  - `Script.cs` - Manejo de scripts de prueba
- **Responsabilidad**: Manejar toda la interacción con el usuario

### 🧪 **Shin-Megami-Tensei.Tests/**

- `Tests.cs` - Suite de pruebas principal
- **Responsabilidad**: Validar que todo funcione correctamente

### 📊 **data/** (Datos del Juego)

- `monsters.json` - Base de datos de monstruos
- `samurai.json` - Base de datos de samurai
- `skills.json` - Base de datos de habilidades
- `E1-**/` - Casos de prueba organizados por entrega

### 🧪 **test/** (Casos de Prueba)

- Réplica exacta de `data/` para testing
- Casos organizados por funcionalidad

---

## 🎲 Reglas del Juego

### 🏗️ **Setup del Equipo**

#### 👤 **Composición del Equipo**

- **1 Samurai** (obligatorio, líder del equipo)
- **Máximo 7 Monstruos** (opcional)
- **Total**: Mínimo 1 unidad (solo samurai), máximo 8 unidades

#### 🎯 **Reglas de Formación**

1. **Samurai único**: Solo un samurai por equipo
2. **Habilidades del Samurai**: Máximo 8 habilidades únicas
3. **Monstruos únicos**: No duplicados en el mismo equipo
4. **Monstruos compartidos**: Los oponentes pueden tener los mismos monstruos

#### 🎭 **Posicionamiento en el Tablero**

```
Tablero (4 posiciones activas):
[Samurai] [Monstruo 1] [Monstruo 2] [Monstruo 3]

Reserva:
[Monstruo 4] [Monstruo 5] [Monstruo 6] [Monstruo 7]
```

### 📊 **Ejemplo de Setup**

```
Jugador 1: Flynn (Samurai) + Jack Frost, Black Frost, King Frost, Frost Ace
Tablero: [Flynn] [Jack Frost] [Black Frost] [King Frost]
Reserva: [Frost Ace]

Jugador 2: Kei (Samurai) + Pyro Jack, Jack Ripper  
Tablero: [Kei] [Pyro Jack] [Jack Ripper] [Vacío]
Reserva: [Vacío]
```

---

## 🔧 Entrega 1 (E1) - Combat System

### 🎯 **Objetivos de la E1**

Implementar el sistema básico de combate que incluya:

#### ⚔️ **Sistema de Combate Básico**

1. **Turnos**: Alternar entre jugadores
2. **Acciones básicas**: Atacar, usar habilidades
3. **Daño**: Cálculo basado en stats y afinidades
4. **Victoria**: Determinar ganador cuando un equipo es derrotado

#### 📊 **Stats de las Unidades**

Cada unidad (Samurai/Monstruo) tiene:

```json
{
  "stats": {
    "HP": 133,     // Puntos de vida
    "MP": 58,      // Puntos de magia
    "Str": 15,     // Fuerza (ataques físicos)
    "Skl": 14,     // Habilidad
    "Mag": 14,     // Magia (ataques mágicos)
    "Spd": 18,     // Velocidad (orden de turnos)
    "Lck": 15      // Suerte
  }
}
```

#### 🔥 **Sistema de Afinidades**

```json
{
  "affinity": {
    "Phys": "-",    // Físico: - (neutral), Wk (débil), Rs (resistente), Nu (nulo)
    "Gun": "-",     // Armas
    "Fire": "Wk",   // Fuego
    "Ice": "-",     // Hielo
    "Elec": "-",    // Eléctrico
    "Force": "-",   // Viento
    "Light": "-",   // Luz
    "Dark": "-"     // Oscuridad
  }
}
```

#### ⚡ **Tipos de Habilidades**

```json
{
  "name": "Lunge",
  "type": "Phys",        // Tipo de daño
  "cost": 6,             // Costo en MP
  "power": 90,           // Poder base
  "target": "Single",    // Objetivo (Single/All)
  "hits": "1",           // Número de golpes
  "effect": "Weak Phys attack. Target: 1 enemy"
}
```

### 🏗️ **Implementación Paso a Paso**

#### **Fase 1: Modelos Base**

1. Crear clases para `Unit`, `Samurai`, `Monster`
2. Implementar `Stats` y `Affinity`
3. Crear `Skill` y sistema de habilidades

#### **Fase 2: Sistema de Equipos**

1. Implementar `Team` con validaciones
2. Sistema de `Board` y `Reserve`
3. Validar formación de equipos

#### **Fase 3: Sistema de Combate**

1. `BattleEngine` para manejar turnos
2. Cálculo de daño con afinidades
3. Sistema de targeting

#### **Fase 4: Interfaz de Usuario**

1. Mostrar estado del combate
2. Input de acciones del jugador
3. Mostrar resultados

---

## 🧪 Testing y Debugging

### 🔍 **Ejecutar Tests Específicos**

#### **Testing Manual con Program.cs**

```bash
cd Shin-Megami-Tensei-Controller
dotnet run

# El programa te preguntará:
# 1. Grupo de test (ej: E1-BasicCombat-Tests)
# 2. Test específico (ej: 006)
```

#### **Tipos de Tests Disponibles**

```
📁 E1-BasicCombat/           # Casos de combate básico
📁 E1-BasicCombat-Tests/     # Tests esperados
📁 E1-InvalidTeams/          # Casos de equipos inválidos  
📁 E1-InvalidTeams-Tests/    # Tests esperados
📁 E1-Random/                # Casos aleatorios
📁 E1-Random-Tests/          # Tests esperados
```

### 🎯 **Workflow de Testing**

1. **Seleccionar test case**: `E1-BasicCombat-Tests/006.txt`
2. **Ver input**: `data/E1-BasicCombat/006.txt`
3. **Ver output esperado**: `data/E1-BasicCombat-Tests/006.txt`
4. **Ejecutar y comparar**:
   - 🔵 **Azul**: Output correcto
   - 🔴 **Rojo**: Test falló

### 🧪 **Tests Automatizados**

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests específicos
dotnet test --filter "TestName~BasicCombat"

# Con verbose para más detalles
dotnet test --verbosity normal
```

---

## 📝 Flujo de Trabajo

### 🔄 **Ciclo de Desarrollo Diario**

#### 1️⃣ **Inicio del Día**

```bash
# Verificar estado del repositorio
git status
git pull origin master

# Compilar y verificar que todo funciona
dotnet build
dotnet test
```

#### 2️⃣ **Durante el Desarrollo**

```bash
# Crear rama para nueva feature (opcional)
git checkout -b feature/combat-system

# Desarrollo iterativo:
# 1. Escribir código
# 2. Compilar: dotnet build  
# 3. Probar: dotnet run
# 4. Testing: dotnet test
# 5. Repeat
```

#### 3️⃣ **Testing Específico**

```bash
# Probar caso específico que falla
cd Shin-Megami-Tensei-Controller
dotnet run
# Seleccionar: E1-BasicCombat-Tests -> 006

# Si falla, debuggear:
# 1. Ver input: cat ../data/E1-BasicCombat/006.txt
# 2. Ver output esperado: cat ../data/E1-BasicCombat-Tests/006.txt  
# 3. Comparar con tu output
```

#### 4️⃣ **Commit y Push**

```bash
# Verificar cambios
git status
git diff

# Commitear cambios
git add .
git commit -m "feat: implement basic combat system for E1"

# Push cambios
git push origin master
```

### 📋 **Checklist para cada Feature**

- [ ] ✅ Compila sin errores
- [ ] ✅ Pasa tests existentes  
- [ ] ✅ Implementa lógica correcta
- [ ] ✅ Maneja casos edge
- [ ] ✅ Código limpio y comentado
- [ ] ✅ Tests específicos pasan

---

## 🎭 Datos del Juego

### 👹 **Monsters (monsters.json)**

```json
{
  "name": "Jack Ripper",
  "stats": { "HP": 133, "MP": 58, "Str": 15, "Skl": 14, "Mag": 14, "Spd": 18, "Lck": 15 },
  "affinity": { "Phys": "-", "Gun": "-", "Fire": "Wk", "Ice": "-", "Elec": "-", "Force": "-", "Light": "-", "Dark": "-" }
}
```

### 🥷 **Samurai (samurai.json)**  

```json
{
  "name": "Shujinko",
  "stats": { "HP": 1317, "MP": 625, "Str": 304, "Skl": 250, "Mag": 80, "Spd": 170, "Lck": 130 },
  "affinity": { "Phys": "Rs", "Gun": "Rs", "Fire": "-", "Ice": "-", "Elec": "-", "Force": "-", "Light": "Nu", "Dark": "Nu" }
}
```

### ⚡ **Skills (skills.json)**

```json
{
  "name": "Lunge",
  "type": "Phys", 
  "cost": 6,
  "power": 90,
  "target": "Single",
  "hits": "1", 
  "effect": "Weak Phys attack. Target: 1 enemy"
}
```

### 🔍 **Códigos de Afinidad**

- `-` : Neutral (daño normal)
- `Wk` : Weak (daño aumentado)
- `Rs` : Resist (daño reducido)
- `Nu` : Null (sin daño)

---

## 🔍 Debugging Tips

### 🚨 **Problemas Comunes**

#### **Error de Compilación**

```bash
# Limpiar y reconstruir
dotnet clean
dotnet restore  
dotnet build
```

#### **Tests Fallan**

```bash
# Ejecutar test específico con detalle
dotnet test --verbosity normal --filter "TestName"

# Ver qué está fallando exactamente  
cd Shin-Megami-Tensei-Controller
dotnet run
# Probar case por case manualmente
```

#### **Problemas con JSON**

```bash
# Verificar formato JSON
cat data/monsters.json | jq '.[0]'  # Si tienes jq instalado
head -20 data/monsters.json         # Ver primeras líneas
```

### 🎯 **Strategy de Debugging**

1. **Identificar el test que falla**
2. **Ejecutar manualmente ese caso**
3. **Comparar input vs output esperado vs tu output**
4. **Debuggear línea por línea la lógica**
5. **Verificar edge cases**

### 🔧 **Herramientas de Rider**

- **Debugger**: Breakpoints y step-through
- **Unit Test Runner**: Ejecutar tests individuales
- **Console Output**: Ver prints y errores
- **Git Integration**: Ver cambios y commits

---

## 📚 Referencias

### 📖 **Documentación del Curso**

- `Proyecto_de_curso__2025_.pdf` - Especificación completa
- `Proyecto_E1__2025_1_.pdf` - Detalles de la Entrega 1

### 🎮 **Shin Megami Tensei Universe**

- **Serie Oficial**: Atlus Shin Megami Tensei
- **Spin-offs**: Persona series
- **Sistema Press Turn**: Mecánica original de SMT IV

### 💻 **Tecnologías**

- [.NET 8 Documentation](https://docs.microsoft.com/dotnet/)
- [C# Programming Guide](https://docs.microsoft.com/dotnet/csharp/)
- [JetBrains Rider](https://www.jetbrains.com/rider/)

---

## 🎯 **Siguiente Paso: ¡Comenzar la E1!**

### 🚀 **Para empezar ahora mismo:**

1. **Abrir Rider** y cargar la solución
2. **Ejecutar** `dotnet run` para ver el estado actual
3. **Revisar** algunos test cases en `data/E1-BasicCombat/`
4. **Comenzar** implementando las clases base del modelo
5. **Probar** continuamente con los casos de prueba

### 💪 **¡A Programar!**

¡Tienes toda la información necesaria para dominar este proyecto! El sistema está diseñado para aprender paso a paso, y cada test case te guiará hacia la implementación correcta.

**¡Que tengas un excelente desarrollo! 🎮⚔️**

---

**Creado con ❤️ para el curso IIC2113 - Diseño Detallado de Software**  
**PUC Chile - 2025**
