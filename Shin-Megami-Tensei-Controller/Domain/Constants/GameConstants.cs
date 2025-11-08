namespace Shin_Megami_Tensei.Domain.Constants;

public static class GameConstants
{
    public static class Team
    {
        public const int MaxMonsters = 7;
        public const int MaxUnits = 8;
        public const int BoardSize = 4;
        public const int SamuraiPosition = 0;
        public const int MaxSamuraiSkills = 8;
    }

    public static class Action
    {
        public const int SamuraiMaxOptions = 6;
        public const int MonsterMaxOptions = 4;
    }

    public static class Combat
    {
        public const double BaseAttackModifier = 54.0;
        public const double BaseGunModifier = 80.0;
        public const double DamageConstant = 0.0114;
        public const double WeakMultiplier = 1.5;
        public const double ResistMultiplier = 0.5;
        public const int InstantKillLuckMultiplier = 2;
    }

    public static class Messages
    {
        public const string InvalidTeam = "Archivo de equipos inválido";
        public const string ChooseTeamFile = "Elige un archivo para cargar los equipos";
        public const string SelectAction = "Seleccione una acción para {0}";
        public const string SelectTarget = "Seleccione un objetivo para {0}";
        public const string SelectSkill = "Seleccione una habilidad para que {0} use";
        public const string SelectMonster = "Seleccione un monstruo para invocar";
        public const string SelectPosition = "Seleccione una posición para invocar";
        public const string Cancel = "Cancelar";
        public const string Winner = "Ganador: {0}";
        public const string Separator = "----------------------------------------";
    }
}

