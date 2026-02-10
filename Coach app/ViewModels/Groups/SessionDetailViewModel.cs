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
        private readonly IExerciseRepository _exerciseRepository;

        [ObservableProperty] private int _sessionId;
        [ObservableProperty] private string _groupName;
        [ObservableProperty] private string _dateDisplay;

        // --- NAVIGATION ONGLETS ---
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsProgramVisible))]
        [NotifyPropertyChangedFor(nameof(IsPerformanceVisible))]
        [NotifyPropertyChangedFor(nameof(IsAttendanceVisible))]
        [NotifyPropertyChangedFor(nameof(TabProgramColor))]
        [NotifyPropertyChangedFor(nameof(TabPerfColor))]
        [NotifyPropertyChangedFor(nameof(TabAttendanceColor))]
        private int _currentTab = 0; // 0=Programme, 1=Perfs, 2=Appel

        public bool IsProgramVisible => CurrentTab == 0;
        public bool IsPerformanceVisible => CurrentTab == 1;
        public bool IsAttendanceVisible => CurrentTab == 2;

        public Color TabProgramColor => CurrentTab == 0 ? Colors.Black : Colors.Gray;
        public Color TabPerfColor => CurrentTab == 1 ? Colors.Black : Colors.Gray;
        public Color TabAttendanceColor => CurrentTab == 2 ? Colors.Black : Colors.Gray;

        // --- DONNÉES ---
        public ObservableCollection<SessionExercise> SessionExercises { get; } = new();
        public ObservableCollection<StudentAttendanceItem> AttendanceList { get; } = new();
        public ObservableCollection<StudentPerformanceItem> StudentPerformances { get; } = new();

        [ObservableProperty] private SessionExercise _selectedExerciseForPerf;

        public SessionDetailViewModel(IGroupRepository repo, IStudentRepository studentRepo, IExerciseRepository exoRepo)
        {
            _groupRepository = repo; _studentRepository = studentRepo; _exerciseRepository = exoRepo;
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
                var session = await _groupRepository.GetSessionByIdAsync(SessionId);
                if (session != null)
                {
                    DateDisplay = session.Date.ToString("D", new CultureInfo("fr-FR"));
                    var group = await _groupRepository.GetGroupByIdAsync(session.GroupId);
                    GroupName = group?.Name;

                    SessionExercises.Clear();
                    var exos = await _groupRepository.GetExercisesForSessionAsync(SessionId);
                    foreach (var ex in exos) SessionExercises.Add(ex);

                    await LoadAttendanceData();
                }
            }
            finally { IsBusy = false; }
        }

        private async Task LoadAttendanceData()
        {
            var session = await _groupRepository.GetSessionByIdAsync(SessionId);
            if (session == null) return;

            var students = await _studentRepository.GetStudentsByGroupIdAsync(session.GroupId);
            var existingAttendance = await _groupRepository.GetAttendanceForSessionAsync(SessionId);

            AttendanceList.Clear();
            foreach (var student in students)
            {
                var record = existingAttendance.FirstOrDefault(a => a.StudentId == student.Id);
                string status = record?.Status ?? "Absent";

                AttendanceList.Add(new StudentAttendanceItem
                {
                    StudentId = student.Id,
                    DisplayName = student.DisplayName,
                    PhotoPath = student.ProfilePhotoPath,
                    AttendanceId = record?.Id ?? 0,
                    Status = status,
                    Note = record?.Note
                });
            }
        }

        // --- COMMANDES NAVIGATION ---
        [RelayCommand] private void SetTab(string index) { if (int.TryParse(index, out int i)) CurrentTab = i; }

        // --- LOGIQUE PERFORMANCES ---

        async partial void OnSelectedExerciseForPerfChanged(SessionExercise value)
        {
            if (value == null) return;
            await LoadPerformancesForExercise(value);
        }

        private async Task LoadPerformancesForExercise(SessionExercise exo)
        {
            IsBusy = true;
            try
            {
                var existingPerfs = await _groupRepository.GetPerformancesAsync(SessionId, exo.ExerciseId);
                StudentPerformances.Clear();

                // CORRECTION ICI : On prend tout le monde SAUF "Absent"
                var presentStudents = AttendanceList
                    .Where(a => !string.Equals(a.Status, "Absent", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Si personne n'est présent, la liste reste vide (c'est le comportement voulu)

                var scoringType = exo.Exercise.ScoringType;

                foreach (var s in presentStudents)
                {
                    var p = existingPerfs.FirstOrDefault(x => x.StudentId == s.StudentId && x.SetNumber == 1);
                    var item = new StudentPerformanceItem
                    {
                        StudentId = s.StudentId,
                        Name = s.DisplayName,
                        PhotoPath = s.PhotoPath,
                        PerfId = p?.Id ?? 0,
                        IsNumeric = scoringType == PerformanceType.Numeric,
                        IsCompletion = scoringType == PerformanceType.Completion,
                        IsLevel = scoringType == PerformanceType.Level
                    };

                    if (item.IsCompletion) item.IsCompleted = p?.Value == "true";
                    else item.ValueDisplay = p?.Value ?? "";

                    StudentPerformances.Add(item);
                }
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task SavePerformances()
        {
            if (SelectedExerciseForPerf == null) return;
            IsBusy = true;
            var type = SelectedExerciseForPerf.Exercise.ScoringType;

            foreach (var item in StudentPerformances)
            {
                string val = type == PerformanceType.Completion ? (item.IsCompleted ? "true" : "false") : item.ValueDisplay;
                var perf = new Performance { Id = item.PerfId, GroupSessionId = SessionId, ExerciseId = SelectedExerciseForPerf.ExerciseId, StudentId = item.StudentId, Value = val, SetNumber = 1, Type = type };
                await _groupRepository.SavePerformanceAsync(perf);
                if (item.PerfId == 0) item.PerfId = perf.Id;
            }
            IsBusy = false;
            await Shell.Current.DisplayAlert("Succès", "Sauvegardé !", "OK");
        }

        [RelayCommand]
        private void IncrementScore(StudentPerformanceItem item)
        {
            if (int.TryParse(item.ValueDisplay, out int val)) item.ValueDisplay = (val + 1).ToString();
            else item.ValueDisplay = "1";
        }

        [RelayCommand]
        private void DecrementScore(StudentPerformanceItem item)
        {
            if (int.TryParse(item.ValueDisplay, out int val)) item.ValueDisplay = (val - 1).ToString();
            else item.ValueDisplay = "0";
        }

        // --- APPEL & PROGRAMME ---

        [RelayCommand]
        private void CycleStatus(StudentAttendanceItem item)
        {
            if (item == null) return;
            // Cycle : Absent -> Présent -> Retard -> Absent
            item.Status = item.Status switch
            {
                "Absent" => "Présent",
                "Présent" => "Retard",
                "Retard" => "Absent",
                _ => "Présent" // Si le statut est inconnu (ex: "Present" anglais), on le corrige en "Présent"
            };
        }

        [RelayCommand]
        private async Task SaveAttendance()
        {
            var list = new List<SessionAttendance>();
            foreach (var item in AttendanceList) list.Add(new SessionAttendance { Id = item.AttendanceId, GroupSessionId = SessionId, StudentId = item.StudentId, Status = item.Status, Note = item.Note });
            await _groupRepository.SaveAttendanceListAsync(list);
            await LoadAttendanceData();
            await Shell.Current.DisplayAlert("Succès", "Appel noté", "OK");
        }

        [RelayCommand]
        private async Task AddExercise()
        {
            var all = await _exerciseRepository.GetAllExercisesAsync();
            var names = all.Select(e => e.Name).ToArray();
            string choice = await Shell.Current.DisplayActionSheet("Ajouter", "Annuler", null, names);
            if (!string.IsNullOrEmpty(choice) && choice != "Annuler")
            {
                var sel = all.First(e => e.Name == choice);
                var newLink = new SessionExercise { GroupSessionId = SessionId, ExerciseId = sel.Id, OrderIndex = SessionExercises.Count + 1, Sets = "4", Reps = "10" };
                await _groupRepository.SaveSessionExerciseAsync(newLink); await LoadData();
            }
        }
        [RelayCommand] private async Task DeleteExercise(SessionExercise ex) { if (ex != null) { await _groupRepository.DeleteSessionExerciseAsync(ex); SessionExercises.Remove(ex); } }
        [RelayCommand] private async Task UpdateExercise(SessionExercise ex) { if (ex != null) await _groupRepository.SaveSessionExerciseAsync(ex); }
        [RelayCommand]
        private async Task ImportTemplate()
        {
            var templates = await _groupRepository.GetAllTemplatesAsync(); if (!templates.Any()) return;
            var names = templates.Select(t => t.Name).ToArray(); string choice = await Shell.Current.DisplayActionSheet("Importer", "Annuler", null, names);
            if (!string.IsNullOrEmpty(choice) && choice != "Annuler")
            {
                var tmpl = templates.First(t => t.Name == choice);
                await _groupRepository.ImportTemplateToSessionAsync(tmpl.Id, SessionId); await LoadData();
            }
        }
    }

    public partial class StudentPerformanceItem : ObservableObject
    {
        public int StudentId { get; set; }
        public int PerfId { get; set; }
        public string Name { get; set; }
        public string PhotoPath { get; set; }
        public bool IsNumeric { get; set; }
        public bool IsCompletion { get; set; }
        public bool IsLevel { get; set; }
        [ObservableProperty] private string _valueDisplay; [ObservableProperty] private bool _isCompleted;
    }
    public partial class StudentAttendanceItem : ObservableObject
    {
        public int StudentId { get; set; }
        public int AttendanceId { get; set; }
        public string DisplayName { get; set; }
        public string PhotoPath { get; set; }
        [ObservableProperty] private string _status; [ObservableProperty] private string _note;
    }
}