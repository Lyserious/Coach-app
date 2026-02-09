using Coach_app.Models;

namespace Coach_app.Data.Repositories
{
    public interface IGroupRepository
    {
        // --- TES MÉTHODES EXISTANTES (Groupes, Séances...) ---
        Task<List<Group>> GetGroupsAsync();
        Task<Group> GetGroupByIdAsync(int id);
        Task SaveGroupAsync(Group group);
        Task DeleteGroupAsync(Group group);

        Task GenerateSessionsForGroupAsync(Group group);
        Task<List<GroupSession>> GetSessionsByGroupIdAsync(int groupId);
        Task<List<GroupSession>> GetSessionsByDateAsync(DateTime date);
        Task<GroupSession> GetSessionByIdAsync(int sessionId);

        Task<List<SessionAttendance>> GetAttendanceForSessionAsync(int sessionId);
        Task SaveAttendanceListAsync(List<SessionAttendance> attendanceList);

        // --- GESTION ÉLÈVES ---
        Task<List<Student>> GetStudentsByGroupIdAsync(int groupId);

        // --- CALENDRIER (Ajouté récemment) ---
        Task AddSessionAsync(GroupSession session);
        Task DeleteSessionAsync(int sessionId);

        // --- GESTION PHOTOS (C'est ici qu'il y avait des doublons !) ---
        // Vérifie que tu as CHAQUE ligne UNE SEULE FOIS :

        Task<List<GroupPhoto>> GetPhotosByGroupIdAsync(int groupId);
        Task<GroupPhoto> GetPhotoByIdAsync(int id);
        Task AddPhotoAsync(GroupPhoto photo);
        Task UpdatePhotoAsync(GroupPhoto photo);
        Task DeletePhotoAsync(GroupPhoto photo);
    }
}