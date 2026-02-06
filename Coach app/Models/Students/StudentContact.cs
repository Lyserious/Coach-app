using SQLite;

namespace Coach_app.Models
{
    [Table("StudentContacts")]
    public class StudentContact
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int StudentId { get; set; }

        // Les nouveaux champs attendus par le ViewModel
        public string FirstName { get; set; }   // Ex: Marie
        public string LastName { get; set; }    // Ex: Dupont
        public string PhoneNumber { get; set; } // Ex: 06 12 34 56 78

        // Optionnel : Pour préciser le lien (Mère, Père, Voisin...)
        public string Relation { get; set; }

        // Propriété calculée utile pour l'affichage (ex: "Marie Dupont")
        [Ignore]
        public string DisplayName => $"{FirstName} {LastName}".Trim();
    }
}