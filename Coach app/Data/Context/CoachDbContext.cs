using Coach_app.Core.Constants; // Note le nouveau namespace
using Coach_app.Models;
using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Domains.Identity; // Adapte selon tes nouveaux namespaces
using Coach_app.Models.Domains.Students;
using Coach_app.Models.Domains.Training;
using SQLite;
// Ajoute ici les autres usings nécessaires pour tes modèles (Performance, etc.)

namespace Coach_app.Data.Context
{
    public class CoachDbContext
    {
        private SQLiteAsyncConnection _database;
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private bool _isInitialized = false;

        public CoachDbContext()
        {
            // Constructeur vide pour l'injection
        }

        public SQLiteAsyncConnection Connection => _database;

        public async Task InitAsync()
        {
            if (_isInitialized && _database != null) return;

            await _initLock.WaitAsync();
            try
            {
                if (_isInitialized && _database != null) return;

                // On utilise le chemin défini dans tes constantes
                _database = new SQLiteAsyncConnection(Constants.GlobalDbPath, Constants.Flags);

                // --- CRÉATION DES TABLES (Centralisée ici) ---
                // Tu dois t'assurer que tous ces modèles sont bien accessibles via les 'using' en haut
                await _database.CreateTableAsync<Coach>();

                await _database.CreateTableAsync<Student>();
                await _database.CreateTableAsync<StudentContact>();
                await _database.CreateTableAsync<StudentGroup>();
                // await _database.CreateTableAsync<StudentNote>(); // Si tu l'utilises

                await _database.CreateTableAsync<Group>();
                await _database.CreateTableAsync<GroupSession>();
                await _database.CreateTableAsync<SessionAttendance>();
                await _database.CreateTableAsync<GroupPhoto>();

                await _database.CreateTableAsync<Exercise>();
                await _database.CreateTableAsync<SessionExercise>();
                await _database.CreateTableAsync<Performance>();

                await _database.CreateTableAsync<SessionTemplate>();
                await _database.CreateTableAsync<SessionTemplateExercise>();

                _isInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }
    }
}