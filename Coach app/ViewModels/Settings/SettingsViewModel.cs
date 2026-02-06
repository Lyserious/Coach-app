using Coach_app.Services.Auth;
using Coach_app.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;

namespace Coach_app.ViewModels.Settings
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly ISessionService _sessionService;

        public SettingsViewModel(ISessionService sessionService)
        {
            _sessionService = sessionService;
            Title = "Paramètres";
        }

        [RelayCommand]
        private async Task Logout()
        {
            // On utilise ta méthode actuelle
            _sessionService.ClearSession();
            await Shell.Current.GoToAsync("//Login");
        }
    }
}