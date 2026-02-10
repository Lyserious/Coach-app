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
            Templates.Clear();
            var list = await _repository.GetAllTemplatesAsync();
            foreach (var t in list) Templates.Add(t);
            IsBusy = false;
        }

        [RelayCommand]
        private async Task AddTemplate()
        {
            // On navigue vers la page détail avec ID=0 (Création)
            await Shell.Current.GoToAsync(nameof(SessionTemplateDetailView));
        }

        [RelayCommand]
        private async Task EditTemplate(SessionTemplate tmpl)
        {
            if (tmpl == null) return;
            // On navigue vers la page détail avec l'ID existant
            await Shell.Current.GoToAsync($"{nameof(SessionTemplateDetailView)}?Id={tmpl.Id}");
        }

        [RelayCommand]
        private async Task DeleteTemplate(SessionTemplate tmpl)
        {
            if (tmpl == null) return;
            bool confirm = await Shell.Current.DisplayAlert("Supprimer", "Supprimer définitivement ce modèle ?", "Oui", "Non");
            if (confirm)
            {
                await _repository.DeleteTemplateAsync(tmpl);
                Templates.Remove(tmpl);
            }
        }
    }
}