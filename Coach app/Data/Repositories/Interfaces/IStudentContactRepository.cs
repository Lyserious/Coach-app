using Coach_app.Models.Domains.Students;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories.Interfaces
{
    public interface IStudentContactRepository
    {
        Task<List<StudentContact>> GetStudentContactsAsync(int studentId);
        Task SaveContactAsync(StudentContact contact);
        Task DeleteContactAsync(int contactId);
        Task DeleteContactsByStudentIdAsync(int studentId);
    }
}