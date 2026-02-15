using Coach_app.Data.Repositories;
using Coach_app.Data.Repositories.Interfaces;
using Coach_app.Models.Domains.Students;
using Coach_app.ViewModels.Base;
using Coach_app.Views.Students;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Coach_app.ViewModels.Students
{
    // 1. On définit les options de tri
    public enum SortOption
    {
        LastName,   // Nom
        FirstName,  // Prénom
        Age,        // Age
        Level,      // Niveau
        Nickname    // Surnom
    }

    public partial class StudentLibraryViewModel : ViewModelBase
    {
        private readonly IStudentRepository _repository;
        private List<Student> _allStudents = new(); // Cache

        // --- PROPRIÉTÉS DE TRI ---
        [ObservableProperty] private string _searchText;

        [ObservableProperty] private SortOption _currentSortOption = SortOption.LastName; // Par défaut : Nom

        [ObservableProperty] private bool _isAscending = true; // Par défaut : A->Z ou Petit->Grand

        // Propriété visuelle pour la flèche (juste pour l'affichage)
        public string SortDirectionIcon => IsAscending ? "⬇️" : "⬆️";

        [ObservableProperty]
        private Student _selectedStudent;

        public ObservableCollection<Student> Students { get; } = new();

        public StudentLibraryViewModel(IStudentRepository repository)
        {
            _repository = repository;
            Title = "Annuaire Élèves";
        }

        // --- COMMANDES ---

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
                ApplyFilterAndSort();
            }
            finally { IsBusy = false; }
        }

        // Commande appelée quand on clique sur un bouton de tri (Nom, Age, etc.)
        [RelayCommand]
        public void SortBy(string optionStr)
        {
            if (Enum.TryParse(optionStr, out SortOption newOption))
            {
                // Si on clique sur le MÊME critère, on inverse l'ordre (Up/Down)
                if (CurrentSortOption == newOption)
                {
                    IsAscending = !IsAscending;
                }
                else
                {
                    // Sinon on change de critère et on remet Ascendant par défaut
                    CurrentSortOption = newOption;
                    IsAscending = true;
                }

                // On met à jour l'icône et la liste
                OnPropertyChanged(nameof(SortDirectionIcon));
                ApplyFilterAndSort();
            }
        }

        // Commande pour changer juste la direction (Bouton flèche)
        [RelayCommand]
        public void ToggleSortDirection()
        {
            IsAscending = !IsAscending;
            OnPropertyChanged(nameof(SortDirectionIcon));
            ApplyFilterAndSort();
        }

        partial void OnSearchTextChanged(string value) => ApplyFilterAndSort();

        // --- LOGIQUE DE TRI ---

        private void ApplyFilterAndSort()
        {
            var filtered = _allStudents.AsEnumerable();

            // 1. FILTRAGE (Recherche)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(s =>
                    (s.LastName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.FirstName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.Nickname?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                );
            }

            // 2. TRI (Sorting)
            filtered = CurrentSortOption switch
            {
                SortOption.LastName => IsAscending ? filtered.OrderBy(s => s.LastName) : filtered.OrderByDescending(s => s.LastName),
                SortOption.FirstName => IsAscending ? filtered.OrderBy(s => s.FirstName) : filtered.OrderByDescending(s => s.FirstName),
                SortOption.Age => IsAscending ? filtered.OrderByDescending(s => s.BirthDate) : filtered.OrderBy(s => s.BirthDate), // Attention : DateNaissance Desc = Age Asc (plus jeune)
                SortOption.Level => IsAscending ? filtered.OrderBy(s => s.MaxLevel) : filtered.OrderByDescending(s => s.MaxLevel),
                SortOption.Nickname => IsAscending ? filtered.OrderBy(s => s.Nickname) : filtered.OrderByDescending(s => s.Nickname),
                _ => filtered
            };

            // 3. MISE À JOUR LISTE
            Students.Clear();
            foreach (var s in filtered) Students.Add(s);
        }

        partial void OnSelectedStudentChanged(Student value)
        {
            if (value != null)
            {

                Shell.Current.GoToAsync($"{nameof(StudentProfileView)}?Id={value.Id}");

                SelectedStudent = null;
            }
        }

        [RelayCommand]
        private async Task AddStudent() => await Shell.Current.GoToAsync(nameof(StudentDetailView));
    }
}