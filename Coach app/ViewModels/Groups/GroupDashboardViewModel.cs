using Coach_app.Data.Repositories;
using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Domains.Students;
using Coach_app.ViewModels.Base;
using Coach_app.ViewModels.Students;
using Coach_app.Views.Groups;
using Coach_app.Views.Students;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Coach_app.ViewModels.Groups
{
    [QueryProperty(nameof(GroupId), "Id")]
    public partial class GroupDashboardViewModel : ViewModelBase
    {
        private readonly IGroupRepository _repository;
        private readonly IStudentRepository _studentRepository;

        [ObservableProperty]
        private int _groupId;

        [ObservableProperty]
        private Group _currentGroup;

        [ObservableProperty]
        private string _groupName;

        // AJOUT : La propriété nécessaire pour l'affichage de la photo dans l'en-tête
        [ObservableProperty]
        private string _photoPath;

        // Liste des élèves
        public ObservableCollection<Student> Students { get; } = new();

        public GroupDashboardViewModel(IGroupRepository repository, IStudentRepository studentRepository)
        {
            _repository = repository;
            _studentRepository = studentRepository;
        }

        async partial void OnGroupIdChanged(int value)
        {
            if (value > 0) await LoadData();
        }

        [RelayCommand]
        public async Task LoadData()
        {
            if (GroupId == 0) return;

            IsBusy = true;
            try
            {
                // 1. Charger le Groupe
                // IMPORTANT : J'ai retiré le "if (CurrentGroup == null)" pour forcer 
                // la mise à jour si on vient de modifier la photo ou le nom.
                var group = await _repository.GetGroupByIdAsync(GroupId);

                if (group != null)
                {
                    CurrentGroup = group;
                    GroupName = group.Name;
                    Title = group.Name;

                    // AJOUT : On charge le chemin de la photo pour la vue
                    PhotoPath = group.PhotoPath;
                }

                // 2. Charger les Élèves
                // Note: Assure-toi que GetStudentsByGroupAsync existe bien dans ton repo (ou GetStudentsByGroupIdAsync)
                var studentsList = await _studentRepository.GetStudentsByGroupAsync(GroupId);

                Students.Clear();
                foreach (var s in studentsList)
                {
                    Students.Add(s);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddStudent()
        {
            // On va vers la création d'élève en donnant l'ID du groupe actuel
            await Shell.Current.GoToAsync($"{nameof(StudentDetailView)}?GroupId={GroupId}");
        }

        [RelayCommand]
        private async Task EditGroup()
        {
            if (CurrentGroup != null)
            {
                await Shell.Current.GoToAsync($"{nameof(GroupDetailView)}?Id={CurrentGroup.Id}");
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task ImportStudent()
        {
            // Navigation vers la page d'import
            await Shell.Current.GoToAsync($"{nameof(AddExistingStudentView)}?GroupId={GroupId}");
        }
        [RelayCommand]
        private async Task GoToCalendar()
        {
            // On navigue vers la nouvelle page de gestion
            await Shell.Current.GoToAsync($"{nameof(GroupSessionsView)}?Id={GroupId}");
        }
        [RelayCommand]
        private async Task GoToGallery()
        {
            await Shell.Current.GoToAsync($"{nameof(GroupGalleryView)}?Id={GroupId}");
        }
    }
}