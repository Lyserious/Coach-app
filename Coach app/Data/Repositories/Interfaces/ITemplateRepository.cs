using Coach_app.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories.Interfaces
{
    public interface ITemplateRepository
    {
        Task<List<SessionTemplate>> GetAllTemplatesAsync();
        Task<List<SessionTemplateExercise>> GetTemplateExercisesAsync(int templateId);
        Task SaveTemplateAsync(SessionTemplate template, List<SessionTemplateExercise> exercises);
        Task DeleteTemplateAsync(SessionTemplate template);
        Task<SessionTemplate> GetTemplateByIdAsync(int id);
    }
}