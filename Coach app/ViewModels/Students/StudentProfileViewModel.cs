using Coach_app.Data.Repositories;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using Coach_app.ViewModels.Groups;
using Coach_app.Views.Groups;
using Coach_app.Views.Students;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Coach_app.ViewModels.Students
{
    [QueryProperty(nameof(Id), "Id")]
    public partial class StudentProfileViewModel : ViewModelBase
    {
        private readonly IStudentRepository _repository;
        private readonly IGroupRepository _groupRepository;

        // Toutes les inscriptions de l'élève (Cache)
        private List<Group> _allStudentGroups = new();

        [ObservableProperty] private int _id;
        [ObservableProperty] private Student _student;
        [ObservableProperty] private string _emergencyContactInfo; // Ex: "Maman (06...)"

        // --- GESTION DES ONGLETS ---
        // 0 = Inscriptions, 1 = Photos, 2 = Notes, 3 = Stats
        [ObservableProperty] private int _selectedTabIndex = 0;

        // --- FILTRES INSCRIPTIONS ---
        // 0 = Tout, 1 = Actuelles, 2 = Futures 3= passées
        [ObservableProperty] private int _inscriptionFilterIndex = 0;

        public ObservableCollection<Group> FilteredGroups { get; } = new();
        public ObservableCollection<StudentNote> Notes { get; } = new();

        // Propriétés pour gérer la visibilité des onglets dans la vue
        public bool IsInscriptionsVisible => SelectedTabIndex == 0;
        public bool IsPhotosVisible => SelectedTabIndex == 1;
        public bool IsNotesVisible => SelectedTabIndex == 2;
        public bool IsStatsVisible => SelectedTabIndex == 3;

        // Couleurs des boutons onglets (Visuel)
        public Color Tab0Color => SelectedTabIndex == 0 ? Colors.Black : Colors.Gray;
        public Color Tab1Color => SelectedTabIndex == 1 ? Colors.Black : Colors.Gray;
        public Color Tab2Color => SelectedTabIndex == 2 ? Colors.Black : Colors.Gray;
        public Color Tab3Color => SelectedTabIndex == 3 ? Colors.Black : Colors.Gray;

        // Couleurs des filtres inscriptions
        public Color Filter0Color => InscriptionFilterIndex == 0 ? Color.Parse("#512BD4") : Colors.LightGray; // #512BD4 = Primary
        public Color Filter0Text => InscriptionFilterIndex == 0 ? Colors.White : Colors.Black;

        public Color Filter1Color => InscriptionFilterIndex == 1 ? Color.Parse("#512BD4") : Colors.LightGray;
        public Color Filter1Text => InscriptionFilterIndex == 1 ? Colors.White : Colors.Black;

        public Color Filter2Color => InscriptionFilterIndex == 2 ? Color.Parse("#512BD4") : Colors.LightGray;
        public Color Filter2Text => InscriptionFilterIndex == 2 ? Colors.White : Colors.Black;

        public Color Filter3Color => InscriptionFilterIndex == 3 ? Color.Parse("#512BD4") : Colors.LightGray;
        public Color Filter3Text => InscriptionFilterIndex == 3 ? Colors.White : Colors.Black;

        public StudentProfileViewModel(IStudentRepository repository, IGroupRepository groupRepository)
        {
            _repository = repository;
            _groupRepository = groupRepository;
        }

        async partial void OnIdChanged(int value)
        {
            if (value > 0) await LoadData(value);
        }

        private async Task LoadData(int studentId)
        {
            IsBusy = true;
            try
            {
                // 1. Infos Élève
                Student = await _repository.GetStudentByIdAsync(studentId);
                Title = Student.DisplayName;

                // 2. Contact Urgence (On prend le premier contact marqué comme urgence ou le premier tout court)
                var contacts = await _repository.GetStudentContactsAsync(studentId);
                var emergency = contacts.FirstOrDefault() ?? new StudentContact(); // Simplifié pour l'exemple
                EmergencyContactInfo = emergency.Id != 0
                    ? $"{emergency.PhoneNumber} ({emergency.FirstName} {emergency.LastName})"
                    : "Aucun contact";

                // 3. Groupes (Inscriptions)
                var groups = await _repository.GetGroupsByStudentAsync(studentId);
                _allStudentGroups = groups;
                ApplyGroupFilter();

                // 4. Notes
                var notes = await _repository.GetStudentNotesAsync(studentId);
                Notes.Clear();
                foreach (var n in notes) Notes.Add(n);

            }
            finally
            {
                IsBusy = false;
            }
        }

        // --- COMMANDES NAVIGATION ---

        [RelayCommand]
        private async Task EditStudent()
        {
            // L'écrou renvoie vers la page d'édition existante (StudentDetailView)
            await Shell.Current.GoToAsync($"{nameof(StudentDetailView)}?Id={Student.Id}");
        }

        [RelayCommand]
        private async Task GoToGroup(Group group)
        {
            if (group == null) return;
            await Shell.Current.GoToAsync($"{nameof(GroupDashboardView)}?Id={group.Id}");
        }

        // --- COMMANDES UI (Onglets & Filtres) ---

        [RelayCommand]
        private void SwitchTab(string indexStr)
        {
            if (int.TryParse(indexStr, out int index))
            {
                SelectedTabIndex = index;
                // Notifie la vue que les couleurs et visibilités ont changé
                OnPropertyChanged(nameof(IsInscriptionsVisible));
                OnPropertyChanged(nameof(IsPhotosVisible));
                OnPropertyChanged(nameof(IsNotesVisible));
                OnPropertyChanged(nameof(IsStatsVisible));

                OnPropertyChanged(nameof(Tab0Color));
                OnPropertyChanged(nameof(Tab1Color));
                OnPropertyChanged(nameof(Tab2Color));
                OnPropertyChanged(nameof(Tab3Color));
            }
        }

        [RelayCommand]
        private void FilterInscriptions(string indexStr)
        {
            if (int.TryParse(indexStr, out int index))
            {
                InscriptionFilterIndex = index;
                ApplyGroupFilter();

                // Refresh visual colors
                OnPropertyChanged(nameof(Filter0Color)); OnPropertyChanged(nameof(Filter0Text));
                OnPropertyChanged(nameof(Filter1Color)); OnPropertyChanged(nameof(Filter1Text));
                OnPropertyChanged(nameof(Filter2Color)); OnPropertyChanged(nameof(Filter2Text));
                OnPropertyChanged(nameof(Filter3Color)); OnPropertyChanged(nameof(Filter3Text));
            }
        }

        private void ApplyGroupFilter()
        {
            FilteredGroups.Clear();
            var today = DateTime.Today;
            IEnumerable<Group> result = _allStudentGroups;

            switch (InscriptionFilterIndex)
            {
                case 1: // Actuelles (En cours)
                    result = result.Where(g => g.StartDate <= today && g.EndDate >= today);
                    break;
                case 2: // Futures
                    result = result.Where(g => g.StartDate > today);
                    break;
                case 3: //passées
                    result = result.Where(g => g.StartDate < today);
                    break;
                case 0: // Tout
                default:
                    // Pas de filtre
                    break;
            }

            foreach (var g in result.OrderByDescending(x => x.StartDate))
            {
                FilteredGroups.Add(g);
            }
        }
    }
}