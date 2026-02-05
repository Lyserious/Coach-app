using Coach_app.Data.Repositories;
using Coach_app.Models;
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

        // Liste des élèves
        public ObservableCollection<Student> Students { get; } = new();

        public GroupDashboardViewModel(IGroupRepository repository, IStudentRepository studentRepository)
        {
            _repository = repository;
            _studentRepository = studentRepository;
        }

        // Cette méthode est appelée automatiquement quand l'ID change via la navigation
        // Assurez-vous qu'elle n'apparaît qu'UNE SEULE FOIS dans ce fichier
        async partial void OnGroupIdChanged(int value)
        {
            await LoadData();
        }

        [RelayCommand]
        public async Task LoadData()
        {
            if (GroupId == 0) return;

            IsBusy = true;
            try
            {
                // 1. Charger le Groupe
                if (CurrentGroup == null)
                {
                    CurrentGroup = await _repository.GetGroupByIdAsync(GroupId);
                    if (CurrentGroup != null)
                    {
                        GroupName = CurrentGroup.Name;
                        Title = CurrentGroup.Name;
                    }
                }

                // 2. Charger les Élèves
                var studentsList = await _studentRepository.GetStudentsByGroupAsync(GroupId);
                Students.Clear();
                foreach (var s in studentsList) Students.Add(s);
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
    }
}