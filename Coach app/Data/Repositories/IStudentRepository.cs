using Coach_app.Models;

namespace Coach_app.Data.Repositories
{
    public interface IStudentRepository
    {
        // CRUD Élève
        Task<List<Student>> GetStudentsByGroupAsync(int groupId);
        Task<List<Student>> GetAllStudentsAsync(); // <--- AJOUT
        Task<Student> GetStudentByIdAsync(int studentId);
        Task<int> SaveStudentAsync(Student student);

        // Gestion Groupes
        Task AddStudentToGroupAsync(int studentId, int groupId);
        Task RemoveStudentFromGroupAsync(int studentId, int groupId);

        // Gestion Contacts
        Task<List<StudentContact>> GetContactsAsync(int studentId);
        Task SaveContactAsync(StudentContact contact);
        Task DeleteContactAsync(int contactId);

        // Gestion Notes/Photos
        Task<List<StudentNote>> GetNotesAsync(int studentId);
        Task SaveNoteAsync(StudentNote note);
    }
}