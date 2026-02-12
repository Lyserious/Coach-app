using Coach_app.Models.Domains.Groups;

namespace Coach_app.Data.Repositories.Interfaces
{
    public interface IPerformanceRepository
    {
        Task<List<Performance>> GetPerformancesBySessionExerciseAsync(int sessionExerciseId);
        Task SavePerformanceAsync(Performance perf);
        Task DeletePerformanceAsync(Performance perf);
    }
}