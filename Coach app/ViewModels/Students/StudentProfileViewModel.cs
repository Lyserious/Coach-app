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
        [ObservableProperty] private string _emergencyContactInfo;

        // --- GESTION DES ONGLETS ---
        [ObservableProperty] private int _selectedTabIndex = 0;

        // --- FILTRES INSCRIPTIONS ---
        [ObservableProperty] private int _inscriptionFilterIndex = 0;

        public ObservableCollection<Group> FilteredGroups { get; } = new();
        public ObservableCollection<StudentNote> Notes { get; } = new();

        // Propriétés visuelles
        public bool IsInscriptionsVisible => SelectedTabIndex == 0;
        public bool IsPhotosVisible => SelectedTabIndex == 1;
        public bool IsNotesVisible => SelectedTabIndex == 2;
        public bool IsStatsVisible => SelectedTabIndex == 3;

        public Color Tab0Color => SelectedTabIndex == 0 ? Colors.Black : Colors.Gray;
        public Color Tab1Color => SelectedTabIndex == 1 ? Colors.Black : Colors.Gray;
        public Color Tab2Color => SelectedTabIndex == 2 ? Colors.Black : Colors.Gray;
        public Color Tab3Color => SelectedTabIndex == 3 ? Colors.Black : Colors.Gray;

        public Color Filter0Color => InscriptionFilterIndex == 0 ? Color.Parse("#512BD4") : Colors.LightGray;
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

        // Quand l'ID change (premier chargement)
        async partial void OnIdChanged(int value)
        {
            if (value > 0)
            {
                _id = value;
                await LoadData();
            }
        }

        // Cette méthode est maintenant une commande publique pour le rafraichissement
        [RelayCommand]
        public async Task LoadData()
        {
            if (_id == 0) return;

            IsBusy = true;
            try
            {
                // 1. Infos Élève
                Student = await _repository.GetStudentByIdAsync(_id);
                Title = Student.DisplayName;

                // 2. Contact Urgence
                var contacts = await _repository.GetStudentContactsAsync(_id);
                var emergency = contacts.FirstOrDefault();
                EmergencyContactInfo = emergency != null
                    ? $"{emergency.PhoneNumber} ({emergency.FirstName} {emergency.Relation})"
                    : "Aucun contact";

                // 3. Groupes
                var groups = await _repository.GetGroupsByStudentAsync(_id);
                _allStudentGroups = groups;
                ApplyGroupFilter();

                // 4. Notes
                var notes = await _repository.GetStudentNotesAsync(_id);
                Notes.Clear();
                foreach (var n in notes) Notes.Add(n);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --- NAVIGATION ---
        [RelayCommand]
        private async Task EditStudent()
        {
            // CORRECTION : On remplace "Id" par "StudentId" pour correspondre à ce que StudentDetailViewModel attend
            await Shell.Current.GoToAsync($"{nameof(StudentDetailView)}?StudentId={Student.Id}");
        }

        [RelayCommand]
        private async Task GoToGroup(Group group)
        {
            if (group == null) return;
            await Shell.Current.GoToAsync($"{nameof(GroupDashboardView)}?Id={group.Id}");
        }

        // --- UI ---
        [RelayCommand]
        private void SwitchTab(string indexStr)
        {
            if (int.TryParse(indexStr, out int index))
            {
                SelectedTabIndex = index;
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
                case 1: // Actuelles
                    result = result.Where(g => g.StartDate <= today && g.EndDate >= today);
                    break;
                case 2: // Futures
                    result = result.Where(g => g.StartDate > today);
                    break;
                case 3: // Passées
                    result = result.Where(g => g.StartDate < today);
                    break;
            }

            foreach (var g in result.OrderByDescending(x => x.StartDate))
            {
                FilteredGroups.Add(g);
            }
        }
    }
}