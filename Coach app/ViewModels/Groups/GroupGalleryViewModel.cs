using Coach_app.Data.Repositories;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using Coach_app.Views.Groups; 
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;


namespace Coach_app.ViewModels.Groups
{
    [QueryProperty(nameof(GroupId), "Id")]
    public partial class GroupGalleryViewModel : ViewModelBase
    {
        private readonly IGroupRepository _repository;

        [ObservableProperty] private int _groupId;

        // Liste des photos affichées à l'écran
        public ObservableCollection<GroupPhoto> Photos { get; } = new();

        public GroupGalleryViewModel(IGroupRepository repository)
        {
            _repository = repository;
            Title = "Galerie du Groupe";
        }

        async partial void OnGroupIdChanged(int value)
        {
            if (value > 0) await LoadPhotos();
        }

        [RelayCommand]
        private async Task LoadPhotos()
        {
            IsBusy = true;
            try
            {
                Photos.Clear();
                var list = await _repository.GetPhotosByGroupIdAsync(GroupId);
                foreach (var p in list) Photos.Add(p);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ImportPhoto()
        {
            try
            {
                // 1. On demande PLUSIEURS photos (PickPhotosAsync)
                var results = await MediaPicker.Default.PickPhotosAsync();

                if (results != null && results.Any())
                {
                    IsBusy = true;

                    // 2. On boucle sur chaque photo sélectionnée
                    foreach (var photo in results)
                    {
                        var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                        string localFilePath = Path.Combine(FileSystem.AppDataDirectory, newFileName);

                        using Stream sourceStream = await photo.OpenReadAsync();
                        using FileStream localFileStream = File.OpenWrite(localFilePath);
                        await sourceStream.CopyToAsync(localFileStream);

                        var newPhoto = new GroupPhoto
                        {
                            GroupId = GroupId,
                            FilePath = localFilePath,
                            DateTaken = DateTime.Now,
                            TagsJson = "[]" // On initialise la liste des tags vide
                        };

                        await _repository.AddPhotoAsync(newPhoto);

                        // Ajout en haut de la liste
                        Photos.Insert(0, newPhoto);
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", "Problème d'import : " + ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        private async Task OpenPhoto(GroupPhoto photo)
        {
            if (photo == null) return;
            // On navigue vers la page de détail (qu'on va créer juste après)
            await Shell.Current.GoToAsync($"{nameof(PhotoDetailView)}?Id={photo.Id}");
        }
    }
}