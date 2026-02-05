using SQLite;

namespace Coach_app.Models
{
    public enum ContactType { Phone, Email, Instagram, Facebook, Other }

    [Table("StudentContacts")]
    public class StudentContact
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int StudentId { get; set; }

        public ContactType Type { get; set; }
        public string Value { get; set; } // Le numéro ou l'identifiant
    }
}