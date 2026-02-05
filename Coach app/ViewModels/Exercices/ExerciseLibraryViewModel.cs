using Coach_app.Data.Repositories;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using Coach_app.Views.Exercises;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Coach_app.ViewModels.Exercises
{
    public partial class ExerciseLibraryViewModel : ViewModelBase
    {
        private readonly IExerciseRepository _repository;
        private List<Exercise> _allExercises; // La liste complète en mémoire

        public ObservableCollection<Exercise> Exercises { get; } = new();

        // Pour les filtres
        public ObservableCollection<string> Categories { get; } = new();

        [ObservableProperty]
        private string _selectedCategoryFilter;

        public ExerciseLibraryViewModel(IExerciseRepository repository)
        {
            Title = "Bibliothèque d'Exercices";
            _repository = repository;

            // On remplit les filtres (En dur ou dynamiquement via l'Enum)
            Categories.Add("Tout");
            Categories.Add("Échauffement");
            Categories.Add("Étirement");
            Categories.Add("Manips");
            Categories.Add("Endurance");
            Categories.Add("Force");
            Categories.Add("Technique");
            Categories.Add("Mental");
            Categories.Add("Renfo");
            Categories.Add("Mouvements");
            Categories.Add("Niveau");
            // Tu peux ajouter les autres si tu veux filtrer dessus

            SelectedCategoryFilter = "Tout";
        }

        [RelayCommand]
        public async Task LoadExercises()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                _allExercises = await _repository.GetAllExercisesAsync();
                ApplyFilter();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void FilterByCategory(string category)
        {
            SelectedCategoryFilter = category;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_allExercises == null) return;

            Exercises.Clear();
            var filtered = _allExercises;

            if (SelectedCategoryFilter != "Tout")
            {
                // Filtrage simple sur le nom d'affichage de la catégorie
                filtered = _allExercises.Where(e => e.CategoryDisplay.Contains(SelectedCategoryFilter) || e.CategoryDisplay == SelectedCategoryFilter).ToList();
            }

            foreach (var item in filtered)
                Exercises.Add(item);
        }

        [RelayCommand]
        public async Task AddExercise()
        {
            await Shell.Current.GoToAsync(nameof(ExerciseDetailView));
        }

        [RelayCommand]
        public async Task EditExercise(Exercise exercise)
        {
            if (exercise == null) return;
            await Shell.Current.GoToAsync($"{nameof(ExerciseDetailView)}?Id={exercise.Id}");
        }
    }
}