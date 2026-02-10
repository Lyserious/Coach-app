using SQLite;

namespace Coach_app.Models
{
    // Type de résultat possible pour un exercice
    public enum PerformanceType
    {
        Completion, // Case à cocher (Fait / Pas fait)
        Numeric,    // Nombre (Reps, Poids, Temps)
        Level       // Niveau (Validé, Aide, Echec...)
    }

    public class Performance
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int GroupSessionId { get; set; } // Lié à la séance

        [Indexed]
        public int StudentId { get; set; }      // Lié à l'élève

        [Indexed]
        public int ExerciseId { get; set; }     // Lié à l'exercice réalisé

        // Le résultat
        public string Value { get; set; }       // Stocke "true", "12", "5a", etc.
        public string Note { get; set; }        // Commentaire spécifique (ex: "Facile")

        // Pour savoir comment lire la "Value"
        public PerformanceType Type { get; set; }

        // Numéro de la série (si on fait l'exo 3 fois)
        public int SetNumber { get; set; }
    }
}