using Coach_app.Data.Repositories.Interfaces;
using Coach_app.Models;
using Coach_app.Services.Data;
  // Ajout namespace Interface si besoin
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly ICoachDatabaseService _dbService;

        public PhotoRepository(ICoachDatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<int> SavePhotoAsync(AppPhoto photo)
        {
            var db = await _dbService.GetConnectionAsync();
            if (photo.Id != 0) await db.UpdateAsync(photo);
            else await db.InsertAsync(photo);
            return photo.Id;
        }

        public async Task TagStudentOnPhotoAsync(int photoId, int studentId)
        {
            var db = await _dbService.GetConnectionAsync();
            var existing = await db.Table<PhotoTag>()
                                          .Where(t => t.PhotoId == photoId && t.StudentId == studentId)
                                          .FirstOrDefaultAsync();
            if (existing == null)
            {
                await db.InsertAsync(new PhotoTag { PhotoId = photoId, StudentId = studentId });
            }
        }

        public async Task<List<AppPhoto>> GetPhotosByStudentAsync(int studentId)
        {
            var db = await _dbService.GetConnectionAsync();
            return await db.QueryAsync<AppPhoto>(
                "SELECT P.* FROM AppPhoto P INNER JOIN PhotoTag T ON P.Id = T.PhotoId WHERE T.StudentId = ? ORDER BY P.DateTaken DESC",
                studentId);
        }

        public async Task DeletePhotoAsync(AppPhoto photo)
        {
            var db = await _dbService.GetConnectionAsync();
            var tags = await db.Table<PhotoTag>().Where(t => t.PhotoId == photo.Id).ToListAsync();
            foreach (var t in tags) await db.DeleteAsync(t);

            await db.DeleteAsync(photo);
        }
    }
}