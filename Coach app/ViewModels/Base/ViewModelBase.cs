using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input; // <--- AJOUT pour RelayCommand

namespace Coach_app.ViewModels.Base
{
    public partial class ViewModelBase : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title;

        public bool IsNotBusy => !IsBusy;

        // --- CORRECTION : La commande manquante ---
        [RelayCommand]
        private async Task Navigate(string route)
        {
            // Cette méthode est appelée quand on clique sur un bouton de la Sidebar
            if (string.IsNullOrWhiteSpace(route)) return;

            // Navigation via le Shell
            await Shell.Current.GoToAsync(route);
        }
    }
}