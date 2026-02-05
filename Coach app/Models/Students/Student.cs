
using SQLite;

namespace Coach_app.Models
{
    [Table("Students")]
    public class Student
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Nickname { get; set; } // Surnom

        public string ProfilePhotoPath { get; set; }

        // Niveau Max (ex: "6a"). Par défaut "3".
        public string MaxLevel { get; set; } = "3";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propriété d'affichage (Propriété calculée)
        [Ignore]
        public string DisplayName => !string.IsNullOrEmpty(Nickname) ? $"{FirstName} \"{Nickname}\" {LastName}" : $"{FirstName} {LastName}";
    }
}