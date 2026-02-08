using SQLite;

namespace Coach_app.Models
{
    public class SessionAttendance
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int GroupSessionId { get; set; } // Lien vers la séance du jour

        [Indexed]
        public int StudentId { get; set; }      // Lien vers l'élève

        public bool IsPresent { get; set; }     // Présent ou Absent

        public string Note { get; set; }        // Ex: "Arrivé en retard", "Blessé"
    }
}