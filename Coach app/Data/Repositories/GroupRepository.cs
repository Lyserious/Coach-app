using Coach_app.Models;
using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Domains.Students;
using Coach_app.Data.Repositories.Interfaces;
using Coach_app.Services.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Coach_app.Data.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        // On injecte nos nouveaux spécialistes
        private readonly ICoachDatabaseService _dbService;
        private readonly ISessionRepository _sessionRepo;
        private readonly IAttendanceRepository _attendanceRepo;
        private readonly IPerformanceRepository _performanceRepo;
        private readonly ITemplateRepository _templateRepo;

        public GroupRepository(
            ICoachDatabaseService dbService,
            ISessionRepository sessionRepo,
            IAttendanceRepository attendanceRepo,
            IPerformanceRepository performanceRepo,
            ITemplateRepository templateRepo)
        {
            _dbService = dbService;
            _sessionRepo = sessionRepo;
            _attendanceRepo = attendanceRepo;
            _performanceRepo = performanceRepo;
            _templateRepo = templateRepo;
        }

        // =========================================================
        // 1. GESTION DES GROUPES (Reste ici car spécifique)
        // =========================================================

        public async Task<List<Group>> GetGroupsAsync()
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return new List<Group>();
            return await db.Table<Group>().ToListAsync();
        }

        public async Task<Group> GetGroupByIdAsync(int id)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return null;
            return await db.Table<Group>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task SaveGroupAsync(Group group)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return;

            if (group.Id != 0)
                await db.UpdateAsync(group);
            else
                await db.InsertAsync(group);

            // On délègue la génération au SessionRepository
            await _sessionRepo.GenerateSessionsForGroupAsync(group);
        }

        public async Task DeleteGroupAsync(Group group)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return;

            // Délégation des nettoyages
            var sessions = await _sessionRepo.GetSessionsByGroupIdAsync(group.Id);
            foreach (var s in sessions) await _sessionRepo.DeleteSessionAsync(s.Id);

            var links = await db.Table<StudentGroup>().Where(l => l.GroupId == group.Id).ToListAsync();
            foreach (var l in links) await db.DeleteAsync(l);

            var photos = await GetPhotosByGroupIdAsync(group.Id);
            foreach (var p in photos) await DeletePhotoAsync(p);

            await db.DeleteAsync(group);
        }

        // =========================================================
        // 2. GESTION DES ÉLÈVES (Lecture simple, reste ici)
        // =========================================================

        public async Task<List<Student>> GetStudentsByGroupIdAsync(int groupId)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return new List<Student>();
            return await db.QueryAsync<Student>(
                "SELECT S.* FROM Student S INNER JOIN StudentGroup SG ON S.Id = SG.StudentId WHERE SG.GroupId = ?",
                groupId);
        }

        // =========================================================
        // 3. CALENDRIER ET SÉANCES (Délégation totale)
        // =========================================================

        public async Task<List<GroupSession>> GetSessionsByGroupIdAsync(int groupId)
            => await _sessionRepo.GetSessionsByGroupIdAsync(groupId);

        public async Task<List<GroupSession>> GetSessionsByDateAsync(DateTime date)
            => await _sessionRepo.GetSessionsByDateAsync(date);

        public async Task GenerateSessionsForGroupAsync(Group group)
            => await _sessionRepo.GenerateSessionsForGroupAsync(group);

        public async Task<GroupSession> GetSessionByIdAsync(int id)
            => await _sessionRepo.GetSessionByIdAsync(id);

        public async Task AddSessionAsync(GroupSession session)
            => await _sessionRepo.AddSessionAsync(session);

        public async Task UpdateSessionAsync(GroupSession session)
            => await _sessionRepo.UpdateSessionAsync(session);

        public async Task DeleteSessionAsync(int sessionId)
            => await _sessionRepo.DeleteSessionAsync(sessionId);

        // =========================================================
        // 4. PRÉSENCES (Délégation totale)
        // =========================================================

        public async Task<List<SessionAttendance>> GetAttendanceForSessionAsync(int sessionId)
            => await _attendanceRepo.GetAttendanceForSessionAsync(sessionId);

        public async Task SaveAttendanceListAsync(List<SessionAttendance> attendanceList)
            => await _attendanceRepo.SaveAttendanceListAsync(attendanceList);

        // =========================================================
        // 5. PHOTOS (Reste ici pour l'instant)
        // =========================================================

        public async Task<List<GroupPhoto>> GetPhotosByGroupIdAsync(int groupId)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return new List<GroupPhoto>();
            return await db.Table<GroupPhoto>()
                                  .Where(p => p.GroupId == groupId)
                                  .OrderByDescending(p => p.DateTaken)
                                  .ToListAsync();
        }

        public async Task AddPhotoAsync(GroupPhoto photo)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return;
            await db.InsertAsync(photo);
        }

        public async Task<GroupPhoto> GetPhotoByIdAsync(int id)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return null;
            return await db.Table<GroupPhoto>().Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdatePhotoAsync(GroupPhoto photo)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return;
            await db.UpdateAsync(photo);
        }

        public async Task DeletePhotoAsync(GroupPhoto photo)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return;
            await db.DeleteAsync(photo);
            if (File.Exists(photo.FilePath)) try { File.Delete(photo.FilePath); } catch { }
        }

        // =========================================================
        // 7. CONTENU SÉANCES (Délégation totale)
        // =========================================================

        public async Task<List<SessionExercise>> GetExercisesForSessionAsync(int sessionId)
            => await _sessionRepo.GetExercisesForSessionAsync(sessionId);

        public async Task SaveSessionExerciseAsync(SessionExercise sessionExercise)
            => await _sessionRepo.SaveSessionExerciseAsync(sessionExercise);

        public async Task DeleteSessionExerciseAsync(SessionExercise sessionExercise)
            => await _sessionRepo.DeleteSessionExerciseAsync(sessionExercise);

        // =========================================================
        // 8. TEMPLATES (Délégation totale)
        // =========================================================

        public async Task<List<SessionTemplate>> GetAllTemplatesAsync()
            => await _templateRepo.GetAllTemplatesAsync();

        public async Task<List<SessionTemplateExercise>> GetTemplateExercisesAsync(int templateId)
            => await _templateRepo.GetTemplateExercisesAsync(templateId);

        public async Task SaveTemplateAsync(SessionTemplate template, List<SessionTemplateExercise> exercises)
            => await _templateRepo.SaveTemplateAsync(template, exercises);

        public async Task DeleteTemplateAsync(SessionTemplate template)
            => await _templateRepo.DeleteTemplateAsync(template);

        public async Task ImportTemplateToSessionAsync(int templateId, int targetSessionId)
            => await _sessionRepo.ImportTemplateToSessionAsync(templateId, targetSessionId, _templateRepo);

        // =========================================================
        // 9. PERFORMANCES (Délégation totale)
        // =========================================================

        public async Task<List<Performance>> GetPerformancesBySessionExerciseAsync(int sessionExerciseId)
            => await _performanceRepo.GetPerformancesBySessionExerciseAsync(sessionExerciseId);

        public async Task SavePerformanceAsync(Performance perf)
            => await _performanceRepo.SavePerformanceAsync(perf);

        public async Task DeletePerformanceAsync(Performance perf)
            => await _performanceRepo.DeletePerformanceAsync(perf);
    }
}