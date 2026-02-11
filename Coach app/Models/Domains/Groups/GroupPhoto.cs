using SQLite;

namespace Coach_app.Models.Domains.Groups
{
    public class GroupPhoto
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int GroupId { get; set; }

        public string FilePath { get; set; } // Le chemin local sur le téléphone
        public DateTime DateTaken { get; set; }

        // Pour plus tard (le tagging)
        public string TagsJson { get; set; } // On stockera les IDs des élèves ici plus tard
    }
}