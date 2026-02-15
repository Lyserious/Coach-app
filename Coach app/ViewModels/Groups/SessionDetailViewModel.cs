// --- ALIAS DE FORCE (Placez ces lignes tout en haut) ---
using ISessionRepository = Coach_app.Data.Repositories.Interfaces.ISessionRepository;
using ITemplateRepository = Coach_app.Data.Repositories.Interfaces.ITemplateRepository;
using IPerformanceRepository = Coach_app.Data.Repositories.Interfaces.IPerformanceRepository;
using IAttendanceRepository = Coach_app.Data.Repositories.Interfaces.IAttendanceRepository;
using IGroupRepository = Coach_app.Data.Repositories.Interfaces.IGroupRepository;
using IExerciseRepository = Coach_app.Data.Repositories.Interfaces.IExerciseRepository;

using Coach_app.Models;
using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Ui;
using Coach_app.Services.Training;
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
        private readonly ISessionRepository _sessionRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ISessionComposer _sessionComposer;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IPerformanceRepository _performanceRepository;
        private readonly ITemplateRepository _templateRepository;

        [ObservableProperty] private int _sessionId;
        [ObservableProperty] private string _groupName;
        [ObservableProperty] private string _dateDisplay;
        [ObservableProperty] private string _sessionDescription;

        // --- NAVIGATION ONGLETS ---
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsProgramVisible))]
        [NotifyPropertyChangedFor(nameof(IsPerformanceVisible))]
        [NotifyPropertyChangedFor(nameof(IsAttendanceVisible))]
        [NotifyPropertyChangedFor(nameof(TabProgramColor))]
        [NotifyPropertyChangedFor(nameof(TabPerfColor))]
        [NotifyPropertyChangedFor(nameof(TabAttendanceColor))]
        private int _currentTab = 0;

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

        public SessionDetailViewModel(
            ISessionRepository sessionRepo,
            IGroupRepository groupRepo,
            IExerciseRepository exoRepo,
            ISessionComposer sessionComposer,
            IAttendanceRepository attendanceRepo,
            IPerformanceRepository perfRepo,
            ITemplateRepository templateRepo)
        {
            _sessionRepository = sessionRepo;
            _groupRepository = groupRepo;
            _exerciseRepository = exoRepo;
            _sessionComposer = sessionComposer;
            _attendanceRepository = attendanceRepo;
            _performanceRepository = perfRepo;
            _templateRepository = templateRepo;
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
                var session = await _sessionRepository.GetSessionByIdAsync(SessionId);
                if (session != null)
                {
                    DateDisplay = session.Date.ToString("D", new CultureInfo("fr-FR"));
                    var group = await _groupRepository.GetGroupByIdAsync(session.GroupId);
                    GroupName = group?.Name;
                    SessionDescription = session.Description;

                    SessionExercises.Clear();
                    var exos = await _sessionRepository.GetExercisesForSessionAsync(SessionId);
                    foreach (var ex in exos) SessionExercises.Add(ex);

                    await LoadAttendanceData(session.GroupId);
                }
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task SaveDescription()
        {
            var session = await _sessionRepository.GetSessionByIdAsync(SessionId);
            if (session != null)
            {
                session.Description = SessionDescription;
                await _sessionRepository.UpdateSessionAsync(session);
                await Shell.Current.DisplayAlert("Succès", "Description mise à jour", "OK");
            }
        }

        private async Task LoadAttendanceData(int groupId)
        {
            var list = await _sessionComposer.GetAttendanceListAsync(SessionId, groupId);
            AttendanceList.Clear();
            foreach (var item in list) AttendanceList.Add(item);
        }

        [RelayCommand] private void SetTab(string index) { if (int.TryParse(index, out int i)) CurrentTab = i; }

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
                var list = await _sessionComposer.GetPerformanceListAsync(exo, AttendanceList.ToList());
                StudentPerformances.Clear();
                foreach (var item in list) StudentPerformances.Add(item);
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

                var perf = new Performance
                {
                    Id = item.PerfId,
                    GroupSessionId = SessionId,
                    ExerciseId = SelectedExerciseForPerf.ExerciseId,
                    SessionExerciseId = SelectedExerciseForPerf.Id,
                    StudentId = item.StudentId,
                    Value = val,
                    SetNumber = 1,
                    Type = type
                };

                await _performanceRepository.SavePerformanceAsync(perf);

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

        [RelayCommand]
        private void CycleStatus(StudentAttendanceItem item)
        {
            if (item == null) return;
            item.Status = _sessionComposer.CycleAttendanceStatus(item.Status);
        }

        [RelayCommand]
        private async Task SaveAttendance()
        {
            var list = new List<SessionAttendance>();
            foreach (var item in AttendanceList)
                list.Add(new SessionAttendance { Id = item.AttendanceId, GroupSessionId = SessionId, StudentId = item.StudentId, Status = item.Status, Note = item.Note });

            await _attendanceRepository.SaveAttendanceListAsync(list);

            var session = await _sessionRepository.GetSessionByIdAsync(SessionId);
            if (session != null) await LoadAttendanceData(session.GroupId);

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
                await _sessionRepository.SaveSessionExerciseAsync(newLink);
                await LoadData();
            }
        }

        [RelayCommand]
        private async Task DeleteExercise(SessionExercise ex)
        {
            if (ex != null) { await _sessionRepository.DeleteSessionExerciseAsync(ex); SessionExercises.Remove(ex); }
        }

        [RelayCommand]
        private async Task UpdateExercise(SessionExercise ex)
        {
            if (ex != null) await _sessionRepository.SaveSessionExerciseAsync(ex);
        }

        [RelayCommand]
        private async Task ImportTemplate()
        {
            try
            {
                var templates = await _templateRepository.GetAllTemplatesAsync();

                if (templates == null || !templates.Any())
                {
                    await Shell.Current.DisplayAlert("Information", "Votre librairie de séances types est vide. Créez d'abord un modèle.", "OK");
                    return;
                }

                var names = templates.Select(t => t.Name).ToArray();
                string choice = await Shell.Current.DisplayActionSheet("Choisir un modèle à importer", "Annuler", null, names);

                if (!string.IsNullOrEmpty(choice) && choice != "Annuler")
                {
                    var tmpl = templates.First(t => t.Name == choice);
                    bool confirm = await Shell.Current.DisplayAlert("Confirmation", $"Voulez-vous copier les exercices de '{tmpl.Name}' dans cette séance ?", "Oui", "Non");

                    if (confirm)
                    {
                        IsBusy = true;
                        await _sessionRepository.ImportTemplateToSessionAsync(tmpl.Id, SessionId, _templateRepository);
                        await LoadData();
                        await Shell.Current.DisplayAlert("Succès", "Exercices importés !", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", $"L'importation a échoué : {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}