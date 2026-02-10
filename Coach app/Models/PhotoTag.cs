using SQLite;

namespace Coach_app.Models
{
    // Table de liaison : Une photo peut avoir plusieurs élèves, un élève plusieurs photos.
    public class PhotoTag
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int PhotoId { get; set; }

        [Indexed]
        public int StudentId { get; set; }
    }
}