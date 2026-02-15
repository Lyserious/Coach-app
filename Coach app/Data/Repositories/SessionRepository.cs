using Coach_app.Models;
using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Domains.Training;
using Coach_app.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    // On force explicitement l'interface du dossier Interfaces
    public class SessionRepository : Coach_app.Data.Repositories.Interfaces.ISessionRepository
    {
        private readonly ICoachDatabaseService _dbService;
        private readonly Coach_app.Data.Repositories.Interfaces.IAttendanceRepository _attendanceRepo;

        public SessionRepository(ICoachDatabaseService dbService, Coach_app.Data.Repositories.Interfaces.IAttendanceRepository attendanceRepo)
        {
            _dbService = dbService;
            _attendanceRepo = attendanceRepo;
        }

        public async Task<List<GroupSession>> GetSessionsByGroupIdAsync(int groupId)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return new List<GroupSession>();
            return await db.Table<GroupSession>()
                                  .Where(s => s.GroupId == groupId)
                                  .OrderBy(s => s.Date)
                                  .ToListAsync();
        }

        public async Task<List<GroupSession>> GetSessionsByDateAsync(DateTime date)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return new List<GroupSession>();

            var startOfDay = date.Date;
            var endOfDay = date.Date.AddDays(1).AddTicks(-1);

            return await db.Table<GroupSession>()
                                  .Where(s => s.Date >= startOfDay && s.Date <= endOfDay)
                                  .OrderBy(s => s.StartTime)
                                  .ToListAsync();
        }

        public async Task<GroupSession> GetSessionByIdAsync(int id)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return null;
            return await db.Table<GroupSession>().Where(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddSessionAsync(GroupSession session)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return;
            await db.InsertAsync(session);
        }

        public async Task UpdateSessionAsync(GroupSession session)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db != null) await db.UpdateAsync(session);
        }

        public async Task DeleteSessionAsync(int sessionId)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return;

            await _attendanceRepo.DeleteAttendanceForSessionAsync(sessionId);
            await db.DeleteAsync<GroupSession>(sessionId);
        }

        public async Task GenerateSessionsForGroupAsync(Group group)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return;

            var today = DateTime.Today;

            var futureSessions = await db.Table<GroupSession>()
                                             .Where(s => s.GroupId == group.Id && s.Date >= today && s.Status == "Scheduled")
                                             .ToListAsync();
            foreach (var s in futureSessions) await db.DeleteAsync(s);

            var existingPastSessions = await db.Table<GroupSession>()
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

            if (sessionsToAdd.Count > 0) await db.InsertAllAsync(sessionsToAdd);
        }

        // --- CONTENU SÉANCES ---

        public async Task<List<SessionExercise>> GetExercisesForSessionAsync(int sessionId)
        {
            var db = await _dbService.GetConnectionAsync();
            var links = await db.Table<SessionExercise>()
                                       .Where(x => x.GroupSessionId == sessionId)
                                       .OrderBy(x => x.OrderIndex)
                                       .ToListAsync();

            foreach (var link in links)
            {
                link.Exercise = await db.Table<Exercise>().Where(e => e.Id == link.ExerciseId).FirstOrDefaultAsync();
            }
            return links;
        }

        public async Task SaveSessionExerciseAsync(SessionExercise sessionExercise)
        {
            var db = await _dbService.GetConnectionAsync();
            if (sessionExercise.Id != 0)
                await db.UpdateAsync(sessionExercise);
            else
                await db.InsertAsync(sessionExercise);
        }

        public async Task DeleteSessionExerciseAsync(SessionExercise sessionExercise)
        {
            var db = await _dbService.GetConnectionAsync();
            await db.DeleteAsync(sessionExercise);
        }

        // Signature mise à jour avec le namespace complet
        public async Task ImportTemplateToSessionAsync(int templateId, int targetSessionId, Coach_app.Data.Repositories.Interfaces.ITemplateRepository templateRepo)
        {
            var db = await _dbService.GetConnectionAsync();

            var template = await templateRepo.GetTemplateByIdAsync(templateId);
            var templateExercises = await templateRepo.GetTemplateExercisesAsync(templateId);

            var targetSession = await GetSessionByIdAsync(targetSessionId);
            if (targetSession != null && template != null)
            {
                targetSession.Description = template.Description;
                await UpdateSessionAsync(targetSession);
            }

            var newSessionExercises = new List<SessionExercise>();
            var existing = await db.Table<SessionExercise>().Where(x => x.GroupSessionId == targetSessionId).ToListAsync();
            int startIndex = existing.Count + 1;

            foreach (var tExo in templateExercises)
            {
                newSessionExercises.Add(new SessionExercise
                {
                    GroupSessionId = targetSessionId,
                    ExerciseId = tExo.ExerciseId,
                    OrderIndex = startIndex++,
                    Sets = tExo.Sets,
                    Reps = tExo.Reps,
                    Weight = tExo.Weight,
                    Rest = tExo.Rest,
                    Note = tExo.Note
                });
            }
            if (newSessionExercises.Any()) await db.InsertAllAsync(newSessionExercises);
        }
    }
}