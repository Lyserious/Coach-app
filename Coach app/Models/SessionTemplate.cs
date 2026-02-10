using SQLite;

namespace Coach_app.Models
{
    public class SessionTemplate
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }        // Ex: "Force Max Poutre"
        public string Description { get; set; } // Ex: "À faire en début de cycle"
        public string Category { get; set; }    // Ex: "Force", "Endurance"
    }
}