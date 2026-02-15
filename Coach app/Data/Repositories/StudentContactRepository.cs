using Coach_app.Models.Domains.Students;
using Coach_app.Services.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    // ON FORCE L'INTERFACE DU DOSSIER INTERFACES
    public class StudentContactRepository : Coach_app.Data.Repositories.Interfaces.IStudentContactRepository
    {
        private readonly ICoachDatabaseService _dbService;

        public StudentContactRepository(ICoachDatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<List<StudentContact>> GetStudentContactsAsync(int studentId)
        {
            var db = await _dbService.GetConnectionAsync();
            return await db.Table<StudentContact>().Where(c => c.StudentId == studentId).ToListAsync();
        }

        public async Task SaveContactAsync(StudentContact contact)
        {
            var db = await _dbService.GetConnectionAsync();
            if (contact.Id != 0)
                await db.UpdateAsync(contact);
            else
                await db.InsertAsync(contact);
        }

        public async Task DeleteContactAsync(int contactId)
        {
            var db = await _dbService.GetConnectionAsync();
            var contact = await db.Table<StudentContact>().Where(c => c.Id == contactId).FirstOrDefaultAsync();
            if (contact != null) await db.DeleteAsync(contact);
        }

        public async Task DeleteContactsByStudentIdAsync(int studentId)
        {
            var db = await _dbService.GetConnectionAsync();
            var contacts = await db.Table<StudentContact>().Where(x => x.StudentId == studentId).ToListAsync();
            foreach (var c in contacts) await db.DeleteAsync(c);
        }
    }
}