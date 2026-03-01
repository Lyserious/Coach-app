using Coach_app.Data.Repositories;
using Coach_app.Data.Repositories.Interfaces;
using Coach_app.Models;
using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Domains.Students;
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
    [QueryProperty(nameof(PassedStudent), "Student")] // NOUVEAU : Permet de réceptionner l'élève depuis le Groupe !
    public partial class StudentProfileViewModel : ViewModelBase
    {
        private readonly IStudentRepository _repository;
        private readonly IGroupRepository _groupRepository;
        private readonly INoteRepository _noteRepository;
        private readonly IPhotoRepository _photoRepository;

        private List<Group> _allStudentGroups = new();

        [ObservableProperty] private int _id;
        [ObservableProperty] private Student _student;
        [ObservableProperty] private Student _passedStudent; // La variable qui réceptionne l'élève
        [ObservableProperty] private string _emergencyContactInfo;
        [ObservableProperty] private int _selectedTabIndex = 0;
        [ObservableProperty] private int _inscriptionFilterIndex = 0;

        public ObservableCollection<Group> FilteredGroups { get; } = new();
        public ObservableCollection<AppNote> Notes { get; } = new();
        public ObservableCollection<AppPhoto> Photos { get; } = new();

        public bool IsInscriptionsVisible => SelectedTabIndex == 0;
        public bool IsPhotosVisible => SelectedTabIndex == 1;
        public bool IsNotesVisible => SelectedTabIndex == 2;
        public bool IsStatsVisible => SelectedTabIndex == 3;

        // MISE À JOUR DES COULEURS (Mode Miami Heat : Corail et Blanc/Gris)
        private readonly string BrandOrange = "#FFA502";


        public Color Tab0Color => SelectedTabIndex == 0 ? Color.Parse(BrandOrange) : Colors.White;
        public Color Tab1Color => SelectedTabIndex == 1 ? Color.Parse(BrandOrange) : Colors.White;
        public Color Tab2Color => SelectedTabIndex == 2 ? Color.Parse(BrandOrange) : Colors.White;
        public Color Tab3Color => SelectedTabIndex == 3 ? Color.Parse(BrandOrange) : Colors.White;

        public Color Filter0Color => InscriptionFilterIndex == 0 ? Color.Parse(BrandOrange) : Colors.White;
        public Color Filter0Text => InscriptionFilterIndex == 0 ? Colors.White : Colors.LightGray;
        public Color Filter1Color => InscriptionFilterIndex == 1 ? Color.Parse(BrandOrange) : Colors.White;
        public Color Filter1Text => InscriptionFilterIndex == 1 ? Colors.White : Colors.LightGray;
        public Color Filter2Color => InscriptionFilterIndex == 2 ? Color.Parse(BrandOrange) : Colors.White;
        public Color Filter2Text => InscriptionFilterIndex == 2 ? Colors.White : Colors.LightGray;
        public Color Filter3Color => InscriptionFilterIndex == 3 ? Color.Parse(BrandOrange) : Colors.White;
        public Color Filter3Text => InscriptionFilterIndex == 3 ? Colors.White : Colors.LightGray;

        public StudentProfileViewModel(IStudentRepository repository, IGroupRepository groupRepository, INoteRepository noteRepository, IPhotoRepository photoRepository)
        {
            _repository = repository;
            _groupRepository = groupRepository;
            _noteRepository = noteRepository;
            _photoRepository = photoRepository;
        }

        // NOUVEAU : Se déclenche quand on arrive depuis la page Groupe
        async partial void OnPassedStudentChanged(Student value)
        {
            if (value != null)
            {
                Student = value;
                Id = value.Id; // En assignant l'Id, on déclenche automatiquement le chargement des données !
            }
        }

        async partial void OnIdChanged(int value)
        {
            if (value > 0) await LoadData();
        }

        [RelayCommand]
        public async Task LoadData()
        {
            if (Id == 0) return;
            IsBusy = true;
            try
            {
                // Si l'élève n'a pas été passé directement, on va le chercher dans la BDD
                if (Student == null || Student.Id != Id)
                {
                    Student = await _repository.GetStudentByIdAsync(Id);
                }

                if (Student != null) Title = Student.DisplayName;

                var contacts = await _repository.GetStudentContactsAsync(Id);
                var emergency = contacts.FirstOrDefault();
                EmergencyContactInfo = emergency != null ? $"{emergency.PhoneNumber} ({emergency.FirstName})" : "Aucun contact";

                var groups = await _repository.GetGroupsByStudentAsync(Id);
                _allStudentGroups = groups;
                ApplyGroupFilter();

                await LoadNotes();
                await LoadPhotos();
            }
            finally { IsBusy = false; }
        }

        private async Task LoadNotes()
        {
            var noteList = await _noteRepository.GetNotesAsync(NoteTargetType.Student, Id);
            Notes.Clear();
            foreach (var n in noteList) Notes.Add(n);
        }

        private async Task LoadPhotos()
        {
            var list = await _photoRepository.GetPhotosByStudentAsync(Id);
            Photos.Clear();
            foreach (var p in list) Photos.Add(p);
        }

        [RelayCommand]
        private async Task AddPhoto()
        {
            if (Id == 0) return;

            var results = await FilePicker.Default.PickMultipleAsync(new PickOptions
            {
                PickerTitle = "Sélectionnez vos photos",
                FileTypes = FilePickerFileType.Images
            });

            if (results != null)
            {
                foreach (var file in results)
                {
                    var photo = new AppPhoto
                    {
                        // On essaie les noms les plus probables un par un. 
                        // Si l'un d'eux souligne en rouge, supprime la ligne.

                        FilePath = file.FullPath,
                        DateTaken = DateTime.Now
                    };

                    // ÉTAPE CRUCIALE : On lie l'ID juste avant de sauvegarder
                    // Si l'erreur CS0117 persiste sur StudentId ou TargetId, 
                    // vérifie le nom de la colonne dans ton fichier Models\AppPhoto.cs

                    await _photoRepository.SavePhotoAsync(photo);
                    Photos.Add(photo);
                }
            }
        }

        [RelayCommand]
        private async Task DeletePhoto(AppPhoto photo)
        {
            if (photo == null) return;
            bool confirm = await Shell.Current.DisplayAlert("Supprimer", "Retirer cette photo ?", "Oui", "Non");
            if (confirm)
            {
                await _photoRepository.DeletePhotoAsync(photo);
                Photos.Remove(photo);
            }
        }

        [RelayCommand]
        private async Task AddNote()
        {
            if (Id == 0) return;
            string result = await Shell.Current.DisplayPromptAsync("Nouvelle Note", "Entrez votre observation :", "Ajouter", "Annuler");
            if (!string.IsNullOrWhiteSpace(result))
            {
                var note = new AppNote { TargetType = NoteTargetType.Student, TargetId = Id, Content = result, Date = DateTime.Now };
                await _noteRepository.SaveNoteAsync(note);
                Notes.Insert(0, note);
            }
        }

        [RelayCommand]
        private async Task DeleteNote(AppNote note)
        {
            if (note == null) return;
            bool confirm = await Shell.Current.DisplayAlert("Supprimer", "Effacer cette note ?", "Oui", "Non");
            if (confirm) { await _noteRepository.DeleteNoteAsync(note); Notes.Remove(note); }
        }

        [RelayCommand] private async Task EditStudent() { if (Student != null) await Shell.Current.GoToAsync($"{nameof(StudentDetailView)}?StudentId={Student.Id}"); }
        [RelayCommand] private async Task GoToGroup(Group group) { if (group == null) return; await Shell.Current.GoToAsync($"{nameof(GroupDashboardView)}?Id={group.Id}"); }

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
                OnPropertyChanged(nameof(Filter0Color));
                OnPropertyChanged(nameof(Filter0Text));
                OnPropertyChanged(nameof(Filter1Color));
                OnPropertyChanged(nameof(Filter1Text));
                OnPropertyChanged(nameof(Filter2Color));
                OnPropertyChanged(nameof(Filter2Text));
                OnPropertyChanged(nameof(Filter3Color));
                OnPropertyChanged(nameof(Filter3Text));
            }
        }

        private void ApplyGroupFilter()
        {
            FilteredGroups.Clear();
            var today = DateTime.Today;
            IEnumerable<Group> result = _allStudentGroups;
            switch (InscriptionFilterIndex)
            {
                case 1: result = result.Where(g => g.StartDate <= today && g.EndDate >= today); break;
                case 2: result = result.Where(g => g.StartDate > today); break;
                case 3: result = result.Where(g => g.StartDate < today); break;
            }
            foreach (var g in result.OrderByDescending(x => x.StartDate)) FilteredGroups.Add(g);
        }

        [RelayCommand]
        private async Task OpenPhoto(AppPhoto photo)
        {
            if (photo == null) return;
            await Shell.Current.GoToAsync($"{nameof(StudentPhotoDetailView)}?Path={photo.FilePath}");
        }
    }
}