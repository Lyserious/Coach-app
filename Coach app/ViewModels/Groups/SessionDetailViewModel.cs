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

        // La liste des élèves avec leur état (Présent/Absent)
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
        private async Task LoadData()
        {
            IsBusy = true;
            try
            {
                // 1. Charger les infos de la séance
                // (Petite astuce : On n'a pas fait de GetSessionById, on va le faire "à la main" ou l'ajouter au repo plus tard. 
                // Pour l'instant on va supposer qu'on peut récupérer la session via le repo.
                // Si la méthode n'existe pas, on va tricher un peu en récupérant via la date ou le groupe, 
                // MAIS pour faire propre, on va AJOUTER GetSessionByIdAsync dans le Repo juste après).

                // ... En attendant, imaginons qu'on l'a :
                _currentSession = await _groupRepository.GetSessionByIdAsync(SessionId);

                if (_currentSession != null)
                {
                    DateDisplay = _currentSession.Date.ToString("D", new CultureInfo("fr-FR"));
                    TimeDisplay = $"{_currentSession.StartTime:hh\\:mm} - {_currentSession.EndTime:hh\\:mm}";

                    var group = await _groupRepository.GetGroupByIdAsync(_currentSession.GroupId);
                    GroupName = group?.Name ?? "Groupe Inconnu";

                    // 2. Charger les élèves du groupe
                    var students = await _studentRepository.GetStudentsByGroupIdAsync(_currentSession.GroupId);

                    // 3. Charger les présences DÉJÀ enregistrées (s'il y en a)
                    var existingAttendance = await _groupRepository.GetAttendanceForSessionAsync(SessionId);

                    AttendanceList.Clear();
                    foreach (var student in students)
                    {
                        // On cherche si cet élève a déjà une ligne de présence
                        var record = existingAttendance.FirstOrDefault(a => a.StudentId == student.Id);

                        AttendanceList.Add(new StudentAttendanceItem
                        {
                            StudentId = student.Id,
                            DisplayName = student.DisplayName,
                            PhotoPath = student.ProfilePhotoPath,
                            IsPresent = record?.IsPresent ?? false, // Par défaut absent si pas d'info
                            AttendanceId = record?.Id ?? 0,          // 0 si c'est nouveau
                            Note = record?.Note
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur Technique", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void TogglePresence(StudentAttendanceItem item)
        {
            // Inverse l'état (Présent <-> Absent)
            if (item != null)
            {
                item.IsPresent = !item.IsPresent;
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
                    // On ne sauvegarde que ceux qui sont "Présents" ou qui ont déjà une ID (pour mettre à jour en Absent)
                    // Ou plus simple : on sauvegarde tout le monde pour garder l'historique "Absent".
                    listToSave.Add(new SessionAttendance
                    {
                        Id = item.AttendanceId,
                        GroupSessionId = SessionId,
                        StudentId = item.StudentId,
                        IsPresent = item.IsPresent,
                        Note = item.Note
                    });
                }

                await _groupRepository.SaveAttendanceListAsync(listToSave);
                await Shell.Current.DisplayAlert("Succès", "L'appel a été enregistré !", "OK");
                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Cancel() => await Shell.Current.GoToAsync("..");
    }

    // Classe "Wrapper" pour l'affichage
    public partial class StudentAttendanceItem : ObservableObject
    {
        public int StudentId { get; set; }
        public int AttendanceId { get; set; } // ID de la ligne dans la BDD (0 si nouveau)
        public string DisplayName { get; set; }
        public string PhotoPath { get; set; }

        [ObservableProperty]
        private bool _isPresent; // Si on change ça, l'interface se mettra à jour (couleur verte/grise)

        [ObservableProperty]
        private string _note;
    }
}