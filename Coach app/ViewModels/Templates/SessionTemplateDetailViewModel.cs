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
        [ObservableProperty] private string _name;
        [ObservableProperty] private string _category;
        [ObservableProperty] private string _description;

        public ObservableCollection<SessionTemplateExercise> Exercises { get; } = new();

        public SessionTemplateDetailViewModel(IGroupRepository repository, IExerciseRepository exerciseRepository)
        {
            _repository = repository;
            _exerciseRepository = exerciseRepository;
            Title = "Nouvelle Séance Type";
        }

        async partial void OnTemplateIdChanged(int value)
        {
            if (value > 0) await LoadData();
        }

        private async Task LoadData()
        {
            IsBusy = true;
            // Note: Il faudra ajouter GetTemplateByIdAsync et GetExercisesForTemplateAsync dans le Repository
            // Pour l'instant, on suppose que SaveTemplateAsync gère tout.
            // ... (Implémentation simplifiée pour l'exemple, voir étape Repository)

            // Simulation chargement si tu n'as pas encore créé les méthodes Get
            // var t = await _repository.GetTemplateByIdAsync(TemplateId); 
            IsBusy = false;
        }

        [RelayCommand]
        private async Task AddExercise()
        {
            var allExercises = await _exerciseRepository.GetAllExercisesAsync();
            var names = allExercises.Select(e => e.Name).ToArray();
            string choice = await Shell.Current.DisplayActionSheet("Ajouter un exercice", "Annuler", null, names);

            if (!string.IsNullOrEmpty(choice) && choice != "Annuler")
            {
                var exo = allExercises.First(e => e.Name == choice);
                Exercises.Add(new SessionTemplateExercise
                {
                    ExerciseId = exo.Id,
                    OrderIndex = Exercises.Count + 1,
                    Sets = "4",
                    Reps = "10"
                    // Astuce: On n'a pas l'objet "Exercise" complet ici pour l'affichage, 
                    // il faudra peut-être une propriété temporaire "ExerciseName" dans SessionTemplateExercise
                    // ou faire une classe wrapper comme pour l'Appel.
                });
            }
        }

        [RelayCommand]
        private void RemoveExercise(SessionTemplateExercise item)
        {
            Exercises.Remove(item);
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Name)) return;

            var template = new SessionTemplate
            {
                Id = TemplateId,
                Name = Name,
                Category = Category,
                Description = Description
            };

            await _repository.SaveTemplateAsync(template, Exercises.ToList());
            await Shell.Current.GoToAsync("..");
        }
    }
}