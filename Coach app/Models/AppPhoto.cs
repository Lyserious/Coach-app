using SQLite;
using System;

namespace Coach_app.Models
{
    public class AppPhoto
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FilePath { get; set; }
        public DateTime DateTaken { get; set; } = DateTime.Now;
        public string Description { get; set; } // Optionnel
    }
}