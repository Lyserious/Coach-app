using SQLite;

namespace Coach_app.Models
{
    public enum PerformanceType
    {
        Completion,
        Numeric,
        Level
    }

    public class Performance
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int GroupSessionId { get; set; }

        [Indexed]
        public int StudentId { get; set; }

        [Indexed]
        public int ExerciseId { get; set; }

        
        [Indexed]
        public int SessionExerciseId { get; set; }

        public string Value { get; set; }
        public string Note { get; set; }

        public PerformanceType Type { get; set; }
        public int SetNumber { get; set; }
    }
}