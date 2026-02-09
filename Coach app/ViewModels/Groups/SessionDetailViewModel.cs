using Coach_app.Data.Repositories;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Coach_app.ViewModels.Groups
{
    [QueryProperty(nameof(SessionId), "Id")]
    public partial class SessionDetailViewModel : ViewModelBase
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IStudentRepository _studentRepository;

        [ObservableProperty] private int _sessionId;
        [ObservableProperty] private string _groupName;
        [ObservableProperty] private string _dateDisplay;
        [ObservableProperty] private string _timeDisplay;

        // Contrôle l'affichage de la zone d'appel (plié/déplié)
        [ObservableProperty] private bool _isAttendanceVisible = true;

        public ObservableCollection<StudentAttendanceItem> AttendanceList { get; } = new();

        private GroupSession _currentSession;

        public SessionDetailViewModel(IGroupRepository groupRepository, IStudentRepository studentRepository)
        {
            _groupRepository = groupRepository;
            _studentRepository = studentRepository;
        }

        async partial void OnSessionIdChanged(int value)
        {
            if (value > 0) await LoadData();
        }

        [RelayCommand]
        private void ToggleAttendanceVisibility()
        {
            IsAttendanceVisible = !IsAttendanceVisible;
        }

        [RelayCommand]
        private async Task LoadData()
        {
            IsBusy = true;
            try
            {
                _currentSession = await _groupRepository.GetSessionByIdAsync(SessionId);

                if (_currentSession != null)
                {
                    DateDisplay = _currentSession.Date.ToString("D", new CultureInfo("fr-FR"));
                    TimeDisplay = $"{_currentSession.StartTime:hh\\:mm} - {_currentSession.EndTime:hh\\:mm}";

                    var group = await _groupRepository.GetGroupByIdAsync(_currentSession.GroupId);
                    GroupName = group?.Name ?? "Groupe Inconnu";

                    var students = await _studentRepository.GetStudentsByGroupIdAsync(_currentSession.GroupId);
                    var existingAttendance = await _groupRepository.GetAttendanceForSessionAsync(SessionId);

                    AttendanceList.Clear();
                    foreach (var student in students)
                    {
                        var record = existingAttendance.FirstOrDefault(a => a.StudentId == student.Id);

                        AttendanceList.Add(new StudentAttendanceItem
                        {
                            StudentId = student.Id,
                            DisplayName = student.DisplayName,
                            PhotoPath = student.ProfilePhotoPath,
                            AttendanceId = record?.Id ?? 0,

                           
                            // Si "record?.Status" est null (pas encore noté), on met "Absent" par défaut.
                            Status = record?.Status ?? "Absent",

                            Note = record?.Note
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CycleStatus(StudentAttendanceItem item)
        {
            // LOGIQUE DU CYCLE : Rien -> Présent -> Absent -> En Retard -> Rien
            if (item != null)
            {
                item.Status = item.Status switch
                {
                    "Absent" => "Present",    // 1er clic : Il est là !
                    "Present" => "Late",      // 2ème clic : Il est là mais en retard
                    "Late" => "Absent",       // 3ème clic : Finalement il n'est pas là
                    _ => "Absent"             // Sécurité (ou si c'était null avant)
                };
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            IsBusy = true;
            try
            {
                var listToSave = new List<SessionAttendance>();

                foreach (var item in AttendanceList)
                {
                    listToSave.Add(new SessionAttendance
                    {
                        Id = item.AttendanceId,
                        GroupSessionId = SessionId,
                        StudentId = item.StudentId,
                        Status = item.Status, // On sauvegarde le string
                        Note = item.Note
                    });
                }

                await _groupRepository.SaveAttendanceListAsync(listToSave);
                // On reste sur la page ou on affiche un petit toast, mais on ne quitte pas forcément
                await Shell.Current.DisplayAlert("Succès", "Appel enregistré", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Cancel() => await Shell.Current.GoToAsync("..");
    }

    public partial class StudentAttendanceItem : ObservableObject
    {
        public int StudentId { get; set; }
        public int AttendanceId { get; set; }
        public string DisplayName { get; set; }
        public string PhotoPath { get; set; }

        // C'est cette propriété qui change tout
        [ObservableProperty]
        private string _status;

        [ObservableProperty]
        private string _note;
    }
}