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
        private readonly INoteRepository _noteRepository; // 1. AJOUT DU REPO

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

        // 2. CHANGEMENT DE TYPE : StudentNote -> AppNote
        public ObservableCollection<AppNote> Notes { get; } = new();

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

        // 3. CONSTRUCTEUR MIS À JOUR
        public StudentProfileViewModel(IStudentRepository repository, IGroupRepository groupRepository, INoteRepository noteRepository)
        {
            _repository = repository;
            _groupRepository = groupRepository;
            _noteRepository = noteRepository;
        }

        async partial void OnIdChanged(int value)
        {
            if (value > 0)
            {
                // Pas besoin de réassigner _id car 'value' est déjà la nouvelle valeur de Id
                // Mais pour être sûr on garde ta logique
                await LoadData();
            }
        }

        [RelayCommand]
        public async Task LoadData()
        {
            if (Id == 0) return; // Utilisation de la propriété publique Id

            IsBusy = true;
            try
            {
                // 1. Infos Élève
                Student = await _repository.GetStudentByIdAsync(Id);
                if (Student != null) Title = Student.DisplayName;

                // 2. Contact Urgence
                var contacts = await _repository.GetStudentContactsAsync(Id);
                var emergency = contacts.FirstOrDefault();
                EmergencyContactInfo = emergency != null
                    ? $"{emergency.PhoneNumber} ({emergency.FirstName} {emergency.Relation})"
                    : "Aucun contact";

                // 3. Groupes
                var groups = await _repository.GetGroupsByStudentAsync(Id);
                _allStudentGroups = groups;
                ApplyGroupFilter();

                // 4. Notes (NOUVELLE LOGIQUE)
                await LoadNotes();
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --- MÉTHODES POUR LES NOTES (AJOUT) ---

        private async Task LoadNotes()
        {
            var noteList = await _noteRepository.GetNotesAsync(NoteTargetType.Student, Id);
            Notes.Clear();
            foreach (var n in noteList) Notes.Add(n);
        }

        [RelayCommand]
        private async Task AddNote()
        {
            if (Id == 0) return;

            string result = await Shell.Current.DisplayPromptAsync("Nouvelle Note", "Entrez votre observation :", "Ajouter", "Annuler");

            if (!string.IsNullOrWhiteSpace(result))
            {
                var note = new AppNote
                {
                    TargetType = NoteTargetType.Student,
                    TargetId = Id,
                    Content = result,
                    Date = DateTime.Now
                };

                await _noteRepository.SaveNoteAsync(note);
                Notes.Insert(0, note); // Ajoute en haut
            }
        }

        [RelayCommand]
        private async Task DeleteNote(AppNote note)
        {
            if (note == null) return;
            bool confirm = await Shell.Current.DisplayAlert("Supprimer", "Effacer cette note ?", "Oui", "Non");
            if (confirm)
            {
                await _noteRepository.DeleteNoteAsync(note);
                Notes.Remove(note);
            }
        }

        // --- NAVIGATION ---
        [RelayCommand]
        private async Task EditStudent()
        {
            if (Student != null)
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