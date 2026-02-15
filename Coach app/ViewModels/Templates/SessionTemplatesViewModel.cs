using Coach_app.Data.Repositories.Interfaces; // Namespace Interfaces
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using Coach_app.Views.Templates;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Coach_app.ViewModels.Templates
{
    public partial class SessionTemplatesViewModel : ViewModelBase
    {
        private readonly ITemplateRepository _templateRepository; // Changement de repo

        public ObservableCollection<SessionTemplate> Templates { get; } = new();

        public SessionTemplatesViewModel(ITemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
            Title = "Recueil de Séances";
        }

        [RelayCommand]
        private async Task LoadTemplates()
        {
            IsBusy = true;
            Templates.Clear();
            // Appel via TemplateRepository
            var list = await _templateRepository.GetAllTemplatesAsync();
            foreach (var t in list) Templates.Add(t);
            IsBusy = false;
        }

        [RelayCommand]
        private async Task AddTemplate()
        {
            await Shell.Current.GoToAsync(nameof(SessionTemplateDetailView));
        }

        [RelayCommand]
        private async Task EditTemplate(SessionTemplate tmpl)
        {
            if (tmpl == null) return;
            await Shell.Current.GoToAsync($"{nameof(SessionTemplateDetailView)}?Id={tmpl.Id}");
        }

        [RelayCommand]
        private async Task DeleteTemplate(SessionTemplate tmpl)
        {
            if (tmpl == null) return;
            bool confirm = await Shell.Current.DisplayAlert("Supprimer", "Supprimer définitivement ce modèle ?", "Oui", "Non");
            if (confirm)
            {
                // Suppression via TemplateRepository
                await _templateRepository.DeleteTemplateAsync(tmpl);
                Templates.Remove(tmpl);
            }
        }
    }
}