using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Domains.Students;

namespace Coach_app.Data.Repositories.Interfaces
{
    public interface IGroupRepository
    {
        // --- Groupes ---
        Task<List<Group>> GetGroupsAsync();
        Task<Group> GetGroupByIdAsync(int id);
        Task SaveGroupAsync(Group group);
        Task DeleteGroupAsync(Group group);

        // --- Élèves (Lecture seule liée au groupe) ---
        Task<List<Student>> GetStudentsByGroupIdAsync(int groupId);

        // --- Photos ---
        Task<List<GroupPhoto>> GetPhotosByGroupIdAsync(int groupId);
        Task AddPhotoAsync(GroupPhoto photo);
        Task<GroupPhoto> GetPhotoByIdAsync(int id);
        Task UpdatePhotoAsync(GroupPhoto photo);
        Task DeletePhotoAsync(GroupPhoto photo);
    }
}