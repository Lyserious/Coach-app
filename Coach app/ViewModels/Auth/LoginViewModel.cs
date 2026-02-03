using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Coach_app.Services.Auth;
using Coach_app.ViewModels.Base;
using System.Threading.Tasks;

namespace Coach_app.ViewModels.Auth
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _password;

        // <--- AJOUT : Champ de confirmation
        [ObservableProperty]
        private string _confirmPassword;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ButtonText))]
        [NotifyPropertyChangedFor(nameof(SwitchModeText))]
        private bool _isRegistering;

        public string ButtonText => IsRegistering ? "Créer mon profil" : "Se connecter";
        public string SwitchModeText => IsRegistering ? "J'ai déjà un compte" : "Créer un nouveau profil coach";

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            Title = "Coach Tracker - Connexion";
            CheckInitialStateAsync();
        }

        private async void CheckInitialStateAsync()
        {
            bool hasCoach = await _authService.HasAnyCoachAsync();
            if (!hasCoach)
            {
                IsRegistering = true;
            }
        }

        [RelayCommand]
        private void ToggleMode()
        {
            IsRegistering = !IsRegistering;
            ErrorMessage = string.Empty;
            ConfirmPassword = string.Empty; // <--- AJOUT : On vide le champ quand on change de mode
        }

        [RelayCommand]
        private async Task Submit()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                
                // On retire les espaces avant/après le nom (ex: "Alex " devient "Alex")
                if (!string.IsNullOrWhiteSpace(Username))
                {
                    Username = Username.Trim();
                }
                

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Veuillez remplir tous les champs.";
                    return;
                }

                if (IsRegistering)
                {
                    // Vérification de la correspondance
                    if (Password != ConfirmPassword)
                    {
                        ErrorMessage = "Les mots de passe ne correspondent pas.";
                        return;
                    }

                    // Si on est bon, on lance la création
                    bool success = await _authService.CreateCoachAsync(Username, Password);
                    if (success)
                    {
                        await LoginInternal();
                    }
                    else
                    {
                        ErrorMessage = "Ce nom de coach existe déjà.";
                    }
                }
                else
                {
                    await LoginInternal();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoginInternal()
        {
            var coach = await _authService.LoginAsync(Username, Password);
            if (coach != null)
            {
                await Shell.Current.DisplayAlert("Succès", $"Bienvenue Coach {coach.Name} !", "Go");
                // Navigation vers le dashboard à venir...
            }
            else
            {
                ErrorMessage = "Nom d'utilisateur ou mot de passe incorrect.";
            }
        }
    }
}