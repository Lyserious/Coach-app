using SQLite;
using System;

namespace Coach_app.Models
{
    [Table("Coaches")]
    public class Coach
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(50), Unique]
        public string Name { get; set; } // Le nom affiché (ex: "Alex")

        public string PasswordHash { get; set; } // Mot de passe crypté (jamais en clair)

        public string Salt { get; set; } // Pour renforcer le hachage

        [MaxLength(100)]
        public string DataFileName { get; set; } // Ex: "coach_alex_123.db3"

        public DateTime CreatedAt { get; set; }

        public Coach()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}