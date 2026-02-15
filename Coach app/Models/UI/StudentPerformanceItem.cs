using CommunityToolkit.Mvvm.ComponentModel;

namespace Coach_app.Models.Ui
{
    public partial class StudentPerformanceItem : ObservableObject
    {
        public int StudentId { get; set; }
        public int PerfId { get; set; }
        public string Name { get; set; }
        public string PhotoPath { get; set; }

        // Flags pour l'affichage conditionnel
        public bool IsNumeric { get; set; }
        public bool IsCompletion { get; set; }
        public bool IsLevel { get; set; }

        [ObservableProperty] private string _valueDisplay;
        [ObservableProperty] private bool _isCompleted;
    }
}