using Coach_app.Configurations;
using Coach_app.Models;
using Coach_app.Services.Auth; // Ou Coach_app.Services selon où est ton ISessionService
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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
            await _database.CreateTableAsync<StudentGroup>();
            await _database.CreateTableAsync<SessionAttendance>();
            await _database.CreateTableAsync<GroupPhoto>();
        }

        // =========================================================
        // 1. GESTION DES GROUPES (Corrigé pour matcher l'interface)
        // =========================================================

        public async Task<List<Group>> GetGroupsAsync()
        {
            await Init();
            if (_database == null) return new List<Group>();
            return await _database.Table<Group>().ToListAsync();
        }

        public async Task<Group> GetGroupByIdAsync(int id)
        {
            await Init();
            if (_database == null) return null;
            return await _database.Table<Group>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task SaveGroupAsync(Group group)
        {
            await Init();
            if (_database == null) return;

            if (group.Id != 0)
                await _database.UpdateAsync(group);
            else
                await _database.InsertAsync(group);

            // Important : On lance la génération des séances après sauvegarde
            await GenerateSessionsForGroupAsync(group);
        }

        public async Task DeleteGroupAsync(Group group)
        {
            await Init();
            if (_database == null) return;

            // 1. Supprimer les séances
            var sessions = await GetSessionsByGroupIdAsync(group.Id);
            foreach (var s in sessions) await DeleteSessionAsync(s.Id);

            // 2. Supprimer les liens élèves
            var links = await _database.Table<StudentGroup>().Where(l => l.GroupId == group.Id).ToListAsync();
            foreach (var l in links) await _database.DeleteAsync(l);

            // 3. Supprimer les photos
            var photos = await GetPhotosByGroupIdAsync(group.Id);
            foreach (var p in photos) await DeletePhotoAsync(p);

            // 4. Supprimer le groupe
            await _database.DeleteAsync(group);
        }

        // =========================================================
        // 2. GESTION DES ÉLÈVES (C'était manquant)
        // =========================================================

        public async Task<List<Student>> GetStudentsByGroupIdAsync(int groupId)
        {
            await Init();
            if (_database == null) return new List<Student>();

            // Requête SQL pour récupérer les élèves liés au groupe
            return await _database.QueryAsync<Student>(
                "SELECT S.* FROM Student S INNER JOIN StudentGroup SG ON S.Id = SG.StudentId WHERE SG.GroupId = ?",
                groupId);
        }

        // =========================================================
        // 3. CALENDRIER ET SÉANCES
        // =========================================================

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

            var today = DateTime.Today;

            // 1. Supprimer les séances futures
            var futureSessions = await _database.Table<GroupSession>()
                                             .Where(s => s.GroupId == group.Id && s.Date >= today && s.Status == "Scheduled")
                                             .ToListAsync();
            foreach (var s in futureSessions) await _database.DeleteAsync(s);

            // 2. Récupérer les existantes passées (anti-doublon)
            var existingPastSessions = await _database.Table<GroupSession>()
                                                      .Where(s => s.GroupId == group.Id && s.Date < today)
                                                      .ToListAsync();
            var existingDates = existingPastSessions.Select(s => s.Date.Date).ToList();

            var sessionsToAdd = new List<GroupSession>();

            if (group.RecurrenceDay.HasValue)
            {
                var currentDate = group.StartDate;
                while (currentDate.DayOfWeek != group.RecurrenceDay.Value)
                    currentDate = currentDate.AddDays(1);

                while (currentDate <= group.EndDate)
                {
                    if (currentDate < today && existingDates.Contains(currentDate.Date))
                    {
                        currentDate = currentDate.AddDays(7);
                        continue;
                    }
                    sessionsToAdd.Add(new GroupSession { GroupId = group.Id, Date = currentDate, StartTime = group.StartTime, EndTime = group.EndTime, Status = "Scheduled" });
                    currentDate = currentDate.AddDays(7);
                }
            }
            else if (group.Type == GroupType.Internship)
            {
                var currentDate = group.StartDate;
                while (currentDate <= group.EndDate)
                {
                    if (currentDate < today && existingDates.Contains(currentDate.Date))
                    {
                        currentDate = currentDate.AddDays(1);
                        continue;
                    }
                    sessionsToAdd.Add(new GroupSession { GroupId = group.Id, Date = currentDate, StartTime = group.StartTime, EndTime = group.EndTime, Status = "Scheduled" });
                    currentDate = currentDate.AddDays(1);
                }
            }

            if (sessionsToAdd.Count > 0) await _database.InsertAllAsync(sessionsToAdd);
        }

        public async Task<GroupSession> GetSessionByIdAsync(int id)
        {
            await Init();
            if (_database == null) return null;
            return await _database.Table<GroupSession>().Where(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddSessionAsync(GroupSession session)
        {
            await Init();
            if (_database == null) return;
            await _database.InsertAsync(session);
        }

        public async Task DeleteSessionAsync(int sessionId)
        {
            await Init();
            if (_database == null) return;

            var attendances = await _database.Table<SessionAttendance>().Where(a => a.GroupSessionId == sessionId).ToListAsync();
            foreach (var att in attendances) await _database.DeleteAsync(att);

            await _database.DeleteAsync<GroupSession>(sessionId);
        }

        // =========================================================
        // 4. PRÉSENCES
        // =========================================================

        public async Task<List<SessionAttendance>> GetAttendanceForSessionAsync(int sessionId)
        {
            await Init();
            if (_database == null) return new List<SessionAttendance>();
            return await _database.Table<SessionAttendance>().Where(a => a.GroupSessionId == sessionId).ToListAsync();
        }

        public async Task SaveAttendanceListAsync(List<SessionAttendance> attendanceList)
        {
            await Init();
            if (_database == null || attendanceList == null || !attendanceList.Any()) return;

            foreach (var item in attendanceList)
            {
                if (item.Id > 0) await _database.UpdateAsync(item);
                else await _database.InsertAsync(item);
            }
        }

        // =========================================================
        // 5. PHOTOS
        // =========================================================

        public async Task<List<GroupPhoto>> GetPhotosByGroupIdAsync(int groupId)
        {
            await Init();
            if (_database == null) return new List<GroupPhoto>();
            return await _database.Table<GroupPhoto>()
                                  .Where(p => p.GroupId == groupId)
                                  .OrderByDescending(p => p.DateTaken)
                                  .ToListAsync();
        }

        public async Task AddPhotoAsync(GroupPhoto photo)
        {
            await Init();
            if (_database == null) return;
            await _database.InsertAsync(photo);
        }

        public async Task<GroupPhoto> GetPhotoByIdAsync(int id)
        {
            await Init();
            if (_database == null) return null;
            return await _database.Table<GroupPhoto>().Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdatePhotoAsync(GroupPhoto photo)
        {
            await Init();
            if (_database == null) return;
            await _database.UpdateAsync(photo);
        }

        public async Task DeletePhotoAsync(GroupPhoto photo)
        {
            await Init();
            if (_database == null) return;
            await _database.DeleteAsync(photo);

            if (File.Exists(photo.FilePath))
            {
                try { File.Delete(photo.FilePath); } catch { }
            }
        }
    }
}