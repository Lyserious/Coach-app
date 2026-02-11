using Coach_app.Models.Domains.Groups;
using SQLite;
using System;

namespace Coach_app.Models.Domains.Training
{
    [Table("Exercises")]
    public class Exercise
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } // Ex: "Planche" ou "7a à vue"

        public ExerciseCategory Category { get; set; }

        // --- Tes nouveaux champs ---

        public string Description { get; set; } // Explication générale

        public string Goal { get; set; } // But de l'exercice

        public string Equipment { get; set; } // Équipement nécessaire

        public string Comments { get; set; } // "Rétroversion bassin", astuces coach

        public PerformanceType ScoringType { get; set; } = PerformanceType.Numeric;

        // Lien vers une vidéo YouTube (https://...) ou un fichier local (file://...)
        public string MediaUrl { get; set; }

        // --- Technique ---
        public bool IsArchived { get; set; } // Pour ne pas le perdre si utilisé dans de vieilles séances
        public DateTime CreatedAt { get; set; }

        public Exercise()
        {
            CreatedAt = DateTime.UtcNow;
        }

        // Helper pour afficher le nom de la catégorie en français dans la liste
        [Ignore]
        public string CategoryDisplay
        {
            get
            {
                return Category switch
                {
                    ExerciseCategory.WarmUp => "Échauffement",
                    ExerciseCategory.Stretching => "Étirement",
                    ExerciseCategory.Manips => "Manips",
                    ExerciseCategory.Endurance => "Endurance",
                    ExerciseCategory.Strength => "Force",
                    ExerciseCategory.Technique => "Technique",
                    ExerciseCategory.Mental => "Mental",
                    ExerciseCategory.Core => "Renfo",
                    ExerciseCategory.Moves => "Mouvements",
                    ExerciseCategory.Level => "Niveau Validé",
                    _ => Category.ToString()
                };
            }
        }
    }
}