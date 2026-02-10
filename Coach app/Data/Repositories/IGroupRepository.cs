using Coach_app.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    public interface IGroupRepository
    {
        // 1. Groupes
        Task<List<Group>> GetGroupsAsync();
        Task<Group> GetGroupByIdAsync(int id);
        Task SaveGroupAsync(Group group);
        Task DeleteGroupAsync(Group group);

        // 2. Élèves
        Task<List<Student>> GetStudentsByGroupIdAsync(int groupId);

        // 3. Séances (Calendrier)
        Task<List<GroupSession>> GetSessionsByGroupIdAsync(int groupId);
        Task<List<GroupSession>> GetSessionsByDateAsync(DateTime date);
        Task GenerateSessionsForGroupAsync(Group group);
        Task<GroupSession> GetSessionByIdAsync(int id);
        Task AddSessionAsync(GroupSession session);
        Task DeleteSessionAsync(int sessionId);

        // 4. Présences
        Task<List<SessionAttendance>> GetAttendanceForSessionAsync(int sessionId);
        Task SaveAttendanceListAsync(List<SessionAttendance> attendanceList);

        // 5. Photos
        Task<List<GroupPhoto>> GetPhotosByGroupIdAsync(int groupId);
        Task AddPhotoAsync(GroupPhoto photo);
        Task<GroupPhoto> GetPhotoByIdAsync(int id);
        Task UpdatePhotoAsync(GroupPhoto photo);
        Task DeletePhotoAsync(GroupPhoto photo);

        // 7. Programme (Contenu Séance)
        Task<List<SessionExercise>> GetExercisesForSessionAsync(int sessionId);
        Task SaveSessionExerciseAsync(SessionExercise sessionExercise);
        Task DeleteSessionExerciseAsync(SessionExercise sessionExercise);

        // Templates
        Task<List<SessionTemplate>> GetAllTemplatesAsync();
        Task SaveTemplateAsync(SessionTemplate template, List<SessionTemplateExercise> exercises);
        Task ImportTemplateToSessionAsync(int templateId, int targetSessionId);

        // 8. Performances
        Task<List<Performance>> GetPerformancesBySessionExerciseAsync(int sessionExerciseId);
        Task SavePerformanceAsync(Performance perf);
        Task DeletePerformanceAsync(Performance perf);
    }
}