# 📋 Documentación Rider - Setup Completo para Proyecto Shin Megami Tensei

## 📖 Tabla de Contenidos

- [🤖 Instalación GitHub Copilot Pro en Rider](#-instalación-github-copilot-pro-en-rider)
- [🔧 Configuración del Debugger para .NET 8](#-configuración-del-debugger-para-net-8)
- [⚙️ Configuración Esencial del IDE](#️-configuración-esencial-del-ide)
- [🎯 Configuración Específica para el Proyecto](#-configuración-específica-para-el-proyecto)
- [🧪 Setup de Testing y Debugging](#-setup-de-testing-y-debugging)
- [🚀 Flujo de Trabajo Optimizado](#-flujo-de-trabajo-optimizado)

---

## 🤖 Instalación GitHub Copilot Pro en Rider

### 📋 Prerrequisitos

- ✅ JetBrains Rider instalado (versión 2024.1 o superior)
- ✅ Cuenta GitHub con Copilot Pro activo (estudiante)
- ✅ Internet estable para autenticación

### 🔧 Paso a Paso

#### **1️⃣ Instalar el Plugin de GitHub Copilot**

1. **Abrir Rider** y ir a:
   ```
   File → Settings (Ctrl+Alt+S en Windows/Linux)
   JetBrains Rider → Preferences (Cmd+, en macOS)
   ```

2. **Navegar a Plugins**:
   ```
   Settings → Plugins
   ```

3. **Buscar GitHub Copilot**:
   - En la pestaña "Marketplace"
   - Buscar: `GitHub Copilot`
   - Instalar el plugin oficial de **GitHub**

4. **Reiniciar Rider** cuando se solicite

#### **2️⃣ Autenticación con GitHub**

1. **Abrir Settings nuevamente**:
   ```
   File → Settings → Tools → GitHub Copilot
   ```

2. **Sign in to GitHub**:
   - Click en "Sign in to GitHub"
   - Se abrirá el navegador web
   - Autorizar JetBrains Rider en tu cuenta GitHub

3. **Verificar autenticación**:
   - Debe aparecer tu username: `vruizz22`
   - Status: "GitHub Copilot is active"

#### **3️⃣ Configurar Copilot Pro Features**

1. **Habilitar Copilot Chat** (equivalente al modo agente de VSCode):
   ```
   Settings → Tools → GitHub Copilot → Enable GitHub Copilot Chat
   ```

2. **Configurar sugerencias**:
   ```
   Settings → Tools → GitHub Copilot
   ✅ Enable completions
   ✅ Enable GitHub Copilot Chat
   ✅ Enable inline chat
   ✅ Show completions automatically
   ```

3. **Configurar teclas de acceso rápido**:
   ```
   Settings → Keymap → buscar "GitHub Copilot"
   
   Sugerencias recomendadas:
   - Accept Copilot Suggestion: Tab
   - Open Copilot Chat: Ctrl+Shift+X (personalizable)
   - Trigger Copilot: Ctrl+Alt+\ 
   ```

#### **4️⃣ Verificar Funcionamiento**

1. **Crear un archivo C# de prueba**:
   ```csharp
   // Escribe esto y espera sugerencias:
   public class TestCopilot
   {
       // Función que calcula el daño en Shin Megami Tensei
       public int CalculateDamage
   ```

2. **Probar Copilot Chat**:
   - Presionar `Ctrl+Shift+X` (o tu tecla configurada)
   - Escribir: "Explícame el sistema de afinidades de SMT"

---

## 🔧 Configuración del Debugger para .NET 8

### 🎯 Debugger Recomendado

Para tu proyecto Shin Megami Tensei en .NET 8, usa el **debugger integrado de Rider** que es superior al de VSCode para proyectos .NET.

### 🔧 Configuración Paso a Paso

#### **1️⃣ Configurar Debug Configurations**

1. **Abrir Run/Debug Configurations**:
   ```
   Run → Edit Configurations... 
   ```

2. **Crear configuración para el proyecto principal**:
   ```
   + → .NET Project
   
   Configuración:
   - Name: "SMT Main Game"
   - Project: Shin-Megami-Tensei-Controller
   - Target framework: net8.0
   - Program arguments: (vacío por ahora)
   - Working directory: ruta/del/proyecto
   ```

3. **Crear configuración para Tests**:
   ```
   + → .NET Project
   
   Configuración:
   - Name: "SMT Tests"
   - Project: Shin-Megami-Tensei.Tests
   - Target framework: net8.0
   ```

#### **2️⃣ Configurar Breakpoints Inteligentes**

1. **Habilitar breakpoints condicionales**:
   ```
   Settings → Build, Execution, Deployment → Debugger
   ✅ Enable data views
   ✅ Enable ToString() evaluation
   ✅ Show method return values
   ```

2. **Configurar Exception Breakpoints**:
   ```
   Run → View Breakpoints (Ctrl+Shift+F8)
   
   Habilitar:
   ✅ .NET Exception Breakpoints
   ✅ Any exception in user code
   ```

#### **3️⃣ Configurar Watch Variables Útiles**

Cuando debuggees el combate, agrega estas variables al watch:

```csharp
// Variables clave para debuggear SMT
player1.CurrentHP
player1.Team.ActiveMembers
currentTurn.AttackPower
damageCalculation.AffinityModifier
battleState.CurrentPhase
```

### 🎯 Breakpoints Estratégicos para SMT

Coloca breakpoints en estos puntos clave:

```csharp
// 1. Inicio de turno
public void StartTurn() { /* BREAKPOINT AQUÍ */ }

// 2. Cálculo de daño
public int CalculateDamage(Unit attacker, Unit target, Skill skill) 
{ /* BREAKPOINT AQUÍ */ }

// 3. Aplicación de afinidades
public float GetAffinityModifier(string skillType, string targetAffinity) 
{ /* BREAKPOINT AQUÍ */ }

// 4. Cambio de estado de unidad
public void TakeDamage(int damage) 
{ /* BREAKPOINT AQUÍ */ }
```

---

## ⚙️ Configuración Esencial del IDE

### 🎨 **1. Tema y Apariencia**

```
Settings → Appearance & Behavior → Appearance

Recomendado para largas sesiones de coding:
- Theme: Darcula (mejor para los ojos)
- Font: JetBrains Mono (optimizada para código)
- Font size: 14-16 (según tu pantalla)
```

### 📁 **2. Estructura de Proyecto**

```
Settings → Project → Project Structure

Configurar:
✅ Show solution folders
✅ Show all files
✅ Group by file type
```

### 🔍 **3. Navegación y Búsqueda**

```
Settings → Editor → General

Habilitar:
✅ Show breadcrumbs
✅ Show line numbers
✅ Show method separators
✅ Highlight current line
```

### 📝 **4. Editor de Código**

```
Settings → Editor → Code Style → C#

Configurar según estándares del proyecto:
- Indentation: 4 spaces
- Braces: Next line
- Line separator: System-dependent
```

### 🔧 **5. Tools Esenciales**

#### **Git Integration**
```
Settings → Version Control → Git

✅ Enable version control integration
✅ Show commit timestamp in blame
✅ Highlight modified lines in editor
```

#### **Database Tools** (para JSON data)
```
Settings → Database

Instalar plugin: Database Tools and SQL (viene incluido)
Útil para visualizar los JSON como tablas
```

#### **NuGet Package Manager**
```
Settings → NuGet

✅ Enable NuGet Package Management
✅ Auto-restore packages
```

---

## 🎯 Configuración Específica para el Proyecto

### 📊 **1. JSON Support Mejorado**

```
Plugins → Marketplace → Instalar:
- JSON Helper
- JSON Path Evaluator

Para manejar mejor:
- data/monsters.json
- data/samurai.json  
- data/skills.json
```

### 🧪 **2. Testing Framework**

```
Settings → Build, Execution, Deployment → Unit Testing

Configurar:
✅ Enable continuous testing
✅ Show code coverage
- Coverage runner: dotCover (incluido en Rider)
```

### 📈 **3. Performance Profiling**

```
Run → Profile → .NET Application

Para optimizar tu juego SMT:
- Memory profiler
- Performance profiler
- Timeline profiler
```

### 🔍 **4. Code Inspection**

```
Settings → Editor → Inspections

Habilitar inspecciones útiles para gaming:
✅ Null reference analysis
✅ Performance issues
✅ Memory allocation analysis
✅ LINQ suggestions
```

---

## 🧪 Setup de Testing y Debugging

### 🎯 **1. Configurar Test Runner**

```
Settings → Build, Execution, Deployment → Unit Testing

Configuración recomendada:
✅ Run tests in parallel
✅ Show output from tests
✅ Track running test
- Default timeout: 30 seconds (para tests de combate largos)
```

### 📊 **2. Test Templates**

Crear Live Templates para tests rápidos:

```
Settings → Editor → Live Templates → C#

Template "smt-test":
[Test]
public void Test_$METHOD$_Should_$EXPECTED$()
{
    // Arrange
    $ARRANGE$
    
    // Act
    $ACT$
    
    // Assert
    $ASSERT$
}
```

### 🔧 **3. Debug Templates**

Template para debug de combate:

```csharp
// Template "debug-combat"
Console.WriteLine($"Turn: {currentTurn}");
Console.WriteLine($"Attacker: {attacker.Name} (HP: {attacker.CurrentHP})");
Console.WriteLine($"Target: {target.Name} (HP: {target.CurrentHP})");
Console.WriteLine($"Skill: {skill.Name} (Power: {skill.Power})");
Console.WriteLine($"Expected Damage: {expectedDamage}");
```

### 📈 **4. Testing Workflow**

```bash
# Configurar estos Run Configurations:

1. "Test All" → dotnet test
2. "Test E1 Basic" → dotnet test --filter "BasicCombat"
3. "Test E1 Invalid" → dotnet test --filter "InvalidTeams"
4. "Test E1 Random" → dotnet test --filter "Random"
```

---

## 🚀 Flujo de Trabajo Optimizado

### 🔄 **1. Daily Workflow**

#### **Inicio del día**:
```
1. Ctrl+Shift+F12 → Restore layout
2. Alt+1 → Project view
3. Ctrl+T → Navigate to test que estás trabajando
4. F5 → Run current configuration
```

#### **Durante desarrollo**:
```
1. Ctrl+R, R → Run tests relacionados
2. Ctrl+R, T → Run all tests
3. F9 → Toggle breakpoint
4. Shift+F9 → Debug current
```

### 🎯 **2. Debugging Workflow para SMT**

```
Flujo recomendado para debuggear combate:

1. Coloca breakpoint en StartCombat()
2. F5 → Start debugging
3. F8 → Step over cada acción
4. F7 → Step into cálculos de daño
5. Alt+F9 → Run to cursor en puntos clave
6. Ctrl+F8 → Evaluate expression para verificar valores
```

### 📊 **3. Testing Workflow**

```
Para cada funcionalidad nueva:

1. Escribir test primero (TDD)
2. Ctrl+R, R → Run test (debería fallar)
3. Implementar funcionalidad
4. Ctrl+R, R → Run test (debería pasar)
5. Ctrl+R, T → Run all tests
6. Si pasan todos → commit
```

### 🔧 **4. Hotkeys Personalizados**

```
Settings → Keymap → Add custom shortcuts:

- Ctrl+Shift+B → Build Solution
- Ctrl+Shift+R → Run Tests in Context
- Ctrl+Shift+D → Debug Tests in Context
- Ctrl+Alt+L → Reformat Code
- Ctrl+Alt+I → Auto-indent lines
```

### 📱 **5. Layout Óptimo**

```
Window → Store Current Layout as Default

Layout recomendado:
- Left: Project view + Structure
- Center: Editor (tabs horizontales)
- Bottom: Debug/Test results/Terminal
- Right: Git changes + TODO
```

---

## 🎮 Configuraciones Adicionales para Gaming Project

### 🔊 **Audio Feedback**
```
Settings → Appearance & Behavior → System Settings

✅ Play sound on test completion
✅ Play sound on build completion
(Útil para saber cuándo terminan tests largos)
```

### 🎨 **Syntax Highlighting para JSON**
```
Settings → Editor → Color Scheme → JSON

Personalizar colores para:
- Monsters data → Green tones
- Samurai data → Blue tones  
- Skills data → Orange tones
```

### 📊 **Performance Monitoring**
```
Help → Diagnostic Tools → Performance Profiler

Para optimizar el juego:
- Memory usage during combat
- CPU spikes in damage calculation
- GC pressure analysis
```

---

¡Con esta configuración tendrás Rider optimizado para desarrollar tu proyecto Shin Megami Tensei como un profesional! 🚀