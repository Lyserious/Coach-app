using SQLite;

namespace Coach_app.Models
{
    // Table de liaison : Un élève peut appartenir à plusieurs groupes
    [Table("StudentGroups")]
    public class StudentGroup
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int StudentId { get; set; }

        [Indexed]
        public int GroupId { get; set; }
    }
}