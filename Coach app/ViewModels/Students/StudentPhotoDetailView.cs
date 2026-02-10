using Coach_app.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer; // Pour le partage

namespace Coach_app.ViewModels.Students
{
    // On écoute le paramètre "Path" passé lors de la navigation
    [QueryProperty(nameof(PhotoPath), "Path")]
    public partial class StudentPhotoDetailViewModel : ViewModelBase
    {
        [ObservableProperty] private string _photoPath;

        [RelayCommand]
        private async Task GoBack()
        {
            // Retour en arrière
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task SharePhoto()
        {
            if (string.IsNullOrEmpty(PhotoPath)) return;

            // Petit bonus : permet de partager/envoyer la photo
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Partager la photo",
                File = new ShareFile(PhotoPath)
            });
        }
    }
}