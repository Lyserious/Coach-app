using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Domains.Students;

namespace Coach_app.Data.Repositories.Interfaces
{
    public interface IStudentRepository
    {
        // --- CRUD ÉLÈVE ---
        Task<List<Student>> GetAllStudentsAsync();
        Task<Student> GetStudentByIdAsync(int id);

        // Retourne l'ID (int) pour confirmer la sauvegarde
        Task<int> SaveStudentAsync(Student student);

        // La méthode manquante qui provoquait l'erreur
        Task DeleteStudentAsync(Student student);

        // --- GESTION GROUPES ---
        Task<List<Student>> GetStudentsByGroupAsync(int groupId);
        Task<List<Group>> GetGroupsByStudentAsync(int studentId); // Indispensable pour le Profil
        Task AddStudentToGroupAsync(int studentId, int groupId);
        Task RemoveStudentFromGroupAsync(int studentId, int groupId);

        // --- GESTION CONTACTS ---
        // On garde le nom explicite "GetStudentContactsAsync" pour le ViewModel
        Task<List<StudentContact>> GetStudentContactsAsync(int studentId);
        Task SaveContactAsync(StudentContact contact);
        Task DeleteContactAsync(int contactId);

        // --- GESTION NOTES ---
        // On garde le nom explicite "GetStudentNotesAsync" pour le ViewModel
        Task<List<StudentNote>> GetStudentNotesAsync(int studentId);
        Task SaveNoteAsync(StudentNote note);


        Task<List<Student>> GetStudentsByGroupIdAsync(int groupId);
    }
}