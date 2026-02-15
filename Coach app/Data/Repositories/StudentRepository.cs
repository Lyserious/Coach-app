using Coach_app.Data.Repositories.Interfaces;
using Coach_app.Core.Constants;
using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Domains.Students;
  // S'assurer du namespace
using Coach_app.Services.Data;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ICoachDatabaseService _dbService;
        private readonly IStudentContactRepository _contactRepo;

        public StudentRepository(ICoachDatabaseService dbService, IStudentContactRepository contactRepo)
        {
            _dbService = dbService;
            _contactRepo = contactRepo;
        }

        // --- CRUD ÉLÈVE ---

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            var db = await _dbService.GetConnectionAsync();
            return await db.Table<Student>().ToListAsync();
        }

        public async Task<Student> GetStudentByIdAsync(int id)
        {
            var db = await _dbService.GetConnectionAsync();
            return await db.Table<Student>().Where(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveStudentAsync(Student student)
        {
            var db = await _dbService.GetConnectionAsync();
            if (student.Id != 0)
                await db.UpdateAsync(student);
            else
                await db.InsertAsync(student);

            return student.Id;
        }

        public async Task DeleteStudentAsync(Student student)
        {
            var db = await _dbService.GetConnectionAsync();

            // 1. Supprimer l'élève
            await db.DeleteAsync(student);

            // 2. Nettoyage en cascade

            // Liens Groupes (Code direct conservé ici car simple jointure)
            var links = await db.Table<StudentGroup>().Where(x => x.StudentId == student.Id).ToListAsync();
            foreach (var l in links) await db.DeleteAsync(l);

            // Contacts (Délégué au repo spécialisé)
            await _contactRepo.DeleteContactsByStudentIdAsync(student.Id);

            // Notes (Legacy StudentNote - code direct conservé ici pour iso-fonctionnalité)
            var notes = await db.Table<StudentNote>().Where(x => x.StudentId == student.Id).ToListAsync();
            foreach (var n in notes) await db.DeleteAsync(n);
        }

        // --- GESTION GROUPES ---

        public async Task<List<Student>> GetStudentsByGroupAsync(int groupId)
        {
            var db = await _dbService.GetConnectionAsync();
            var links = await db.Table<StudentGroup>().Where(sg => sg.GroupId == groupId).ToListAsync();
            if (!links.Any()) return new List<Student>();

            var ids = links.Select(sg => sg.StudentId).ToList();
            return await db.Table<Student>().Where(s => ids.Contains(s.Id)).ToListAsync();
        }

        public async Task<List<Group>> GetGroupsByStudentAsync(int studentId)
        {
            var db = await _dbService.GetConnectionAsync();
            var links = await db.Table<StudentGroup>().Where(x => x.StudentId == studentId).ToListAsync();
            if (!links.Any()) return new List<Group>();

            var ids = links.Select(x => x.GroupId).ToList();
            return await db.Table<Group>().Where(g => ids.Contains(g.Id)).ToListAsync();
        }

        public async Task AddStudentToGroupAsync(int studentId, int groupId)
        {
            var db = await _dbService.GetConnectionAsync();
            var exists = await db.Table<StudentGroup>()
                            .Where(sg => sg.StudentId == studentId && sg.GroupId == groupId)
                            .FirstOrDefaultAsync();

            if (exists == null)
            {
                await db.InsertAsync(new StudentGroup { StudentId = studentId, GroupId = groupId });
            }
        }

        public async Task RemoveStudentFromGroupAsync(int studentId, int groupId)
        {
            var db = await _dbService.GetConnectionAsync();
            var link = await db.Table<StudentGroup>()
                            .Where(sg => sg.StudentId == studentId && sg.GroupId == groupId)
                            .FirstOrDefaultAsync();
            if (link != null) await db.DeleteAsync(link);
        }

        // --- GESTION CONTACTS (Délégation Facade) ---

        public async Task<List<StudentContact>> GetStudentContactsAsync(int studentId)
            => await _contactRepo.GetStudentContactsAsync(studentId);

        public async Task SaveContactAsync(StudentContact contact)
            => await _contactRepo.SaveContactAsync(contact);

        public async Task DeleteContactAsync(int contactId)
            => await _contactRepo.DeleteContactAsync(contactId);

        // --- GESTION NOTES (LEGACY) ---

        public async Task<List<StudentNote>> GetStudentNotesAsync(int studentId)
        {
            var db = await _dbService.GetConnectionAsync();
            return await db.Table<StudentNote>()
                                  .Where(n => n.StudentId == studentId)
                                  .OrderByDescending(n => n.Date)
                                  .ToListAsync();
        }

        public async Task SaveNoteAsync(StudentNote note)
        {
            var db = await _dbService.GetConnectionAsync();
            if (note.Id != 0)
                await db.UpdateAsync(note);
            else
                await db.InsertAsync(note);
        }

        public async Task<List<Student>> GetStudentsByGroupIdAsync(int groupId)
        {
            // Redondant avec GetStudentsByGroupAsync mais conservé pour compatibilité interface
            return await GetStudentsByGroupAsync(groupId);
        }
    }
}