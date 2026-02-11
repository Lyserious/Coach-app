using SQLite;

namespace Coach_app.Models.Domains.Groups
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