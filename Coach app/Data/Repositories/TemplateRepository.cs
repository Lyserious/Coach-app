using Coach_app.Models;
using Coach_app.Models.Domains.Groups;
using Coach_app.Services.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    // On force explicitement l'interface du dossier Interfaces
    public class TemplateRepository : Coach_app.Data.Repositories.Interfaces.ITemplateRepository
    {
        private readonly ICoachDatabaseService _dbService;

        public TemplateRepository(ICoachDatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<List<SessionTemplate>> GetAllTemplatesAsync()
        {
            var db = await _dbService.GetConnectionAsync();
            return await db.Table<SessionTemplate>().ToListAsync();
        }

        public async Task<List<SessionTemplateExercise>> GetTemplateExercisesAsync(int templateId)
        {
            var db = await _dbService.GetConnectionAsync();
            return await db.Table<SessionTemplateExercise>().Where(x => x.TemplateId == templateId).ToListAsync();
        }

        public async Task SaveTemplateAsync(SessionTemplate template, List<SessionTemplateExercise> exercises)
        {
            var db = await _dbService.GetConnectionAsync();
            if (template.Id != 0) await db.UpdateAsync(template);
            else await db.InsertAsync(template);

            var oldExos = await db.Table<SessionTemplateExercise>().Where(x => x.TemplateId == template.Id).ToListAsync();
            foreach (var old in oldExos) await db.DeleteAsync(old);

            foreach (var ex in exercises)
            {
                ex.TemplateId = template.Id;
            }
            if (exercises.Any()) await db.InsertAllAsync(exercises);
        }

        public async Task DeleteTemplateAsync(SessionTemplate template)
        {
            var db = await _dbService.GetConnectionAsync();
            var exos = await GetTemplateExercisesAsync(template.Id);
            foreach (var ex in exos) await db.DeleteAsync(ex);
            await db.DeleteAsync(template);
        }

        public async Task<SessionTemplate> GetTemplateByIdAsync(int id)
        {
            var db = await _dbService.GetConnectionAsync();
            return await db.Table<SessionTemplate>().Where(t => t.Id == id).FirstOrDefaultAsync();
        }
    }
}