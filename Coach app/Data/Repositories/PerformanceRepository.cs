using SQLite;
using Coach_app.Data.Context;
using Coach_app.Models.Domains.Groups;

namespace Coach_app.Data.Repositories
{
    public class PerformanceRepository : Interfaces.IPerformanceRepository
    {
        private readonly CoachDbContext _context;
        private SQLiteAsyncConnection Connection => _context.Connection;

        public PerformanceRepository(CoachDbContext context) { _context = context; }

        public async Task<List<Performance>> GetPerformancesBySessionExerciseAsync(int sessionExerciseId)
        {
            await _context.InitAsync();
            return await Connection.Table<Performance>()
                                  .Where(p => p.SessionExerciseId == sessionExerciseId)
                                  .ToListAsync();
        }

        public async Task SavePerformanceAsync(Performance perf)
        {
            await _context.InitAsync();
            if (perf.Id != 0) await Connection.UpdateAsync(perf);
            else await Connection.InsertAsync(perf);
        }

        public async Task DeletePerformanceAsync(Performance perf)
        {
            await _context.InitAsync();
            await Connection.DeleteAsync(perf);
        }
    }
}