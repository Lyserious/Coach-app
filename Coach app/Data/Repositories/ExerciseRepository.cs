using Coach_app.Configurations;
using Coach_app.Models;
using Coach_app.Services.Auth;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    public class ExerciseRepository : IExerciseRepository
    {
        private SQLiteAsyncConnection _database;
        private readonly ISessionService _sessionService;

        public ExerciseRepository(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        private async Task Init()
        {
            if (_database != null) return;

            var currentCoach = _sessionService.CurrentCoach;
            if (currentCoach == null) return;

            string dbPath = Constants.GetCoachDbPath(currentCoach.DataFileName);
            _database = new SQLiteAsyncConnection(dbPath, Constants.Flags);

            // Création de la table
            await _database.CreateTableAsync<Exercise>();
        }

        public async Task<List<Exercise>> GetAllExercisesAsync()
        {
            await Init();
            if (_database == null) return new List<Exercise>();

            // On retourne tout sauf les archivés, triés par catégorie puis par nom
            return await _database.Table<Exercise>()
                                  .Where(e => !e.IsArchived)
                                  .OrderBy(e => e.Category)
                                  .ThenBy(e => e.Name)
                                  .ToListAsync();
        }

        public async Task<Exercise> GetExerciseByIdAsync(int id)
        {
            await Init();
            if (_database == null) return null;
            return await _database.Table<Exercise>().Where(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveExerciseAsync(Exercise exercise)
        {
            await Init();
            if (_database == null) return 0;

            if (exercise.Id != 0)
                return await _database.UpdateAsync(exercise);
            else
                return await _database.InsertAsync(exercise);
        }

        public async Task<int> DeleteExerciseAsync(int id)
        {
            await Init();
            if (_database == null) return 0;

            var exo = await GetExerciseByIdAsync(id);
            if (exo != null)
            {
                exo.IsArchived = true;
                return await _database.UpdateAsync(exo);
            }
            return 0;
        }
    }
}