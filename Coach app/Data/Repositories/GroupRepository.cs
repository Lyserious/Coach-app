using Coach_app.Configurations;
using Coach_app.Models;
using Coach_app.Services.Auth;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Threading; // Nécessaire pour le SemaphoreSlim

namespace Coach_app.Data.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private SQLiteAsyncConnection _database;
        private readonly ISessionService _sessionService;

        // --- SECURITÉ INITIALISATION (Ajout) ---
        // Ce verrou empêche que deux parties de l'app essaient de créer les tables en même temps
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private bool _isInitialized = false;

        public GroupRepository(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        // Méthode Init() sécurisée
        private async Task Init()
        {
            // 1. Si c'est déjà prêt, on ne fait rien (Rapide)
            if (_isInitialized && _database != null) return;

            // 2. Sinon, on attend son tour (Sécurité)
            await _initLock.WaitAsync();
            try
            {
                // 3. On revérifie une fois qu'on a le ticket d'entrée
                if (_isInitialized && _database != null) return;

                var currentCoach = _sessionService.CurrentCoach;
                if (currentCoach == null) return;

                string dbPath = Constants.GetCoachDbPath(currentCoach.DataFileName);
                _database = new SQLiteAsyncConnection(dbPath, Constants.Flags);

                // 4. Création des tables (Ordre garanti)
                await _database.CreateTableAsync<Group>();
                await _database.CreateTableAsync<GroupSession>();
                await _database.CreateTableAsync<Student>();
                await _database.CreateTableAsync<StudentGroup>();
                await _database.CreateTableAsync<SessionAttendance>();
                await _database.CreateTableAsync<GroupPhoto>();
                await _database.CreateTableAsync<Exercise>();
                await _database.CreateTableAsync<SessionExercise>();
                await _database.CreateTableAsync<SessionTemplate>();
                await _database.CreateTableAsync<SessionTemplateExercise>();
                await _database.CreateTableAsync<Performance>();

                _isInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        // =========================================================
        // 1. GESTION DES GROUPES
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
        // 2. GESTION DES ÉLÈVES
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

        // =========================================================
        // 7. GESTION DU CONTENU DES SÉANCES & TEMPLATES
        // =========================================================

        public async Task<List<SessionExercise>> GetExercisesForSessionAsync(int sessionId)
        {
            await Init();
            var links = await _database.Table<SessionExercise>()
                                       .Where(x => x.GroupSessionId == sessionId)
                                       .OrderBy(x => x.OrderIndex)
                                       .ToListAsync();

            foreach (var link in links)
            {
                link.Exercise = await _database.Table<Exercise>().Where(e => e.Id == link.ExerciseId).FirstOrDefaultAsync();
            }
            return links;
        }

        public async Task SaveSessionExerciseAsync(SessionExercise sessionExercise)
        {
            await Init();
            if (sessionExercise.Id != 0)
                await _database.UpdateAsync(sessionExercise);
            else
                await _database.InsertAsync(sessionExercise);
        }

        public async Task DeleteSessionExerciseAsync(SessionExercise sessionExercise)
        {
            await Init();
            await _database.DeleteAsync(sessionExercise);
        }

        // --- GESTION DES TEMPLATES ---

        public async Task<List<SessionTemplate>> GetAllTemplatesAsync()
        {
            await Init();
            return await _database.Table<SessionTemplate>().ToListAsync();
        }

        public async Task SaveTemplateAsync(SessionTemplate template, List<SessionTemplateExercise> exercises)
        {
            await Init();
            if (template.Id != 0) await _database.UpdateAsync(template);
            else await _database.InsertAsync(template);

            var oldExos = await _database.Table<SessionTemplateExercise>().Where(x => x.TemplateId == template.Id).ToListAsync();
            foreach (var old in oldExos) await _database.DeleteAsync(old);

            foreach (var ex in exercises)
            {
                ex.TemplateId = template.Id;
            }
            if (exercises.Any()) await _database.InsertAllAsync(exercises);
        }

        public async Task ImportTemplateToSessionAsync(int templateId, int targetSessionId)
        {
            await Init();

            var templateExercises = await _database.Table<SessionTemplateExercise>()
                                                   .Where(x => x.TemplateId == templateId)
                                                   .ToListAsync();

            var newSessionExercises = new List<SessionExercise>();

            foreach (var tExo in templateExercises)
            {
                newSessionExercises.Add(new SessionExercise
                {
                    GroupSessionId = targetSessionId,
                    ExerciseId = tExo.ExerciseId,
                    OrderIndex = tExo.OrderIndex,
                    Sets = tExo.Sets,
                    Reps = tExo.Reps,
                    Weight = tExo.Weight,
                    Rest = tExo.Rest,
                    Note = tExo.Note
                });
            }

            if (newSessionExercises.Any())
                await _database.InsertAllAsync(newSessionExercises);
        }

        // =========================================================
        // 8. GESTION DES PERFORMANCES
        // =========================================================

        public async Task<List<Performance>> GetPerformancesAsync(int sessionId, int exerciseId)
        {
            await Init();
            return await _database.Table<Performance>()
                                  .Where(p => p.GroupSessionId == sessionId && p.ExerciseId == exerciseId)
                                  .ToListAsync();
        }

        public async Task SavePerformanceAsync(Performance perf)
        {
            await Init();
            if (perf.Id != 0)
                await _database.UpdateAsync(perf);
            else
                await _database.InsertAsync(perf);
        }

        public async Task DeletePerformanceAsync(Performance perf)
        {
            await Init();
            await _database.DeleteAsync(perf);
        }
    }
}