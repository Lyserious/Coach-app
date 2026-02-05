using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Coach_app.Services.Auth;
using Coach_app.ViewModels.Base;
using Coach_app.Views.Auth;

namespace Coach_app.ViewModels.Home
{
    public partial class DashboardViewModel : ViewModelBase
    {
        private readonly ISessionService _sessionService;

        [ObservableProperty]
        private string _welcomeMessage;

        public DashboardViewModel(ISessionService sessionService)
        {
            _sessionService = sessionService;
            Title = "Accueil";
            LoadData();
        }

        private void LoadData()
        {
            if (_sessionService.CurrentCoach != null)
            {
                WelcomeMessage = $"Bonjour, {_sessionService.CurrentCoach.Name} !";
            }
        }

        [RelayCommand]
        private async Task Logout()
        {
            bool confirm = await Shell.Current.DisplayAlert("Déconnexion", "Voulez-vous vraiment vous déconnecter ?", "Oui", "Non");
            if (confirm)
            {
                _sessionService.ClearSession();
                // Retour au Login via la route absolue
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
        }
    }
}