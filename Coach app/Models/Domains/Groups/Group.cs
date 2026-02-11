using SQLite;
using System;

namespace Coach_app.Models.Domains.Groups
{
    [Table("Groups")]
    public class Group
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } // Ex: "Ggrimpe"

        public GroupType Type { get; set; }

        // --- Gestion de la Récurrence ---

        // Date de début du cycle (ou date unique si stage d'un jour)
        public DateTime StartDate { get; set; }

        // Date de fin du cycle
        public DateTime EndDate { get; set; }

        // Heure de début de la séance (ex: 18h00)
        public TimeSpan StartTime { get; set; }

        // Heure de fin (ex: 20h00)
        public TimeSpan EndTime { get; set; }
        public string PhotoPath { get; set; }= "ecto.png";
        // Jour de récurrence (null si c'est un événement ponctuel sur plusieurs jours consécutifs comme un stage bloqué)
        // 0 = Dimanche, 1 = Lundi, ... 4 = Jeudi
        public DayOfWeek? RecurrenceDay { get; set; }

        // --- Visuel ---
        public string CoverImagePath { get; set; } // Chemin local. Si null -> image par défaut

        // --- Technique ---
        public bool IsArchived { get; set; } // Soft Delete

        public DateTime CreatedAt { get; set; }

        public Group()
        {
            CreatedAt = DateTime.UtcNow;
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(3); // Valeur par défaut
        }

        // Helper pour l'affichage dans la liste (ex: "Tous les jeudis • 18:00 - 20:00")
        [Ignore]
        public string ScheduleDisplay
        {
            get
            {
                // Traduction du Type
                string typeFr = Type switch
                {
                    GroupType.Class => "Cours",
                    GroupType.PrivateLesson => "Cours Particulier",
                    GroupType.Internship => "Stage",
                    GroupType.Training => "Formation",
                    _ => "Autre"
                };

                string timeStr = $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";

                // LOGIQUE D'AFFICHAGE SIMPLIFIÉE
                if (RecurrenceDay.HasValue) // C'est un Cours (Récurrent)
                {
                    // On récupère le nom du jour en français via la culture système
                    var culture = new System.Globalization.CultureInfo("fr-FR");
                    string dayName = culture.DateTimeFormat.GetDayName(RecurrenceDay.Value);
                    // Met la première lettre en majuscule (lundi -> Lundi)
                    dayName = char.ToUpper(dayName[0]) + dayName.Substring(1);

                    return $"{dayName} • {timeStr}"; // Ex: "Mardi • 18:00 - 20:00"
                }
                else // C'est un Stage (Dates précises)
                {
                    return $"Du {StartDate:dd/MM} au {EndDate:dd/MM}";
                }
            }
        }
    }
}