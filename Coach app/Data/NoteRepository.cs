using Coach_app.Configurations;
using Coach_app.Models;
using Coach_app.Services.Auth;
using SQLite;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private SQLiteAsyncConnection _database;
        private readonly ISessionService _sessionService;

        // Son propre verrou de sécurité
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private bool _isInitialized = false;

        public NoteRepository(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        private async Task Init()
        {
            if (_isInitialized && _database != null) return;

            await _initLock.WaitAsync();
            try
            {
                if (_isInitialized && _database != null) return;

                var currentCoach = _sessionService.CurrentCoach;
                if (currentCoach == null) return;

                // On utilise le même chemin de base de données que le reste de l'app
                string dbPath = Constants.GetCoachDbPath(currentCoach.DataFileName);
                _database = new SQLiteAsyncConnection(dbPath, Constants.Flags);

                // On crée uniquement la table des notes ici
                await _database.CreateTableAsync<AppNote>();

                _isInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task<List<AppNote>> GetNotesAsync(NoteTargetType type, int targetId)
        {
            await Init();
            // On filtre par TYPE et par ID
            return await _database.Table<AppNote>()
                                  .Where(n => n.TargetType == type && n.TargetId == targetId)
                                  .OrderByDescending(n => n.Date) // Les plus récentes en haut
                                  .ToListAsync();
        }

        public async Task SaveNoteAsync(AppNote note)
        {
            await Init();
            if (note.Id != 0)
                await _database.UpdateAsync(note);
            else
                await _database.InsertAsync(note);
        }

        public async Task DeleteNoteAsync(AppNote note)
        {
            await Init();
            await _database.DeleteAsync(note);
        }
    }
}