using SQLite;

namespace Coach_app.Models
{
    public class SessionTemplateExercise
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int TemplateId { get; set; }

        [Indexed]
        public int ExerciseId { get; set; }

        public int OrderIndex { get; set; }
        public string Sets { get; set; }
        public string Reps { get; set; }
        public string Weight { get; set; }
        public string Rest { get; set; }
        public string Note { get; set; }
    }
}