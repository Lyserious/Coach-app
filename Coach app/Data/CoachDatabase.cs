using SQLite;
using Coach_app.Configurations;
using Coach_app.Models;

namespace Coach_app.Data
{
    public class CoachDatabase
    {
        // C'est cette variable qui manquait et causait l'erreur CS0103
        private SQLiteAsyncConnection _database;

        public CoachDatabase()
        {
            // Constructeur vide nécessaire si utilisé comme service singleton
        }

        public async Task Init()
        {
            if (_database is not null)
                return;

            // Assure-toi que Constants.DatabasePath est bien accessible (voir point 2 ci-dessous)
            _database = new SQLiteAsyncConnection(Constants.GlobalDbPath, Constants.Flags);
            // Création des tables
            await _database.CreateTableAsync<Student>();
            await _database.CreateTableAsync<Group>();
            await _database.CreateTableAsync<StudentContact>();
            await _database.CreateTableAsync<SessionAttendance>();
            await _database.CreateTableAsync<GroupSession>();
        }
    }
}