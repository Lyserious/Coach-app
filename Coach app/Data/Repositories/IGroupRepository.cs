using Coach_app.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    public interface IGroupRepository
    {
        Task<List<Group>> GetActiveGroupsAsync(); // Récupère tout sauf les archivés
        Task<Group> GetGroupByIdAsync(int id);
        Task<int> SaveGroupAsync(Group group);
        Task<int> DeleteGroupAsync(int id); // Soft delete
    }
}