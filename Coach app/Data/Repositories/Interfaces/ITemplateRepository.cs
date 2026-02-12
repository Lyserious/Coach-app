using Coach_app.Models.Domains.Training;
using Coach_app.Models; 

namespace Coach_app.Data.Repositories.Interfaces
{
    public interface ITemplateRepository
    {
        Task<List<SessionTemplate>> GetAllTemplatesAsync();
        Task<List<SessionTemplateExercise>> GetTemplateExercisesAsync(int templateId);
        Task SaveTemplateAsync(SessionTemplate template, List<SessionTemplateExercise> exercises);
        Task DeleteTemplateAsync(SessionTemplate template);
        Task ImportTemplateToSessionAsync(int templateId, int targetSessionId);
    }
}