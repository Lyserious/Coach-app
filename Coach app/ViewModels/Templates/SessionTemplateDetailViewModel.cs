using Coach_app.Data.Repositories;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Coach_app.ViewModels.Templates
{
    [QueryProperty(nameof(TemplateId), "Id")]
    public partial class SessionTemplateDetailViewModel : ViewModelBase
    {
        private readonly IGroupRepository _repository;
        private readonly IExerciseRepository _exerciseRepository;

        [ObservableProperty] private int _templateId;

        // --- ÉTATS D'AFFICHAGE ---
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsReadMode))]
        private bool _isEditMode;
        public bool IsReadMode => !IsEditMode;

        // --- DONNÉES ---
        [ObservableProperty] private string _name;
        [ObservableProperty] private string _category;
        [ObservableProperty] private string _description;

        public ObservableCollection<TemplateExerciseItem> Exercises { get; } = new();

        public SessionTemplateDetailViewModel(IGroupRepository repository, IExerciseRepository exerciseRepository)
        {
            _repository = repository;
            _exerciseRepository = exerciseRepository;
        }

        async partial void OnTemplateIdChanged(int value)
        {
            if (value > 0)
            {
                // Si on ouvre un modèle existant -> Mode Lecture
                IsEditMode = false;
                await LoadData(value);
            }
            else
            {
                // Création -> Mode Édition direct
                IsEditMode = true;
                Title = "Nouveau Modèle";
                Exercises.Clear();
            }
        }

        private async Task LoadData(int id)
        {
            IsBusy = true;
            try
            {
                var all = await _repository.GetAllTemplatesAsync();
                var t = all.FirstOrDefault(x => x.Id == id);
                if (t != null)
                {
                    Name = t.Name;
                    Category = t.Category;
                    Description = t.Description;
                    Title = t.Name; // Le titre de la page devient le nom de la séance
                }

                var exos = await _repository.GetTemplateExercisesAsync(id);
                var allExos = await _exerciseRepository.GetAllExercisesAsync();

                Exercises.Clear();
                foreach (var ex in exos)
                {
                    var realExo = allExos.FirstOrDefault(e => e.Id == ex.ExerciseId);
                    Exercises.Add(new TemplateExerciseItem
                    {
                        ExerciseId = ex.ExerciseId,
                        Name = realExo?.Name ?? "Exercice inconnu",
                        Sets = ex.Sets,
                        Reps = ex.Reps,
                        Weight = ex.Weight
                    });
                }
            }
            finally { IsBusy = false; }
        }

        // --- ACTIONS ---

        [RelayCommand]
        private void EnableEditMode()
        {
            IsEditMode = true;
            Title = "Modification...";
        }

        [RelayCommand]
        private async Task CancelEdit()
        {
            if (TemplateId == 0)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                IsEditMode = false;
                await LoadData(TemplateId); // On recharge les données d'origine (annulation)
            }
        }

        [RelayCommand]
        private async Task AddExercise()
        {
            var all = await _exerciseRepository.GetAllExercisesAsync();
            var names = all.Select(e => e.Name).ToArray();
            string choice = await Shell.Current.DisplayActionSheet("Ajouter un exercice", "Annuler", null, names);

            if (!string.IsNullOrEmpty(choice) && choice != "Annuler")
            {
                var sel = all.First(e => e.Name == choice);
                Exercises.Add(new TemplateExerciseItem
                {
                    ExerciseId = sel.Id,
                    Name = sel.Name,
                    Sets = "4",
                    Reps = "10"
                });
            }
        }

        [RelayCommand]
        private void RemoveExercise(TemplateExerciseItem item)
        {
            Exercises.Remove(item);
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Erreur", "Le modèle doit avoir un nom !", "OK");
                return;
            }

            var template = new SessionTemplate
            {
                Id = TemplateId,
                Name = Name,
                Category = Category,
                Description = Description
            };

            var listToSave = new List<SessionTemplateExercise>();
            int order = 1;
            foreach (var item in Exercises)
            {
                listToSave.Add(new SessionTemplateExercise
                {
                    TemplateId = TemplateId,
                    ExerciseId = item.ExerciseId,
                    OrderIndex = order++,
                    Sets = item.Sets,
                    Reps = item.Reps,
                    Weight = item.Weight
                });
            }

            await _repository.SaveTemplateAsync(template, listToSave);

            // Si c'était une création, on récupère l'ID (optionnel, mais propre)
            // Pour l'instant on repasse juste en mode lecture
            IsEditMode = false;
            Title = Name;

            // Si ID était 0, il faut recharger la liste ou revenir en arrière pour voir le nouvel ID
            if (TemplateId == 0)
            {
                await Shell.Current.DisplayAlert("Succès", "Séance type créée !", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
    }

    public class TemplateExerciseItem
    {
        public int ExerciseId { get; set; }
        public string Name { get; set; }
        public string Sets { get; set; }
        public string Reps { get; set; }
        public string Weight { get; set; }
    }
}