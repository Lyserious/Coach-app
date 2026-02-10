using SQLite;
using System;

namespace Coach_app.Models
{
    // On définit qui peut avoir des notes
    public enum NoteTargetType
    {
        Student,
        Group,
        Session,
        Program // (SessionTemplate)
    }

    public class AppNote
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Content { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        // "Clé étrangère" générique
        [Indexed]
        public int TargetId { get; set; } // L'ID de l'élève, du groupe, etc.
        public NoteTargetType TargetType { get; set; } // Est-ce un élève ? un groupe ?
    }
}