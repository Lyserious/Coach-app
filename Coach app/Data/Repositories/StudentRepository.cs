using Coach_app.Configurations;
using Coach_app.Models;
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

        // --- ÉLÈVES ---

        public async Task<List<Student>> GetStudentsByGroupAsync(int groupId)
        {
            await Init();
            var query = "SELECT S.* FROM Students S INNER JOIN StudentGroups SG ON S.Id = SG.StudentId WHERE SG.GroupId = ?";
            return await _database.QueryAsync<Student>(query, groupId);
        }

        public async Task<Student> GetStudentByIdAsync(int studentId)
        {
            await Init();
            return await _database.Table<Student>().Where(s => s.Id == studentId).FirstOrDefaultAsync();
        }

        public async Task<int> SaveStudentAsync(Student student)
        {
            await Init();
            if (student.Id != 0)
            {
                await _database.UpdateAsync(student);
                return student.Id; // On retourne l'ID existant
            }
            else
            {
                await _database.InsertAsync(student);
                // CORRECTION ICI : SQLite met à jour l'ID dans l'objet 'student' après l'insertion.
                // On retourne cet ID (ex: 55) et non le nombre de lignes (1).
                return student.Id;
            }
        }

        // --- MÉTHODE POUR L'IMPORT (On en aura besoin juste après) ---
        public async Task<List<Student>> GetAllStudentsAsync()
        {
            await Init();
            // Retourne tous les élèves de la base, triés par nom
            return await _database.Table<Student>().OrderBy(s => s.LastName).ToListAsync();
        }

        // --- GROUPES ---

        public async Task AddStudentToGroupAsync(int studentId, int groupId)
        {
            await Init();
            var existing = await _database.Table<StudentGroup>()
                .Where(x => x.StudentId == studentId && x.GroupId == groupId)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                await _database.InsertAsync(new StudentGroup { StudentId = studentId, GroupId = groupId });
            }
        }

        public async Task RemoveStudentFromGroupAsync(int studentId, int groupId)
        {
            await Init();
            var link = await _database.Table<StudentGroup>()
                .Where(x => x.StudentId == studentId && x.GroupId == groupId)
                .FirstOrDefaultAsync();

            if (link != null) await _database.DeleteAsync(link);
        }

        // --- CONTACTS ---

        public async Task<List<StudentContact>> GetContactsAsync(int studentId)
        {
            await Init();
            return await _database.Table<StudentContact>().Where(c => c.StudentId == studentId).ToListAsync();
        }

        public async Task SaveContactAsync(StudentContact contact)
        {
            await Init();
            if (contact.Id != 0) await _database.UpdateAsync(contact);
            else await _database.InsertAsync(contact);
        }

        public async Task DeleteContactAsync(int contactId)
        {
            await Init();
            await _database.DeleteAsync<StudentContact>(contactId);
        }

        // --- NOTES ---

        public async Task<List<StudentNote>> GetNotesAsync(int studentId)
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
            if (note.Id != 0) await _database.UpdateAsync(note);
            else await _database.InsertAsync(note);
        }
    }
}