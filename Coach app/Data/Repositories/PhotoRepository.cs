using Coach_app.Configurations;
using Coach_app.Models;
using Coach_app.Services.Auth;
using SQLite;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    public class PhotoRepository : IPhotoRepository
    {
        private SQLiteAsyncConnection _database;
        private readonly ISessionService _sessionService;
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private bool _isInitialized = false;

        public PhotoRepository(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public async Task Init()
        {
            if (_isInitialized && _database != null) return;
            await _initLock.WaitAsync();
            try
            {
                if (_isInitialized && _database != null) return;
                var currentCoach = _sessionService.CurrentCoach;
                if (currentCoach == null) return;

                string dbPath = Constants.GetCoachDbPath(currentCoach.DataFileName);
                _database = new SQLiteAsyncConnection(dbPath, Constants.Flags);

                // On crée les tables Photos et Tags
                await _database.CreateTableAsync<AppPhoto>();
                await _database.CreateTableAsync<PhotoTag>();

                _isInitialized = true;
            }
            finally { _initLock.Release(); }
        }

        public async Task<int> SavePhotoAsync(AppPhoto photo)
        {
            await Init();
            if (photo.Id != 0) await _database.UpdateAsync(photo);
            else await _database.InsertAsync(photo);
            return photo.Id;
        }

        public async Task TagStudentOnPhotoAsync(int photoId, int studentId)
        {
            await Init();
            // On vérifie si le tag existe déjà pour éviter les doublons
            var existing = await _database.Table<PhotoTag>()
                                          .Where(t => t.PhotoId == photoId && t.StudentId == studentId)
                                          .FirstOrDefaultAsync();
            if (existing == null)
            {
                await _database.InsertAsync(new PhotoTag { PhotoId = photoId, StudentId = studentId });
            }
        }

        public async Task<List<AppPhoto>> GetPhotosByStudentAsync(int studentId)
        {
            await Init();
            // Requête SQL pour récupérer les photos liées à l'élève via la table de Tag
            return await _database.QueryAsync<AppPhoto>(
                "SELECT P.* FROM AppPhoto P INNER JOIN PhotoTag T ON P.Id = T.PhotoId WHERE T.StudentId = ? ORDER BY P.DateTaken DESC",
                studentId);
        }

        public async Task DeletePhotoAsync(AppPhoto photo)
        {
            await Init();
            // Supprimer les tags associés
            var tags = await _database.Table<PhotoTag>().Where(t => t.PhotoId == photo.Id).ToListAsync();
            foreach (var t in tags) await _database.DeleteAsync(t);

            // Supprimer la photo de la base (Le fichier reste sur le disque par sécurité, ou on peut le supprimer ici)
            await _database.DeleteAsync(photo);
        }
    }
}