using SQLite;

namespace Coach_app.Models
{
    public class SessionAttendance
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int GroupSessionId { get; set; }

        [Indexed]
        public int StudentId { get; set; }
      
        public string Status { get; set; }

        public string Note { get; set; }
    }
}