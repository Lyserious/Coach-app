using Coach_app.Data.Context;
using Coach_app.Models;
using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Domains.Training;
using SQLite;

namespace Coach_app.Data.Repositories
{
    public class SessionRepository : Interfaces.ISessionRepository
    {
        private readonly CoachDbContext _context;
        // Raccourci pour accéder à la connexion thread-safe
        private SQLiteAsyncConnection Connection => _context.Connection;

        public SessionRepository(CoachDbContext context)
        {
            _context = context;
        }

        public async Task<GroupSession> GetSessionByIdAsync(int id)
        {
            await _context.InitAsync();
            return await Connection.Table<GroupSession>().FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<GroupSession>> GetSessionsByGroupIdAsync(int groupId)
        {
            await _context.InitAsync();
            return await Connection.Table<GroupSession>()
                                  .Where(s => s.GroupId == groupId)
                                  .OrderBy(s => s.Date)
                                  .ToListAsync();
        }

        public async Task<List<GroupSession>> GetSessionsByDateAsync(DateTime date)
        {
            await _context.InitAsync();
            var start = date.Date;
            var end = date.Date.AddDays(1).AddTicks(-1);
            return await Connection.Table<GroupSession>()
                                  .Where(s => s.Date >= start && s.Date <= end)
                                  .OrderBy(s => s.StartTime)
                                  .ToListAsync();
        }

        public async Task AddSessionAsync(GroupSession session)
        {
            await _context.InitAsync();
            await Connection.InsertAsync(session);
        }

        public async Task UpdateSessionAsync(GroupSession session)
        {
            await _context.InitAsync();
            await Connection.UpdateAsync(session);
        }

        public async Task DeleteSessionAsync(int sessionId)
        {
            await _context.InitAsync();
            // Nettoyage en cascade (Appel + Exos + Perfs devraient être gérés ici idéalement)
            var attendances = await Connection.Table<SessionAttendance>().Where(a => a.GroupSessionId == sessionId).ToListAsync();
            foreach (var att in attendances) await Connection.DeleteAsync(att);

            await Connection.DeleteAsync<GroupSession>(sessionId);
        }

        // --- Contenu Séance ---

        public async Task<List<SessionExercise>> GetExercisesForSessionAsync(int sessionId)
        {
            await _context.InitAsync();
            var links = await Connection.Table<SessionExercise>()
                                       .Where(x => x.GroupSessionId == sessionId)
                                       .OrderBy(x => x.OrderIndex)
                                       .ToListAsync();

            foreach (var link in links)
            {
                // Note : Idéalement on ferait un JOIN, mais SQLite-net-pcl est limité.
                link.Exercise = await Connection.Table<Exercise>().FirstOrDefaultAsync(e => e.Id == link.ExerciseId);
            }
            return links;
        }

        public async Task SaveSessionExerciseAsync(SessionExercise sessionExercise)
        {
            await _context.InitAsync();
            if (sessionExercise.Id != 0) await Connection.UpdateAsync(sessionExercise);
            else await Connection.InsertAsync(sessionExercise);
        }

        public async Task DeleteSessionExerciseAsync(SessionExercise sessionExercise)
        {
            await _context.InitAsync();
            await Connection.DeleteAsync(sessionExercise);
        }

        // --- Appel ---

        public async Task<List<SessionAttendance>> GetAttendanceForSessionAsync(int sessionId)
        {
            await _context.InitAsync();
            return await Connection.Table<SessionAttendance>().Where(a => a.GroupSessionId == sessionId).ToListAsync();
        }

        public async Task SaveAttendanceListAsync(List<SessionAttendance> attendanceList)
        {
            await _context.InitAsync();
            foreach (var item in attendanceList)
            {
                if (item.Id > 0) await Connection.UpdateAsync(item);
                else await Connection.InsertAsync(item);
            }
        }

        // --- Génération (Migration telle quelle pour l'instant) ---
        public async Task GenerateSessionsForGroupAsync(Group group)
        {
            await _context.InitAsync();
            var today = DateTime.Today;

            // Suppression des futures séances non réalisées pour régénération
            var futureSessions = await Connection.Table<GroupSession>()
                                             .Where(s => s.GroupId == group.Id && s.Date >= today && s.Status == "Scheduled")
                                             .ToListAsync();
            foreach (var s in futureSessions) await Connection.DeleteAsync(s);

            // Récupération des dates passées pour éviter doublons
            var existingPastSessions = await Connection.Table<GroupSession>()
                                                      .Where(s => s.GroupId == group.Id && s.Date < today)
                                                      .ToListAsync();
            var existingDates = existingPastSessions.Select(s => s.Date.Date).ToHashSet(); // HashSet pour perf

            var sessionsToAdd = new List<GroupSession>();

            if (group.RecurrenceDay.HasValue)
            {
                var currentDate = group.StartDate;
                // Caler sur le bon jour
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

            if (sessionsToAdd.Count > 0) await Connection.InsertAllAsync(sessionsToAdd);
        }
    }
}