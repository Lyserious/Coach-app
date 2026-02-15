using Coach_app.Core.Constants;
using Coach_app.Models;
using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Domains.Identity;
using Coach_app.Models.Domains.Students;
using Coach_app.Models.Domains.Training;
using Coach_app.Services.Auth;
using SQLite;
using System.Threading;
using System.Threading.Tasks;

namespace Coach_app.Services.Data
{
    public interface ICoachDatabaseService
    {
        Task<SQLiteAsyncConnection> GetConnectionAsync();
    }

    public class CoachDatabaseService : ICoachDatabaseService
    {
        private SQLiteAsyncConnection _database;
        private readonly ISessionService _sessionService;
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private bool _isInitialized = false;

        public CoachDatabaseService(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_isInitialized && _database != null) return _database;

            await _initLock.WaitAsync();
            try
            {
                if (_isInitialized && _database != null) return _database;

                var currentCoach = _sessionService.CurrentCoach;
                if (currentCoach == null) return null; // Ou gérer l'erreur

                string dbPath = Constants.GetCoachDbPath(currentCoach.DataFileName);
                _database = new SQLiteAsyncConnection(dbPath, Constants.Flags);

                // Création des tables (Code déplacé depuis GroupRepository)
                // Création des tables (Liste complète unifiée)
                await _database.CreateTableAsync<Group>();
                await _database.CreateTableAsync<GroupSession>();

                // --- AJOUTS SPECIFIQUES MODULE STUDENT ---
                await _database.CreateTableAsync<Student>();
                await _database.CreateTableAsync<StudentGroup>();
                await _database.CreateTableAsync<StudentContact>();
                await _database.CreateTableAsync<StudentNote>();
                // -----------------------------------------

                await _database.CreateTableAsync<SessionAttendance>();
                await _database.CreateTableAsync<GroupPhoto>();
                // --- AJOUTS SPECIFIQUES PHOTO REPO ---
                await _database.CreateTableAsync<AppPhoto>();
                await _database.CreateTableAsync<PhotoTag>();
                // -------------------------------------

                await _database.CreateTableAsync<Exercise>();
                await _database.CreateTableAsync<SessionExercise>();
                await _database.CreateTableAsync<SessionTemplate>();
                await _database.CreateTableAsync<SessionTemplateExercise>();
                await _database.CreateTableAsync<Performance>();
                _isInitialized = true;
                return _database;
            }
            finally
            {
                _initLock.Release();
            }
        }
    }
}