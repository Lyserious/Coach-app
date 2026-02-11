using System.IO;

namespace Coach_app.Core.Constants
{
    public static class Constants
    {
        // Nom de la base de données "Maître" qui liste les coachs
        public const string GlobalDbName = "coach_app_global.db3";

        // Flags pour l'ouverture de la base SQLite
        public const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;

        // Chemin vers la base de données globale
        public static string GlobalDbPath =>
            Path.Combine(FileSystem.AppDataDirectory, GlobalDbName);

        // Méthode helper pour générer le chemin de la DB d'un coach spécifique
        public static string GetCoachDbPath(string coachUniqueId)
        {
            return Path.Combine(FileSystem.AppDataDirectory, $"coach_{coachUniqueId}.db3");
        }
    }
}