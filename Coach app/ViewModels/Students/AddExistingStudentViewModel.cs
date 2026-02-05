using Coach_app.Data.Repositories;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Coach_app.ViewModels.Students
{
    [QueryProperty(nameof(GroupId), "GroupId")]
    public partial class AddExistingStudentViewModel : ViewModelBase
    {
        private readonly IStudentRepository _repository;

        [ObservableProperty] private int _groupId;

        // Liste des élèves disponibles (ceux qui ne sont PAS dans le groupe)
        public ObservableCollection<Student> AvailableStudents { get; } = new();

        public AddExistingStudentViewModel(IStudentRepository repository)
        {
            _repository = repository;
            Title = "Importer des élèves";
        }

        async partial void OnGroupIdChanged(int value)
        {
            await LoadData();
        }

        [RelayCommand]
        public async Task LoadData()
        {
            IsBusy = true;
            try
            {
                AvailableStudents.Clear();

                // 1. Récupérer TOUS les élèves de la base
                var allStudents = await _repository.GetAllStudentsAsync();

                // 2. Récupérer ceux DÉJÀ dans le groupe
                var studentsInGroup = await _repository.GetStudentsByGroupAsync(GroupId);
                var existingIds = studentsInGroup.Select(s => s.Id).ToList();

                // 3. Filtrer : On ne garde que ceux qui ne sont pas dans le groupe
                foreach (var s in allStudents)
                {
                    if (!existingIds.Contains(s.Id))
                    {
                        AvailableStudents.Add(s);
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task AddStudentToGroup(Student student)
        {
            if (student == null) return;

            // Ajout immédiat
            await _repository.AddStudentToGroupAsync(student.Id, GroupId);

            // On le retire de la liste visuelle pour montrer qu'il est traité
            AvailableStudents.Remove(student);

            // Petit feedback optionnel (Toast ou juste visuel)
        }

        [RelayCommand]
        private async Task Done()
        {
            // Retour au Dashboard
            await Shell.Current.GoToAsync("..");
        }
    }
}