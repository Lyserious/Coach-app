using SQLite;

namespace Coach_app.Models
{
    public class GroupSession
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed] // Pour retrouver vite les séances d'un groupe
        public int GroupId { get; set; }

        public DateTime Date { get; set; }       // La date précise (ex: 14/10/2024)
        public TimeSpan StartTime { get; set; }  // ex: 18:00
        public TimeSpan EndTime { get; set; }    // ex: 20:00

        // "Scheduled" (Prévu), "Cancelled" (Annulé), "Holiday" (Vacances)
        public string Status { get; set; } = "Scheduled";

        // Pour noter le thème de la séance (ex: "Bloc Force", "Technique pieds")
        public string Note { get; set; }
    }
}