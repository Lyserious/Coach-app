using Coach_app.Data.Repositories;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using Coach_app.Views.Students;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Coach_app.ViewModels.Students
{
    public partial class StudentLibraryViewModel : ViewModelBase
    {
        private readonly IStudentRepository _repository;

        // Cache pour la recherche
        private List<Student> _allStudents = new();

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private Student _selectedStudent;

        public ObservableCollection<Student> Students { get; } = new();

        public StudentLibraryViewModel(IStudentRepository repository)
        {
            _repository = repository;
            Title = "Annuaire Élèves";
        }

        // Filtre automatique à la frappe
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
                var list = await _repository.GetAllStudentsAsync();

                _allStudents.Clear();
                _allStudents.AddRange(list);

                ApplyFilter();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilter()
        {
            Students.Clear();
            var filtered = _allStudents.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(s =>
                    s.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var s in filtered) Students.Add(s);
        }

        // Clic sur un élève -> Édition
        partial void OnSelectedStudentChanged(Student value)
        {
            if (value != null)
            {
                Shell.Current.GoToAsync($"{nameof(StudentDetailView)}?Id={value.Id}");
                SelectedStudent = null;
            }
        }

        // Bouton + -> Création
        [RelayCommand]
        private async Task AddStudent()
        {
            await Shell.Current.GoToAsync(nameof(StudentDetailView));
        }
    }
}