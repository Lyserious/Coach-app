using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Domains.Training;
using Coach_app.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories.Interfaces
{
    public interface ISessionRepository
    {
        // Session CRUD
        Task<List<GroupSession>> GetSessionsByGroupIdAsync(int groupId);
        Task<List<GroupSession>> GetSessionsByDateAsync(DateTime date);
        Task<GroupSession> GetSessionByIdAsync(int id);
        Task AddSessionAsync(GroupSession session);
        Task UpdateSessionAsync(GroupSession session);
        Task DeleteSessionAsync(int sessionId);
        Task GenerateSessionsForGroupAsync(Group group);

        // Contenu de séance
        Task<List<SessionExercise>> GetExercisesForSessionAsync(int sessionId);
        Task SaveSessionExerciseAsync(SessionExercise sessionExercise);
        Task DeleteSessionExerciseAsync(SessionExercise sessionExercise);

        // Méthode qui posait problème (CS1061)
        Task ImportTemplateToSessionAsync(int templateId, int targetSessionId, ITemplateRepository templateRepo);
    }
}