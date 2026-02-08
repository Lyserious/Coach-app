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
            if (currentCoach == null) return;

            
            string dbPath = Constants.GetCoachDbPath(currentCoach.DataFileName);
            _database = new SQLiteAsyncConnection(dbPath, Constants.Flags);

            
            await _database.CreateTableAsync<Group>();
            await _database.CreateTableAsync<GroupSession>();
            await _database.CreateTableAsync<Student>();
            await _database.CreateTableAsync<StudentGroup>(); // Table de liaison

            
            await _database.CreateTableAsync<SessionAttendance>();
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

        public async Task<List<GroupSession>> GetSessionsByGroupIdAsync(int groupId)
        {
            await Init();
            if (_database == null) return new List<GroupSession>();

            return await _database.Table<GroupSession>()
                                  .Where(s => s.GroupId == groupId)
                                  .OrderBy(s => s.Date)
                                  .ToListAsync();
        }

        public async Task<List<GroupSession>> GetSessionsByDateAsync(DateTime date)
        {
            await Init();
            if (_database == null) return new List<GroupSession>();

            var startOfDay = date.Date;
            var endOfDay = date.Date.AddDays(1).AddTicks(-1);

            return await _database.Table<GroupSession>()
                                  .Where(s => s.Date >= startOfDay && s.Date <= endOfDay)
                                  .OrderBy(s => s.StartTime)
                                  .ToListAsync();
        }

        public async Task GenerateSessionsForGroupAsync(Group group)
        {
            await Init();
            if (_database == null) return;

            // 1. Supprimer les futures séances "Scheduled" pour éviter les doublons
            var today = DateTime.Today;
            var oldSessions = await _database.Table<GroupSession>()
                                             .Where(s => s.GroupId == group.Id && s.Date >= today && s.Status == "Scheduled")
                                             .ToListAsync();

            foreach (var s in oldSessions)
            {
                await _database.DeleteAsync(s);
            }

            // 2. Générer selon le type
            var sessionsToAdd = new List<GroupSession>();

            // Cas A : Cours Hebdomadaire
            if (group.RecurrenceDay.HasValue)
            {
                var currentDate = group.StartDate;
                // Avancer jusqu'au bon jour de la semaine
                while (currentDate.DayOfWeek != group.RecurrenceDay.Value)
                {
                    currentDate = currentDate.AddDays(1);
                }

                while (currentDate <= group.EndDate)
                {
                    sessionsToAdd.Add(new GroupSession
                    {
                        GroupId = group.Id,
                        Date = currentDate,
                        StartTime = group.StartTime,
                        EndTime = group.EndTime,
                        Status = "Scheduled"
                    });
                    currentDate = currentDate.AddDays(7); // +1 semaine
                }
            }
            // Cas B : Stage (Tous les jours)
            else if (group.Type == GroupType.Internship)
            {
                var currentDate = group.StartDate;
                while (currentDate <= group.EndDate)
                {
                    sessionsToAdd.Add(new GroupSession
                    {
                        GroupId = group.Id,
                        Date = currentDate,
                        StartTime = group.StartTime,
                        EndTime = group.EndTime,
                        Status = "Scheduled"
                    });
                    currentDate = currentDate.AddDays(1); // +1 jour
                }
            }

            // 3. Insérer en masse
            if (sessionsToAdd.Count > 0)
            {
                await _database.InsertAllAsync(sessionsToAdd);
            }
        }
        public async Task<List<SessionAttendance>> GetAttendanceForSessionAsync(int sessionId)
        {
            await Init();
            if (_database == null) return new List<SessionAttendance>();

            return await _database.Table<SessionAttendance>()
                                  .Where(a => a.GroupSessionId == sessionId)
                                  .ToListAsync();
        }

        public async Task SaveAttendanceListAsync(List<SessionAttendance> attendanceList)
        {
            await Init();
            if (_database == null || attendanceList == null || !attendanceList.Any()) return;

            // On sauvegarde chaque ligne (Update si ID existe, Insert sinon)
            foreach (var item in attendanceList)
            {
                if (item.Id > 0)
                    await _database.UpdateAsync(item);
                else
                    await _database.InsertAsync(item);
            }
        }
        
        public async Task<GroupSession> GetSessionByIdAsync(int id)
        {
            await Init();
            if (_database == null) return null;
            return await _database.Table<GroupSession>().Where(s => s.Id == id).FirstOrDefaultAsync();
        }
    }
}