using Coach_app.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories.Interfaces
{
    public interface IPhotoRepository
    {
        Task Init();
        Task<int> SavePhotoAsync(AppPhoto photo);
        Task TagStudentOnPhotoAsync(int photoId, int studentId);
        Task<List<AppPhoto>> GetPhotosByStudentAsync(int studentId);
        Task DeletePhotoAsync(AppPhoto photo);
    }
}