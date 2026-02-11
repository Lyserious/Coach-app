using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Domains.Training;
using SQLite;

namespace Coach_app.Models
{
    public class SessionExercise
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int GroupSessionId { get; set; }

        [Indexed]
        public int ExerciseId { get; set; }

        public int OrderIndex { get; set; }

        // --- Objectifs (Consignes) ---
        public string Sets { get; set; }    // Ex: "4"
        public string Reps { get; set; }    // Ex: "10" (C'est l'objectif)
        public string Weight { get; set; }
        public string Rest { get; set; }
        public string Note { get; set; }

        // --- Comment on note cet exo ? ---
        public PerformanceType ScoringType { get; set; } = PerformanceType.Numeric;

        [Ignore]
        public Exercise Exercise { get; set; }
    }
}