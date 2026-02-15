using Coach_app.Models.Domains.Groups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories.Interfaces
{
    public interface IPerformanceRepository
    {
        Task<List<Performance>> GetPerformancesBySessionExerciseAsync(int sessionExerciseId);
        Task SavePerformanceAsync(Performance perf);
        Task DeletePerformanceAsync(Performance perf);
    }
}