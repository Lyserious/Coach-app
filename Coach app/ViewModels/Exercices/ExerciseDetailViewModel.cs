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

        // --- ÉTATS D'AFFICHAGE ---
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsReadMode))]
        private bool _isEditMode;

        public bool IsReadMode => !IsEditMode;

        // --- DONNÉES ---
        [ObservableProperty] private int _exerciseId;
        [ObservableProperty] private string _name;
        [ObservableProperty] private ExerciseCategory _selectedCategory;
        [ObservableProperty] private string _description;
        [ObservableProperty] private string _goal;
        [ObservableProperty] private string _equipment;
        [ObservableProperty] private string _comments;

        // AJOUT ICI : On notifie HasMedia quand l'URL change
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasMedia))]
        private string _mediaUrl;

        // Propriété calculée pour savoir si on affiche le bouton vidéo
        public bool HasMedia => !string.IsNullOrWhiteSpace(MediaUrl);

        public List<ExerciseCategory> Categories { get; } = Enum.GetValues(typeof(ExerciseCategory)).Cast<ExerciseCategory>().ToList();

        public ExerciseDetailViewModel(IExerciseRepository repository)
        {
            _repository = repository;
            Title = "Détail Exercice";
        }

        async partial void OnExerciseIdChanged(int value)
        {
            if (value > 0)
            {
                IsEditMode = false; // Mode Lecture par défaut

                var exo = await _repository.GetExerciseByIdAsync(value);
                if (exo != null)
                {
                    Name = exo.Name;
                    SelectedCategory = exo.Category;
                    Description = exo.Description;
                    Goal = exo.Goal;
                    Equipment = exo.Equipment;
                    Comments = exo.Comments;
                    MediaUrl = exo.MediaUrl;
                }
            }
            else
            {
                IsEditMode = true; // Création -> Mode Édition
                Title = "Créer un exercice";
                SelectedCategory = ExerciseCategory.WarmUp;
            }
        }

        // --- COMMANDES ---

        [RelayCommand]
        private async Task OpenMedia()
        {
            if (string.IsNullOrWhiteSpace(MediaUrl)) return;

            try
            {
                // Ouvre le lien dans le navigateur par défaut ou l'app YouTube
                await Launcher.Default.OpenAsync(new Uri(MediaUrl));
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlert("Erreur", "Impossible d'ouvrir ce lien.", "OK");
            }
        }

        [RelayCommand]
        private void EnableEditMode()
        {
            IsEditMode = true;
            Title = "Modification...";
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

            if (ExerciseId > 0)
            {
                var old = await _repository.GetExerciseByIdAsync(ExerciseId);
                if (old != null) exo.CreatedAt = old.CreatedAt;
            }

            await _repository.SaveExerciseAsync(exo);

            // Retour en mode lecture pour voir le résultat
            IsEditMode = false;
            Title = "Détail Exercice";
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
        private async Task Cancel()
        {
            if (ExerciseId == 0)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                IsEditMode = false;
                OnExerciseIdChanged(ExerciseId); // Recharge les données originales
            }
        }
    }
}