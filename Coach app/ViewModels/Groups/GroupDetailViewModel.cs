using Coach_app.Data.Repositories;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;

namespace Coach_app.ViewModels.Groups
{
    // Cette annotation permet de recevoir l'ID lors de la navigation
    [QueryProperty(nameof(GroupId), "Id")]
    public partial class GroupDetailViewModel : ViewModelBase
    {
        private readonly IGroupRepository _repository;

        // --- PROPRIETES ---

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEditMode))]
        [NotifyPropertyChangedFor(nameof(SubmitButtonText))] 
        [NotifyPropertyChangedFor(nameof(TitleText))]
        private int _groupId;

        public bool IsEditMode => GroupId > 0;

        public string SubmitButtonText => IsEditMode ? "Mettre à jour" : "Créer le groupe";

        public string TitleText => IsEditMode ? "Modifier le groupe" : "Nouveau Groupe";





        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsWeekly))]
        [NotifyPropertyChangedFor(nameof(IsPrivate))]
        [NotifyPropertyChangedFor(nameof(IsEvent))]
        private int _selectedTypeIndex;

        [ObservableProperty]
        private int _selectedDayIndex; // 0=Lundi

        [ObservableProperty]
        private DateTime _startDate;

        [ObservableProperty]
        private DateTime _endDate;

        [ObservableProperty]
        private TimeSpan _startTime;

        [ObservableProperty]
        private TimeSpan _endTime;

        // --- LISTES ---
        public List<string> GroupTypeNames { get; } = new List<string>
        {
            "Cours Hebdomadaire",   // 0
            "Cours Particulier",    // 1
            "Stage / Événement",    // 2
            "Formation"             // 3
        };

        public List<string> DayNames { get; }

        // --- LOGIQUE D'AFFICHAGE ---
        public bool IsWeekly => SelectedTypeIndex == 0;
        public bool IsPrivate => SelectedTypeIndex == 1;
        public bool IsEvent => SelectedTypeIndex == 2 || SelectedTypeIndex == 3;

        // --- CONSTRUCTEUR ---
        public GroupDetailViewModel(IGroupRepository repository)
        {
            _repository = repository;
            Title = "Nouveau Groupe";

            // Génération Jours (Lundi..Dimanche)
            var culture = new CultureInfo("fr-FR");
            DayNames = culture.DateTimeFormat.DayNames
                .Skip(1)
                .Concat(culture.DateTimeFormat.DayNames.Take(1))
                .Select(d => char.ToUpper(d[0]) + d.Substring(1))
                .ToList();

            // Valeurs par défaut
            SelectedTypeIndex = 0;
            SelectedDayIndex = 0;
            StartTime = new TimeSpan(18, 0, 0);
            EndTime = new TimeSpan(20, 0, 0);
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(6);
        }

        // --- CHARGEMENT ---
        async partial void OnGroupIdChanged(int value)
        {
            if (value > 0)
            {
                IsBusy = true;
                try
                {
                    var group = await _repository.GetGroupByIdAsync(value);
                    if (group != null)
                    {
                        Title = "Modifier le Groupe";
                        Name = group.Name;

                        SelectedTypeIndex = group.Type switch
                        {
                            GroupType.Class => 0,
                            GroupType.PrivateLesson => 1,
                            GroupType.Internship => 2,
                            _ => 3
                        };

                        StartDate = group.StartDate;
                        EndDate = group.EndDate;
                        StartTime = group.StartTime;
                        EndTime = group.EndTime;

                        if (group.RecurrenceDay.HasValue)
                        {
                            int dayIndex = ((int)group.RecurrenceDay.Value - 1);
                            if (dayIndex < 0) dayIndex = 6;
                            SelectedDayIndex = dayIndex;
                        }
                    }
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        // --- COMMANDES ---

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Erreur", "Le nom est obligatoire", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var type = SelectedTypeIndex switch
                {
                    0 => GroupType.Class,
                    1 => GroupType.PrivateLesson,
                    2 => GroupType.Internship,
                    _ => GroupType.Training
                };

                DayOfWeek? recurrenceDay = null;
                if (IsWeekly)
                {
                    recurrenceDay = (DayOfWeek)((SelectedDayIndex + 1) % 7);
                }

                var groupToSave = new Group
                {
                    Id = GroupId,
                    Name = Name,
                    Type = type,
                    RecurrenceDay = recurrenceDay,
                    StartDate = IsPrivate ? DateTime.Today : StartDate,
                    EndDate = IsPrivate ? DateTime.Today.AddYears(1) : EndDate,
                    StartTime = IsPrivate ? TimeSpan.Zero : StartTime,
                    EndTime = IsPrivate ? TimeSpan.Zero : EndTime
                };

                if (GroupId > 0)
                {
                    var old = await _repository.GetGroupByIdAsync(GroupId);
                    if (old != null)
                    {
                        groupToSave.CoverImagePath = old.CoverImagePath;
                        groupToSave.CreatedAt = old.CreatedAt;
                    }
                }

                await _repository.SaveGroupAsync(groupToSave);
                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (GroupId == 0) return;
            bool confirm = await Shell.Current.DisplayAlert("Archiver", "Voulez-vous archiver ce groupe ?", "Oui", "Non");
            if (confirm)
            {
                await _repository.DeleteGroupAsync(GroupId);
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        private async Task Cancel() => await Shell.Current.GoToAsync("..");
    }
}