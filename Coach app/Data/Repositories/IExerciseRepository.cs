using Coach_app.Models.Domains.Training;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    public interface IExerciseRepository
    {
        Task<List<Exercise>> GetAllExercisesAsync();
        Task<Exercise> GetExerciseByIdAsync(int id);
        Task<int> SaveExerciseAsync(Exercise exercise);
        Task<int> DeleteExerciseAsync(int id); // Soft delete (Archive)
    }
}