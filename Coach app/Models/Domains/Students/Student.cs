using SQLite;

namespace Coach_app.Models.Domains.Students
{
    [Table("Students")]
    public class Student
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Nickname { get; set; } 


        public string ProfilePhotoPath { get; set; } = "lezardo.png";
        public DateTime BirthDate { get; set; } // Pour l'Age
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
       
        public string PhotoConsent { get; set; } = "Interne uniquement";

        public string MaxLevel { get; set; } = "3";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propriété d'affichage (Propriété calculée)
        [Ignore]
        public string DisplayName => !string.IsNullOrEmpty(Nickname) ? $"{FirstName} \"{Nickname}\" {LastName}" : $"{FirstName} {LastName}";

        [Ignore]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Year;
                if (BirthDate.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}