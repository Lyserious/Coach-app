using Coach_app.Models.Domains.Groups;
using Coach_app.Services.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    // On force explicitement l'interface du dossier Interfaces
    public class PerformanceRepository : Coach_app.Data.Repositories.Interfaces.IPerformanceRepository
    {
        private readonly ICoachDatabaseService _dbService;

        public PerformanceRepository(ICoachDatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<List<Performance>> GetPerformancesBySessionExerciseAsync(int sessionExerciseId)
        {
            var db = await _dbService.GetConnectionAsync();
            return await db.Table<Performance>()
                           .Where(p => p.SessionExerciseId == sessionExerciseId)
                           .ToListAsync();
        }

        public async Task SavePerformanceAsync(Performance perf)
        {
            var db = await _dbService.GetConnectionAsync();
            if (perf.Id != 0)
                await db.UpdateAsync(perf);
            else
                await db.InsertAsync(perf);
        }

        public async Task DeletePerformanceAsync(Performance perf)
        {
            var db = await _dbService.GetConnectionAsync();
            await db.DeleteAsync(perf);
        }
    }
}