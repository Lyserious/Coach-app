using Coach_app.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    public interface INoteRepository
    {
        // Récupérer les notes d'un objet précis (ex: L'élève #42)
        Task<List<AppNote>> GetNotesAsync(NoteTargetType type, int targetId);

        // Ajouter ou Modifier
        Task SaveNoteAsync(AppNote note);

        // Supprimer
        Task DeleteNoteAsync(AppNote note);
    }
}