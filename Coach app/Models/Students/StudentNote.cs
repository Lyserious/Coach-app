using SQLite;

namespace Coach_app.Models
{
    public enum NoteType { Text, Photo, Video }

    [Table("StudentNotes")]
    public class StudentNote
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int StudentId { get; set; }

        public NoteType Type { get; set; }

        public string Content { get; set; } // Le texte de la note OU le chemin de la photo
        public string Category { get; set; } // "Mental", "Physique", "Blessure"...

        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}