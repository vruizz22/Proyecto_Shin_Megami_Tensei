# Guía Completa de Integración de la Interfaz Gráfica (GUI)

## Índice

1. [Introducción](#introducción)
2. [Requisitos Previos](#requisitos-previos)
3. [Paso 1: Instalación de Dependencias](#paso-1-instalación-de-dependencias)
4. [Paso 2: Agregar la Librería GUI](#paso-2-agregar-la-librería-gui)
5. [Paso 3: Crear las Interfaces Requeridas](#paso-3-crear-las-interfaces-requeridas)
6. [Paso 4: Implementar las Clases Adaptadoras](#paso-4-implementar-las-clases-adaptadoras)
7. [Paso 5: Modificar el Program.cs](#paso-5-modificar-el-programcs)
8. [Paso 6: Integrar con tu Lógica de Juego](#paso-6-integrar-con-tu-lógica-de-juego)
9. [Paso 7: Verificación y Testing](#paso-7-verificación-y-testing)
10. [Troubleshooting](#troubleshooting)

---

## Introducción

Esta guía te permitirá integrar la interfaz gráfica (GUI) de Shin Megami Tensei en tu proyecto actual, basándose en el repositorio de ejemplo funcional. La integración se realizará **sin romper los test cases existentes**, utilizando el patrón de diseño de interfaces para mantener compatibilidad con ambos modos (consola y GUI).

---

## Requisitos Previos

- Proyecto Shin Megami Tensei con estructura MVC
- JetBrains Rider como IDE
- .NET 6.0 o superior
- Tests funcionando correctamente

---

## Paso 1: Instalación de Dependencias

### 1.1 Abrir NuGet Package Manager

En JetBrains Rider:

1. Click derecho en el proyecto `Shin-Megami-Tensei-View`
2. Selecciona **"Manage NuGet Packages"**

### 1.2 Instalar Paquetes Requeridos

Instala los siguientes paquetes NuGet en el proyecto `Shin-Megami-Tensei-View`:

- **Avalonia** (versión 11.0.0 o superior)
- **Avalonia.Desktop** (versión 11.0.0 o superior)
- **Avalonia.Themes.Fluent** (versión 11.0.0 o superior)

```xml
<!-- Verificar en Shin-Megami-Tensei-View.csproj -->
<ItemGroup>
    <PackageReference Include="Avalonia" Version="11.0.0" />
    <PackageReference Include="Avalonia.Desktop" Version="11.0.0" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.0" />
</ItemGroup>
```

---

## Paso 2: Agregar la Librería GUI

### 2.1 Copiar la Carpeta GuiLib

1. Descarga o copia la carpeta `GuiLib` proporcionada por los profesores
2. Colócala dentro del proyecto `Shin-Megami-Tensei-View`

Tu estructura debe quedar:

```
Shin-Megami-Tensei-View/
├── GuiLib/
│   ├── Shin-Megami-Tensei-GUI.dll
│   └── Shin-Megami-Tensei-GUI.deps.json
│   └── [otros archivos...]
├── ConsoleLib/
└── Shin-Megami-Tensei-View.csproj
```

### 2.2 Agregar Referencia a la DLL en Shin-Megami-Tensei-View

1. Click derecho en **Dependencies** del proyecto `Shin-Megami-Tensei-View`
2. Selecciona **"Add Reference..."** o **"Add From..."**
3. Navega hasta `Shin-Megami-Tensei-View/GuiLib/`
4. Selecciona `Shin-Megami-Tensei-GUI.dll`
5. Click en **"OK"**

### 2.3 Agregar Referencia en Shin-Megami-Tensei-Controller

Repite el mismo proceso para el proyecto `Shin-Megami-Tensei-Controller`:

1. Click derecho en **Dependencies** del proyecto `Shin-Megami-Tensei-Controller`
2. **"Add Reference..."** → **"Add From..."**
3. Selecciona la misma DLL: `Shin-Megami-Tensei-View/GuiLib/Shin-Megami-Tensei-GUI.dll`

### 2.4 Verificación

Verifica que puedes importar la librería agregando en cualquier archivo:

```csharp
using Shin_Megami_Tensei_GUI;
```

---

## Paso 3: Crear las Interfaces Requeridas

### 3.1 Crear IUnit en el Controller

En el proyecto `Shin-Megami-Tensei-Controller`, crea una clase que implemente `IUnit`:

**Archivo:** `Shin-Megami-Tensei-Controller/Models/UnitAdapter.cs`

```csharp
using Shin_Megami_Tensei_GUI;

namespace Shin_Megami_Tensei_Controller.Models
{
    /// <summary>
    /// Adaptador que conecta la clase Unit del dominio con la interfaz IUnit de la GUI
    /// </summary>
    public class UnitAdapter : IUnit
    {
        private readonly Unit _unit;

        public UnitAdapter(Unit unit)
        {
            _unit = unit;
        }

        public string Name => _unit.Name;
        public int HP => _unit.Stats.CurrentHp;
        public int MP => _unit.Stats.CurrentMp;
        public int MaxHP => _unit.Stats.MaxHp;
        public int MaxMP => _unit.Stats.MaxMp;

        // Método auxiliar para obtener la unidad original
        public Unit GetOriginalUnit() => _unit;
    }
}
```

### 3.2 Crear IPlayer en el Controller

**Archivo:** `Shin-Megami-Tensei-Controller/GameLogic/PlayerAdapter.cs`

```csharp
using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei_Controller.Models;

namespace Shin_Megami_Tensei_Controller.GameLogic
{
    /// <summary>
    /// Adaptador que conecta la clase Team con la interfaz IPlayer de la GUI
    /// </summary>
    public class PlayerAdapter : IPlayer
    {
        private readonly Team _team;
        private IUnit?[] _unitsInBoard;
        private IEnumerable<IUnit> _unitsInReserve;

        public PlayerAdapter(Team team)
        {
            _team = team;
            UpdateUnits();
        }

        public IUnit?[] UnitsInBoard => _unitsInBoard;
        public IEnumerable<IUnit> UnitsInReserve => _unitsInReserve;

        /// <summary>
        /// Actualiza la representación de las unidades para la GUI
        /// </summary>
        public void UpdateUnits()
        {
            // Convertir unidades en el tablero
            _unitsInBoard = new IUnit?[3];
            for (int i = 0; i < _team.UnitsInBoard.Length && i < 3; i++)
            {
                _unitsInBoard[i] = _team.UnitsInBoard[i] != null 
                    ? new UnitAdapter(_team.UnitsInBoard[i]) 
                    : null;
            }

            // Convertir unidades en reserva
            _unitsInReserve = _team.UnitsInReserve
                .Where(unit => unit != null)
                .Select(unit => new UnitAdapter(unit))
                .ToList();
        }

        public Team GetOriginalTeam() => _team;
    }
}
```

### 3.3 Crear IState en el Controller

**Archivo:** `Shin-Megami-Tensei-Controller/GameLogic/GameStateAdapter.cs`

```csharp
using Shin_Megami_Tensei_GUI;

namespace Shin_Megami_Tensei_Controller.GameLogic
{
    /// <summary>
    /// Adaptador que representa el estado completo del juego para la GUI
    /// </summary>
    public class GameStateAdapter : IState
    {
        public IPlayer Player1 { get; set; }
        public IPlayer Player2 { get; set; }
        public IEnumerable<string> Options { get; set; }
        public int Turns { get; set; }
        public int BlinkingTurns { get; set; }
        public IEnumerable<string> Order { get; set; }

        public GameStateAdapter(
            PlayerAdapter player1, 
            PlayerAdapter player2,
            IEnumerable<string> options,
            int turns,
            int blinkingTurns,
            IEnumerable<string> order)
        {
            Player1 = player1;
            Player2 = player2;
            Options = options;
            Turns = turns;
            BlinkingTurns = blinkingTurns;
            Order = order;
        }

        /// <summary>
        /// Actualiza las unidades de ambos jugadores
        /// </summary>
        public void RefreshUnits()
        {
            if (Player1 is PlayerAdapter p1)
                p1.UpdateUnits();
            if (Player2 is PlayerAdapter p2)
                p2.UpdateUnits();
        }
    }
}
```

---

## Paso 4: Implementar las Clases Adaptadoras

### 4.1 Crear la Vista GUI

**Archivo:** `Shin-Megami-Tensei-Controller/Presentation/GuiPresenter.cs`

```csharp
using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei_Controller.GameLogic;
using Shin_Megami_Tensei_Controller.Data;

namespace Shin_Megami_Tensei_Controller.Presentation
{
    /// <summary>
    /// Presentador que maneja toda la interacción con la GUI
    /// </summary>
    public class GuiPresenter
    {
        private readonly SMTGUI _gui;
        private GameStateAdapter? _currentState;

        public GuiPresenter()
        {
            _gui = new SMTGUI();
        }

        /// <summary>
        /// Inicia la ventana GUI
        /// </summary>
        public void Start(Action gameLoop)
        {
            _gui.Start(gameLoop);
        }

        /// <summary>
        /// Obtiene la información del equipo seleccionado por el usuario
        /// </summary>
        public (string samuraiName, string[] skillNames, string[] demonNames) GetTeamInfo(int playerId)
        {
            ITeamInfo teamInfo = _gui.GetTeamInfo(playerId);
            return (teamInfo.SamuraiName, teamInfo.SkillNames, teamInfo.DemonNames);
        }

        /// <summary>
        /// Actualiza la ventana con el estado actual del juego
        /// </summary>
        public void UpdateGameState(GameStateAdapter state)
        {
            _currentState = state;
            _currentState.RefreshUnits();
            _gui.Update(_currentState);
        }

        /// <summary>
        /// Muestra un mensaje de fin de juego
        /// </summary>
        public void ShowEndGameMessage(string message)
        {
            _gui.ShowEndGameMessage(message);
        }

        /// <summary>
        /// Espera hasta que el usuario haga click en un elemento específico
        /// </summary>
        public IClickedElement WaitForClick()
        {
            return _gui.GetClickedElement();
        }

        /// <summary>
        /// Espera hasta que el usuario seleccione un botón específico
        /// </summary>
        public IClickedElement WaitForButton(string buttonText)
        {
            IClickedElement clickedElement;
            do
            {
                clickedElement = _gui.GetClickedElement();
            } while (!(clickedElement.Type == ClickedElementType.Button 
                      && clickedElement.Text == buttonText));
            
            return clickedElement;
        }

        /// <summary>
        /// Espera hasta que el usuario seleccione una unidad en el tablero
        /// </summary>
        public IClickedElement WaitForUnitInBoard()
        {
            IClickedElement clickedElement;
            do
            {
                clickedElement = _gui.GetClickedElement();
            } while (clickedElement.Type != ClickedElementType.UnitInBoard);
            
            return clickedElement;
        }

        /// <summary>
        /// Espera hasta que el usuario seleccione una unidad en reserva
        /// </summary>
        public IClickedElement WaitForUnitInReserve()
        {
            IClickedElement clickedElement;
            do
            {
                clickedElement = _gui.GetClickedElement();
            } while (clickedElement.Type != ClickedElementType.UnitInReserve);
            
            return clickedElement;
        }
    }
}
```

### 4.2 Crear Utilidad de Conversión de Equipos

**Archivo:** `Shin-Megami-Tensei-Controller/Data/TeamInfoConverter.cs`

```csharp
using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei_Controller.GameLogic;

namespace Shin_Megami_Tensei_Controller.Data
{
    /// <summary>
    /// Convierte la información de equipos de la GUI al formato del juego
    /// </summary>
    public static class TeamInfoConverter
    {
        /// <summary>
        /// Crea un equipo a partir de la información de la GUI
        /// </summary>
        public static Team CreateTeamFromGuiInfo(
            ITeamInfo teamInfo, 
            DataLoader dataLoader)
        {
            // Obtener el samurai
            var samurai = dataLoader.GetSamuraiByName(teamInfo.SamuraiName);
            if (samurai == null)
            {
                throw new InvalidOperationException(
                    $"Samurai '{teamInfo.SamuraiName}' no encontrado");
            }

            // Obtener las habilidades
            var skills = teamInfo.SkillNames
                .Select(skillName => dataLoader.GetSkillByName(skillName))
                .Where(skill => skill != null)
                .ToList();

            // Obtener los demonios
            var demons = teamInfo.DemonNames
                .Select(demonName => dataLoader.GetMonsterByName(demonName))
                .Where(demon => demon != null)
                .ToList();

            // Crear el equipo
            return new Team(samurai, skills, demons);
        }

        /// <summary>
        /// Valida que el equipo seleccionado sea válido
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateTeamInfo(
            ITeamInfo teamInfo, 
            DataLoader dataLoader)
        {
            // Validar samurai
            var samurai = dataLoader.GetSamuraiByName(teamInfo.SamuraiName);
            if (samurai == null)
            {
                return (false, $"El samurai '{teamInfo.SamuraiName}' no existe");
            }

            // Validar habilidades
            if (teamInfo.SkillNames.Length > 6)
            {
                return (false, "No puedes tener más de 6 habilidades");
            }

            foreach (var skillName in teamInfo.SkillNames)
            {
                var skill = dataLoader.GetSkillByName(skillName);
                if (skill == null)
                {
                    return (false, $"La habilidad '{skillName}' no existe");
                }
            }

            // Validar demonios
            if (teamInfo.DemonNames.Length < 1 || teamInfo.DemonNames.Length > 8)
            {
                return (false, "Debes tener entre 1 y 8 demonios");
            }

            foreach (var demonName in teamInfo.DemonNames)
            {
                var demon = dataLoader.GetMonsterByName(demonName);
                if (demon == null)
                {
                    return (false, $"El demonio '{demonName}' no existe");
                }
            }

            return (true, string.Empty);
        }
    }
}
```

---

## Paso 5: Modificar el Program.cs

### 5.1 Estructura del Program.cs con Soporte GUI

**Archivo:** `Shin-Megami-Tensei-Controller/Program.cs`

```csharp
using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Controller;
using Shin_Megami_Tensei_Controller.Presentation;
using Shin_Megami_Tensei_Controller.GameLogic;

// ==========================================
// CONFIGURACIÓN: Cambiar para alternar modos
// ==========================================
bool useGui = true;  // Cambiar a false para modo consola
// ==========================================

if (useGui)
{
    RunGuiMode();
}
else
{
    RunConsoleMode();
}

void RunGuiMode()
{
    var guiPresenter = new GuiPresenter();
    guiPresenter.Start(GameLoopWithGui);

    void GameLoopWithGui()
    {
        try
        {
            var game = new GameManager(guiPresenter);
            game.StartGame();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en el juego: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}

void RunConsoleMode()
{
    string testFolder = SelectTestFolder();
    string test = SelectTest(testFolder);
    string teamsFolder = testFolder.Replace("-Tests", "");
    AnnounceTestCase(test);

    var view = View.BuildManualTestingView(test);
    var game = new Game(view, teamsFolder);
    game.Play();
}

// ==========================================
// Funciones auxiliares para modo consola
// ==========================================

string SelectTestFolder()
{
    Console.WriteLine("¿Qué grupo de test quieres usar?");
    string[] dirs = Directory.GetDirectories("data", "*-Tests", SearchOption.TopDirectoryOnly);
    Array.Sort(dirs);
    
    for (int i = 0; i < dirs.Length; i++)
        Console.WriteLine($"{i}- {dirs[i]}");
    
    int selectedIndex = AskUserToSelectNumber(0, dirs.Length - 1);
    return dirs[selectedIndex];
}

string SelectTest(string testFolder)
{
    Console.WriteLine("¿Qué test quieres ejecutar?");
    string[] tests = Directory.GetFiles(testFolder, "*.txt");
    Array.Sort(tests);
    
    for (int i = 0; i < tests.Length; i++)
        Console.WriteLine($"{i}- {tests[i]}");
    
    int selectedIndex = AskUserToSelectNumber(0, tests.Length - 1);
    return tests[selectedIndex];
}

int AskUserToSelectNumber(int minValue, int maxValue)
{
    Console.WriteLine($"(Ingresa un número entre {minValue} y {maxValue})");
    int value;
    bool wasParsePossible;
    do
    {
        string? userInput = Console.ReadLine();
        wasParsePossible = int.TryParse(userInput, out value);
    } while (!wasParsePossible || value < minValue || value > maxValue);

    return value;
}

void AnnounceTestCase(string test)
{
    Console.WriteLine("----------------------------------------");
    Console.WriteLine($"Replicando test: {test}");
    Console.WriteLine("----------------------------------------\n");
}
```

---

## Paso 6: Integrar con tu Lógica de Juego

### 6.1 Modificar GameManager para Soportar GUI

**Archivo:** `Shin-Megami-Tensei-Controller/GameLogic/GameManager.cs`

```csharp
using Shin_Megami_Tensei_Controller.Presentation;
using Shin_Megami_Tensei_Controller.Data;
using Shin_Megami_Tensei_GUI;

namespace Shin_Megami_Tensei_Controller.GameLogic
{
    public class GameManager
    {
        private readonly GuiPresenter _gui;
        private readonly DataLoader _dataLoader;
        private Team _team1;
        private Team _team2;
        private PlayerAdapter _player1Adapter;
        private PlayerAdapter _player2Adapter;
        private GameStateAdapter _gameState;
        private int _currentTurn;

        public GameManager(GuiPresenter guiPresenter)
        {
            _gui = guiPresenter;
            _dataLoader = new DataLoader("data"); // Ajustar la ruta según tu proyecto
            _currentTurn = 0;
        }

        public void StartGame()
        {
            // 1. Seleccionar equipos
            if (!SelectTeams())
            {
                _gui.ShowEndGameMessage("Al menos un equipo es inválido");
                return;
            }

            // 2. Inicializar estado del juego
            InitializeGameState();

            // 3. Mostrar estado inicial
            _gui.UpdateGameState(_gameState);

            // 4. Loop principal del juego
            GameLoop();
        }

        private bool SelectTeams()
        {
            // Obtener equipo 1
            var team1Info = _gui.GetTeamInfo(1);
            var (isValid1, error1) = TeamInfoConverter.ValidateTeamInfo(
                CreateTeamInfo(team1Info), _dataLoader);
            
            if (!isValid1)
            {
                _gui.ShowEndGameMessage($"Equipo 1 inválido: {error1}");
                return false;
            }

            // Obtener equipo 2
            var team2Info = _gui.GetTeamInfo(2);
            var (isValid2, error2) = TeamInfoConverter.ValidateTeamInfo(
                CreateTeamInfo(team2Info), _dataLoader);
            
            if (!isValid2)
            {
                _gui.ShowEndGameMessage($"Equipo 2 inválido: {error2}");
                return false;
            }

            // Crear equipos
            _team1 = TeamInfoConverter.CreateTeamFromGuiInfo(
                CreateTeamInfo(team1Info), _dataLoader);
            _team2 = TeamInfoConverter.CreateTeamFromGuiInfo(
                CreateTeamInfo(team2Info), _dataLoader);

            return true;
        }

        private ITeamInfo CreateTeamInfo((string samuraiName, string[] skillNames, string[] demonNames) info)
        {
            // Crear un objeto anónimo que implemente ITeamInfo
            return new TeamInfoWrapper(info.samuraiName, info.skillNames, info.demonNames);
        }

        private void InitializeGameState()
        {
            _player1Adapter = new PlayerAdapter(_team1);
            _player2Adapter = new PlayerAdapter(_team2);

            var initialOptions = new List<string>
            {
                "Atacar",
                "Disparar",
                "Usar Habilidad",
                "Invocar",
                "Pasar Turno"
            };

            var initialOrder = CalculateInitialOrder();

            _gameState = new GameStateAdapter(
                _player1Adapter,
                _player2Adapter,
                initialOptions,
                turns: 0,
                blinkingTurns: 0,
                order: initialOrder
            );
        }

        private List<string> CalculateInitialOrder()
        {
            // Implementar la lógica para calcular el orden de turnos
            var allUnits = new List<(string name, int speed)>();

            foreach (var unit in _team1.UnitsInBoard.Where(u => u != null))
            {
                allUnits.Add((unit.Name, unit.Stats.Speed));
            }

            foreach (var unit in _team2.UnitsInBoard.Where(u => u != null))
            {
                allUnits.Add((unit.Name, unit.Stats.Speed));
            }

            return allUnits
                .OrderByDescending(u => u.speed)
                .Select(u => u.name)
                .ToList();
        }

        private void GameLoop()
        {
            while (!IsGameOver())
            {
                ProcessTurn();
                _currentTurn++;
                _gameState.Turns = _currentTurn;
                _gui.UpdateGameState(_gameState);
            }

            AnnounceWinner();
        }

        private void ProcessTurn()
        {
            // Esperar a que el jugador seleccione una acción
            var clickedElement = _gui.WaitForClick();

            if (clickedElement.Type == ClickedElementType.Button)
            {
                ProcessButtonAction(clickedElement.Text);
            }
        }

        private void ProcessButtonAction(string action)
        {
            switch (action)
            {
                case "Atacar":
                    HandleAttack();
                    break;
                case "Disparar":
                    HandleShoot();
                    break;
                case "Usar Habilidad":
                    HandleSkillUse();
                    break;
                case "Invocar":
                    HandleSummon();
                    break;
                case "Pasar Turno":
                    HandlePassTurn();
                    break;
                case "Rendirse":
                    HandleSurrender();
                    break;
            }
        }

        private void HandleAttack()
        {
            // Esperar selección de objetivo
            var target = _gui.WaitForUnitInBoard();
            
            // Ejecutar ataque usando tu lógica existente
            // ... implementar según tu BattleEngine
            
            _gui.UpdateGameState(_gameState);
        }

        private void HandleShoot()
        {
            // Implementar lógica de disparo
        }

        private void HandleSkillUse()
        {
            // Mostrar opciones de habilidades disponibles
            var currentUnit = GetCurrentUnit();
            var skills = currentUnit.GetAvailableSkills();
            
            var skillOptions = skills
                .Select(s => $"{s.Name} - MP: {s.MpCost}")
                .ToList();
            skillOptions.Add("Cancelar");

            _gameState.Options = skillOptions;
            _gui.UpdateGameState(_gameState);

            // Esperar selección de habilidad
            var selectedSkill = _gui.WaitForButton("");
            
            if (selectedSkill.Text != "Cancelar")
            {
                // Ejecutar habilidad
                // ... implementar
            }
        }

        private void HandleSummon()
        {
            // Esperar selección de demonio en reserva
            var demonToSummon = _gui.WaitForUnitInReserve();
            
            // Esperar selección de posición en el tablero
            var boardPosition = _gui.WaitForUnitInBoard();
            
            // Ejecutar invocación
            // ... implementar
            
            _gui.UpdateGameState(_gameState);
        }

        private void HandlePassTurn()
        {
            // Pasar turno
        }

        private void HandleSurrender()
        {
            // Implementar rendición
        }

        private bool IsGameOver()
        {
            // Verificar si algún equipo ha perdido todas sus unidades
            return _team1.IsDefeated() || _team2.IsDefeated();
        }

        private void AnnounceWinner()
        {
            string winner = _team1.IsDefeated() ? "Jugador 2" : "Jugador 1";
            _gui.ShowEndGameMessage($"¡{winner} ha ganado!");
        }

        private Unit GetCurrentUnit()
        {
            // Implementar lógica para obtener la unidad actual
            throw new NotImplementedException();
        }
    }

    // Clase auxiliar para envolver la información del equipo
    internal class TeamInfoWrapper : ITeamInfo
    {
        public string SamuraiName { get; }
        public string[] SkillNames { get; }
        public string[] DemonNames { get; }

        public TeamInfoWrapper(string samuraiName, string[] skillNames, string[] demonNames)
        {
            SamuraiName = samuraiName;
            SkillNames = skillNames;
            DemonNames = demonNames;
        }
    }
}
```

---

## Paso 7: Verificación y Testing

### 7.1 Verificar que los Tests Siguen Funcionando

```bash
# En la terminal de Rider
dotnet test
```

Todos los tests deben pasar sin errores. Si alguno falla, verifica que:

- No hayas modificado las clases originales del dominio
- La clase `Game` conserve su constructor original
- El modo consola siga funcionando correctamente

### 7.2 Probar el Modo GUI

1. En `Program.cs`, establece `bool useGui = true;`
2. Ejecuta el proyecto
3. Verifica que:
   - Se abre la ventana de selección de equipos
   - Puedes seleccionar equipos para ambos jugadores
   - Se muestra correctamente el tablero de juego
   - Puedes interactuar con los botones y unidades

### 7.3 Probar el Modo Consola

1. En `Program.cs`, establece `bool useGui = false;`
2. Ejecuta el proyecto
3. Verifica que el modo consola funciona como antes

---

## Troubleshooting

### Error: "Could not load file or assembly 'Shin-Megami-Tensei-GUI'"

**Solución:**

1. Verifica que la DLL esté correctamente referenciada
2. Limpia y reconstruye la solución: `Build → Rebuild Solution`
3. Verifica que todas las DLLs de la carpeta `GuiLib` estén presentes

### Error: "Avalonia.NativeControlHost no encontrado"

**Solución:**
Reinstala los paquetes NuGet de Avalonia:

```bash
dotnet remove package Avalonia
dotnet add package Avalonia --version 11.0.0
```

### La ventana no se abre en macOS

**Problema:** macOS requiere que la GUI se ejecute en el thread principal.

**Solución:** Asegúrate de usar `gui.Start(callback)` correctamente:

```csharp
var gui = new SMTGUI();
gui.Start(MainGameLoop);

void MainGameLoop()
{
    // Tu lógica de juego aquí
}
```

### Los tests fallan después de integrar la GUI

**Solución:**

1. Verifica que no hayas modificado la firma del constructor original de `Game`
2. Asegúrate de que `Game` conserve su constructor que acepta `View` y `string`
3. No elimines código existente, solo añade nuevas funcionalidades

### La ventana se congela durante la ejecución

**Problema:** Estás bloqueando el thread de la UI.

**Solución:** Asegúrate de llamar a `gui.Update(state)` regularmente para refrescar la interfaz.

### Error: "System.NullReferenceException" al actualizar el estado

**Solución:**

1. Asegúrate de llamar a `RefreshUnits()` antes de `gui.Update()`
2. Verifica que todos los adaptadores estén correctamente inicializados:

```csharp
_gameState.RefreshUnits();
_gui.UpdateGameState(_gameState);
```

---

## Checklist Final

Antes de considerar la integración completa, verifica:

- [ ] Los paquetes NuGet de Avalonia están instalados
- [ ] La DLL de GUI está referenciada en ambos proyectos
- [ ] Se crearon las clases `UnitAdapter`, `PlayerAdapter` y `GameStateAdapter`
- [ ] Se creó la clase `GuiPresenter`
- [ ] Se modificó el `Program.cs` con soporte para ambos modos
- [ ] Los tests siguen pasando en modo consola
- [ ] La ventana de selección de equipos funciona correctamente
- [ ] Se puede jugar una partida completa en modo GUI
- [ ] Se validan correctamente los equipos inválidos
- [ ] Se muestra el mensaje de ganador al finalizar
- [ ] El código está documentado con comentarios XML

---

## Recursos Adicionales

- **Documentación de Avalonia:** <https://docs.avaloniaui.net/>
- **Enunciado oficial:** Revisar el PDF proporcionado por los profesores
- **Repositorio de ejemplo:** <https://github.com/vruizz22/Proyecto-Megaten-IIC2113-GUI>

---

## Notas Finales

Esta integración mantiene **total compatibilidad** con tu código existente mediante:

1. **Patrón Adapter:** Convierte tus clases de dominio a las interfaces de la GUI
2. **Inyección de Dependencias:** Permite alternar entre consola y GUI fácilmente
3. **Separación de Responsabilidades:** La lógica del juego permanece intacta

**¡Éxito con tu integración!** 🎮
