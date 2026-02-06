using Coach_app.Services.Auth;
using Coach_app.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
            Title = "Tableau de bord";
            UpdateWelcomeMessage();
        }

        private void UpdateWelcomeMessage()
        {
            var coachName = _sessionService.CurrentCoach?.Name ?? "Coach";
            WelcomeMessage = $"Bonjour, {coachName} !";
        }

        [RelayCommand]
        private async Task GoToPage(string route)
        {
            if (!string.IsNullOrEmpty(route))
            {
                await Shell.Current.GoToAsync(route);
            }
        }

        [RelayCommand]
        private async Task Logout()
        {
            // CORRECTION ICI : Utilisation de TA méthode 'ClearSession'
            _sessionService.ClearSession();

            // Redirection vers la page de login
            await Shell.Current.GoToAsync("//Login");
        }
    }
}