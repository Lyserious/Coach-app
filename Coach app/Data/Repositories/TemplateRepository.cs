using SQLite;
using Coach_app.Data.Context;
using Coach_app.Models.Domains.Training;
using Coach_app.Models; // Check tes namespaces pour SessionTemplate

namespace Coach_app.Data.Repositories
{
    public class TemplateRepository : Interfaces.ITemplateRepository
    {
        private readonly CoachDbContext _context;
        private readonly Interfaces.ISessionRepository _sessionRepository; // Injection croisée pour l'import

        private SQLiteAsyncConnection Connection => _context.Connection;

        public TemplateRepository(CoachDbContext context, Interfaces.ISessionRepository sessionRepository)
        {
            _context = context;
            _sessionRepository = sessionRepository;
        }

        public async Task<List<SessionTemplate>> GetAllTemplatesAsync()
        {
            await _context.InitAsync();
            return await Connection.Table<SessionTemplate>().ToListAsync();
        }

        public async Task<List<SessionTemplateExercise>> GetTemplateExercisesAsync(int templateId)
        {
            await _context.InitAsync();
            return await Connection.Table<SessionTemplateExercise>().Where(x => x.TemplateId == templateId).ToListAsync();
        }

        public async Task SaveTemplateAsync(SessionTemplate template, List<SessionTemplateExercise> exercises)
        {
            await _context.InitAsync();
            if (template.Id != 0) await Connection.UpdateAsync(template);
            else await Connection.InsertAsync(template);

            // Remplacement total des exercices du template (bourrin mais efficace)
            var oldExos = await Connection.Table<SessionTemplateExercise>().Where(x => x.TemplateId == template.Id).ToListAsync();
            foreach (var old in oldExos) await Connection.DeleteAsync(old);

            foreach (var ex in exercises) ex.TemplateId = template.Id;
            if (exercises.Any()) await Connection.InsertAllAsync(exercises);
        }

        public async Task DeleteTemplateAsync(SessionTemplate template)
        {
            await _context.InitAsync();
            var exos = await GetTemplateExercisesAsync(template.Id);
            foreach (var ex in exos) await Connection.DeleteAsync(ex);
            await Connection.DeleteAsync(template);
        }

        public async Task ImportTemplateToSessionAsync(int templateId, int targetSessionId)
        {
            await _context.InitAsync();

            var template = await Connection.Table<SessionTemplate>().FirstOrDefaultAsync(t => t.Id == templateId);
            var templateExercises = await GetTemplateExercisesAsync(templateId);
            var targetSession = await _sessionRepository.GetSessionByIdAsync(targetSessionId);

            if (targetSession != null && template != null)
            {
                targetSession.Description = template.Description;
                await _sessionRepository.UpdateSessionAsync(targetSession);
            }

            var existing = await _sessionRepository.GetExercisesForSessionAsync(targetSessionId);
            int startIndex = existing.Count + 1;
            var newSessionExercises = new List<SessionExercise>();

            foreach (var tExo in templateExercises)
            {
                newSessionExercises.Add(new SessionExercise
                {
                    GroupSessionId = targetSessionId,
                    ExerciseId = tExo.ExerciseId,
                    OrderIndex = startIndex++,
                    Sets = tExo.Sets,
                    Reps = tExo.Reps,
                    Weight = tExo.Weight,
                    Rest = tExo.Rest,
                    Note = tExo.Note
                });
            }
            if (newSessionExercises.Any()) await Connection.InsertAllAsync(newSessionExercises);
        }
    }
}