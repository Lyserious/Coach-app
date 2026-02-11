using Coach_app.Core.Constants;
using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Domains.Students;
using Coach_app.Services.Auth;
using SQLite;

namespace Coach_app.Data.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private SQLiteAsyncConnection _database;
        private readonly ISessionService _sessionService;

        public StudentRepository(ISessionService sessionService)
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

            await _database.CreateTableAsync<Student>();
            await _database.CreateTableAsync<StudentGroup>();
            await _database.CreateTableAsync<StudentContact>();
            await _database.CreateTableAsync<StudentNote>();
        }

        // --- CRUD ÉLÈVE ---

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            await Init();
            return await _database.Table<Student>().ToListAsync();
        }

        public async Task<Student> GetStudentByIdAsync(int id)
        {
            await Init();
            return await _database.Table<Student>().Where(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveStudentAsync(Student student)
        {
            await Init();
            if (student.Id != 0)
                await _database.UpdateAsync(student);
            else
                await _database.InsertAsync(student);

            return student.Id;
        }

        public async Task DeleteStudentAsync(Student student)
        {
            await Init();
            // 1. Supprimer l'élève
            await _database.DeleteAsync(student);

            // 2. Nettoyage en cascade (Optionnel mais recommandé)
            // Supprimer ses liens avec les groupes
            var links = await _database.Table<StudentGroup>().Where(x => x.StudentId == student.Id).ToListAsync();
            foreach (var l in links) await _database.DeleteAsync(l);

            // Supprimer ses contacts
            var contacts = await _database.Table<StudentContact>().Where(x => x.StudentId == student.Id).ToListAsync();
            foreach (var c in contacts) await _database.DeleteAsync(c);

            // Supprimer ses notes
            var notes = await _database.Table<StudentNote>().Where(x => x.StudentId == student.Id).ToListAsync();
            foreach (var n in notes) await _database.DeleteAsync(n);
        }

        // --- GESTION GROUPES ---

        public async Task<List<Student>> GetStudentsByGroupAsync(int groupId)
        {
            await Init();
            var links = await _database.Table<StudentGroup>().Where(sg => sg.GroupId == groupId).ToListAsync();
            if (!links.Any()) return new List<Student>();

            var ids = links.Select(sg => sg.StudentId).ToList();
            return await _database.Table<Student>().Where(s => ids.Contains(s.Id)).ToListAsync();
        }

        public async Task<List<Group>> GetGroupsByStudentAsync(int studentId)
        {
            await Init();
            var links = await _database.Table<StudentGroup>().Where(x => x.StudentId == studentId).ToListAsync();
            if (!links.Any()) return new List<Group>();

            var ids = links.Select(x => x.GroupId).ToList();
            return await _database.Table<Group>().Where(g => ids.Contains(g.Id)).ToListAsync();
        }

        public async Task AddStudentToGroupAsync(int studentId, int groupId)
        {
            await Init();
            var exists = await _database.Table<StudentGroup>()
                            .Where(sg => sg.StudentId == studentId && sg.GroupId == groupId)
                            .FirstOrDefaultAsync();

            if (exists == null)
            {
                await _database.InsertAsync(new StudentGroup { StudentId = studentId, GroupId = groupId });
            }
        }

        public async Task RemoveStudentFromGroupAsync(int studentId, int groupId)
        {
            await Init();
            var link = await _database.Table<StudentGroup>()
                            .Where(sg => sg.StudentId == studentId && sg.GroupId == groupId)
                            .FirstOrDefaultAsync();
            if (link != null) await _database.DeleteAsync(link);
        }

        // --- GESTION CONTACTS ---

        public async Task<List<StudentContact>> GetStudentContactsAsync(int studentId)
        {
            await Init();
            return await _database.Table<StudentContact>()
                                  .Where(c => c.StudentId == studentId)
                                  .ToListAsync();
        }

        public async Task SaveContactAsync(StudentContact contact)
        {
            await Init();
            if (contact.Id != 0)
                await _database.UpdateAsync(contact);
            else
                await _database.InsertAsync(contact);
        }

        public async Task DeleteContactAsync(int contactId)
        {
            await Init();
            var contact = await _database.Table<StudentContact>().Where(c => c.Id == contactId).FirstOrDefaultAsync();
            if (contact != null) await _database.DeleteAsync(contact);
        }

        // --- GESTION NOTES ---

        public async Task<List<StudentNote>> GetStudentNotesAsync(int studentId)
        {
            await Init();
            return await _database.Table<StudentNote>()
                                  .Where(n => n.StudentId == studentId)
                                  .OrderByDescending(n => n.Date)
                                  .ToListAsync();
        }

        public async Task SaveNoteAsync(StudentNote note)
        {
            await Init();
            if (note.Id != 0)
                await _database.UpdateAsync(note);
            else
                await _database.InsertAsync(note);
        }
        public async Task<List<Student>> GetStudentsByGroupIdAsync(int groupId)
        {
            await Init();

            var links = await _database.Table<StudentGroup>()
                                       .Where(x => x.GroupId == groupId)
                                       .ToListAsync();

            if (links == null || !links.Any())
                return new List<Student>();


            var studentIds = links.Select(l => l.StudentId).ToList();
            var allStudents = await _database.Table<Student>().ToListAsync();

            return allStudents.Where(s => studentIds.Contains(s.Id)).ToList();
        }
    }
}