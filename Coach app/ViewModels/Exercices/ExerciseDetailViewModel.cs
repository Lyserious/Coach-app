
using Coach_app.Data.Repositories;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Coach_app.ViewModels.Exercises
{
    [QueryProperty(nameof(ExerciseId), "Id")]
    public partial class ExerciseDetailViewModel : ViewModelBase
    {
        private readonly IExerciseRepository _repository;

        [ObservableProperty] private int _exerciseId;
        [ObservableProperty] private string _name;
        [ObservableProperty] private ExerciseCategory _selectedCategory;
        [ObservableProperty] private string _description;
        [ObservableProperty] private string _goal;        // But
        [ObservableProperty] private string _equipment;   // Matériel
        [ObservableProperty] private string _comments;    // Commentaires
        [ObservableProperty] private string _mediaUrl;    // Vidéo/Lien

        // Liste pour le Picker
        public List<ExerciseCategory> Categories { get; } = Enum.GetValues(typeof(ExerciseCategory)).Cast<ExerciseCategory>().ToList();

        public ExerciseDetailViewModel(IExerciseRepository repository)
        {
            _repository = repository;
            Title = "Nouvel Exercice";
            SelectedCategory = ExerciseCategory.WarmUp;
        }

        async partial void OnExerciseIdChanged(int value)
        {
            if (value > 0)
            {
                var exo = await _repository.GetExerciseByIdAsync(value);
                if (exo != null)
                {
                    Title = "Modifier l'exercice";
                    Name = exo.Name;
                    SelectedCategory = exo.Category;
                    Description = exo.Description;
                    Goal = exo.Goal;
                    Equipment = exo.Equipment;
                    Comments = exo.Comments;
                    MediaUrl = exo.MediaUrl;
                }
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Erreur", "Le nom est obligatoire", "OK");
                return;
            }

            var exo = new Exercise
            {
                Id = ExerciseId,
                Name = Name,
                Category = SelectedCategory,
                Description = Description,
                Goal = Goal,
                Equipment = Equipment,
                Comments = Comments,
                MediaUrl = MediaUrl
            };

            // Si modif, on garde la date de création
            if (ExerciseId > 0)
            {
                var old = await _repository.GetExerciseByIdAsync(ExerciseId);
                if (old != null) exo.CreatedAt = old.CreatedAt;
            }

            await _repository.SaveExerciseAsync(exo);
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (ExerciseId == 0) return;
            bool confirm = await Shell.Current.DisplayAlert("Archiver", "Voulez-vous archiver cet exercice ?", "Oui", "Non");
            if (confirm)
            {
                await _repository.DeleteExerciseAsync(ExerciseId);
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        private async Task Cancel() => await Shell.Current.GoToAsync("..");
    }
}