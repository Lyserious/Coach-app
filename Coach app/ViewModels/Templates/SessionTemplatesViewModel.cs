using Coach_app.Data.Repositories;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using Coach_app.Views.Templates;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Coach_app.ViewModels.Templates
{
    public partial class SessionTemplatesViewModel : ViewModelBase
    {
        private readonly IGroupRepository _repository;

        public ObservableCollection<SessionTemplate> Templates { get; } = new();

        public SessionTemplatesViewModel(IGroupRepository repository)
        {
            _repository = repository;
            Title = "Recueil de Séances";
        }

        [RelayCommand]
        private async Task LoadTemplates()
        {
            IsBusy = true;
            try
            {
                Templates.Clear();
                var list = await _repository.GetAllTemplatesAsync();
                foreach (var t in list) Templates.Add(t);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddTemplate()
        {
            // On va vers la page détail avec un ID=0 pour créer
            await Shell.Current.GoToAsync(nameof(SessionTemplateDetailView));
        }

        [RelayCommand]
        private async Task EditTemplate(SessionTemplate template)
        {
            if (template == null) return;
            await Shell.Current.GoToAsync($"{nameof(SessionTemplateDetailView)}?Id={template.Id}");
        }

        [RelayCommand]
        private async Task DeleteTemplate(SessionTemplate template)
        {
            // Note: Il faudra ajouter DeleteTemplateAsync dans ton Repository si ce n'est pas fait !
            // Pour l'instant on simule ou tu l'ajoutes après.
            bool confirm = await Shell.Current.DisplayAlert("Supprimer", "Supprimer ce modèle ?", "Oui", "Non");
            if (confirm)
            {
                // await _repository.DeleteTemplateAsync(template); // À implémenter
                Templates.Remove(template);
            }
        }
    }
}