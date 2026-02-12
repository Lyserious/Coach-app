using Coach_app.Models;
using Coach_app.Models.Domains.Groups; 
using Coach_app.Models.Domains.Training;

namespace Coach_app.Data.Repositories.Interfaces
{
    public interface ISessionRepository
    {
        Task<GroupSession> GetSessionByIdAsync(int id);
        Task<List<GroupSession>> GetSessionsByGroupIdAsync(int groupId);
        Task<List<GroupSession>> GetSessionsByDateAsync(DateTime date);
        Task AddSessionAsync(GroupSession session);
        Task UpdateSessionAsync(GroupSession session);
        Task DeleteSessionAsync(int sessionId);

        // Gestion du contenu de séance
        Task<List<SessionExercise>> GetExercisesForSessionAsync(int sessionId);
        Task SaveSessionExerciseAsync(SessionExercise sessionExercise);
        Task DeleteSessionExerciseAsync(SessionExercise sessionExercise);

        // Gestion de l'appel
        Task<List<SessionAttendance>> GetAttendanceForSessionAsync(int sessionId);
        Task SaveAttendanceListAsync(List<SessionAttendance> attendanceList);

        // Logique de génération 
        Task GenerateSessionsForGroupAsync(Group group);
    }
}