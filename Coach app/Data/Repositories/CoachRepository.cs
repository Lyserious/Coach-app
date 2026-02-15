using Coach_app.Core.Constants;
using Coach_app.Data.Repositories.Interfaces;
using Coach_app.Models.Domains.Identity;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    public class CoachRepository : ICoachRepository
    {
        private SQLiteAsyncConnection _database;

        public CoachRepository()
        {
            // On diffère l'initialisation pour ne pas bloquer le constructeur
        }

        private async Task Init()
        {
            if (_database != null)
                return;

            _database = new SQLiteAsyncConnection(Constants.GlobalDbPath, Constants.Flags);
            await _database.CreateTableAsync<Coach>();
        }

        public async Task InitializeAsync()
        {
            await Init();
        }

        public async Task<List<Coach>> GetAllCoachesAsync()
        {
            await Init();
            return await _database.Table<Coach>().ToListAsync();
        }

        public async Task<Coach> GetCoachByNameAsync(string name)
        {
            await Init();
            return await _database.Table<Coach>()
                            .Where(c => c.Name.ToLower() == name.ToLower())
                            .FirstOrDefaultAsync();
        }

        public async Task<bool> AddCoachAsync(Coach coach)
        {
            await Init();
            try
            {
                await _database.InsertAsync(coach);
                return true;
            }
            catch (Exception ex)
            {
                // Gérer l'erreur (ex: nom déjà pris grâce à l'attribut [Unique])
                System.Diagnostics.Debug.WriteLine($"Erreur ajout coach: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCoachAsync(int id)
        {
            await Init();
            await _database.DeleteAsync<Coach>(id);
            return true;
        }
    }
}