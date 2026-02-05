using Coach_app.Configurations;
using Coach_app.Models;
using Coach_app.Services.Auth;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private SQLiteAsyncConnection _database;
        private readonly ISessionService _sessionService;

        public GroupRepository(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        private async Task Init()
        {
            if (_database != null) return;

            var currentCoach = _sessionService.CurrentCoach;

            // SÉCURITÉ : Si pas de coach connecté, on ne fait rien
            if (currentCoach == null)
                return;

            string dbPath = Constants.GetCoachDbPath(currentCoach.DataFileName);
            _database = new SQLiteAsyncConnection(dbPath, Constants.Flags);
            await _database.CreateTableAsync<Group>();
        }

        public async Task<List<Group>> GetActiveGroupsAsync()
        {
            await Init();

            // SÉCURITÉ : Si la DB est null, on renvoie une liste vide pour éviter le crash
            if (_database == null)
                return new List<Group>();

            return await _database.Table<Group>().Where(g => !g.IsArchived).ToListAsync();
        }

        public async Task<Group> GetGroupByIdAsync(int id)
        {
            await Init();
            if (_database == null) return null;
            return await _database.Table<Group>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveGroupAsync(Group group)
        {
            await Init();
            if (_database == null) return 0;

            if (group.Id != 0)
                return await _database.UpdateAsync(group);
            else
                return await _database.InsertAsync(group);
        }

        public async Task<int> DeleteGroupAsync(int id)
        {
            await Init();
            if (_database == null) return 0;

            var group = await GetGroupByIdAsync(id);
            if (group != null)
            {
                group.IsArchived = true;
                return await _database.UpdateAsync(group);
            }
            return 0;
        }
    }
}