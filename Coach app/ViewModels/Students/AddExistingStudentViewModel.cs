using Coach_app.Data.Repositories;
using Coach_app.Models.Domains.Students;
using Coach_app.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Coach_app.ViewModels.Students
{
    [QueryProperty("GroupId", "GroupId")]
    public partial class AddExistingStudentViewModel : ViewModelBase
    {
        private readonly IStudentRepository _repository;

        // Cache contenant TOUS les élèves éligibles (copie invisible pour filtrer)
        private List<Student> _allEligibleStudents = new();

        [ObservableProperty]
        private int _groupId;

        [ObservableProperty]
        private string _searchText;

        // La liste affichée à l'écran
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

        // Déclenché à chaque lettre tapée
        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        [RelayCommand]
        public async Task LoadData()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // 1. On récupère tout le monde
                var allStudents = await _repository.GetAllStudentsAsync();

                // 2. On récupère ceux qui sont DÉJÀ dans le groupe
                var studentsInGroup = await _repository.GetStudentsByGroupAsync(GroupId);
                var existingIds = studentsInGroup.Select(s => s.Id).ToList();

                // 3. On remplit le CACHE avec ceux qui ne sont PAS dans le groupe
                _allEligibleStudents.Clear(); // Important : on vide le cache
                foreach (var s in allStudents)
                {
                    if (!existingIds.Contains(s.Id))
                    {
                        _allEligibleStudents.Add(s);
                    }
                }

                // 4. On met à jour l'affichage
                ApplyFilter();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilter()
        {
            // IMPORTANT : On vide la liste visible pour éviter les doublons
            AvailableStudents.Clear();

            var filtered = _allEligibleStudents.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(s =>
                    s.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var s in filtered)
            {
                AvailableStudents.Add(s);
            }
        }

        [RelayCommand]
        public async Task AddStudentToGroup(Student student)
        {
            if (student == null) return;

            // Ajout en base
            await _repository.AddStudentToGroupAsync(student.Id, GroupId);

            // On le retire des listes pour qu'il disparaisse de l'écran
            _allEligibleStudents.Remove(student);
            AvailableStudents.Remove(student);
        }

        [RelayCommand]
        private async Task Done()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}