using CommunityToolkit.Mvvm.ComponentModel;

namespace Coach_app.Models.Ui
{
    public partial class StudentAttendanceItem : ObservableObject
    {
        public int StudentId { get; set; }
        public int AttendanceId { get; set; }
        public string DisplayName { get; set; }
        public string PhotoPath { get; set; }

        [ObservableProperty] private string _status;
        [ObservableProperty] private string _note;
    }
}