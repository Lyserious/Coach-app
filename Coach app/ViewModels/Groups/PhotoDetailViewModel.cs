using Coach_app.Data.Repositories;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Coach_app.ViewModels.Groups
{
    [QueryProperty(nameof(PhotoId), "Id")]
    public partial class PhotoDetailViewModel : ViewModelBase
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IStudentRepository _studentRepository;

        [ObservableProperty] private int _photoId;
        [ObservableProperty] private string _imagePath;
        [ObservableProperty] private string _dateDisplay;

        // NOUVEAU : Gère si le panneau de tag est visible ou non
        [ObservableProperty] private bool _isTaggingVisible = false;

        public ObservableCollection<StudentTagItem> Students { get; } = new();
        private GroupPhoto _currentPhoto;

        public PhotoDetailViewModel(IGroupRepository groupRepository, IStudentRepository studentRepository)
        {
            _groupRepository = groupRepository;
            _studentRepository = studentRepository;
        }

        async partial void OnPhotoIdChanged(int value)
        {
            if (value > 0) await LoadData();
        }

        private async Task LoadData()
        {
            IsBusy = true;
            try
            {
                _currentPhoto = await _groupRepository.GetPhotoByIdAsync(PhotoId);

                if (_currentPhoto != null)
                {
                    ImagePath = _currentPhoto.FilePath;
                    DateDisplay = _currentPhoto.DateTaken.ToString("f");

                    List<int> taggedIds = new();
                    if (!string.IsNullOrEmpty(_currentPhoto.TagsJson))
                    {
                        try { taggedIds = JsonSerializer.Deserialize<List<int>>(_currentPhoto.TagsJson); } catch { }
                    }

                    var allStudents = await _studentRepository.GetStudentsByGroupIdAsync(_currentPhoto.GroupId);
                    Students.Clear();
                    foreach (var student in allStudents)
                    {
                        Students.Add(new StudentTagItem
                        {
                            StudentId = student.Id,
                            Name = student.DisplayName,
                            IsTagged = taggedIds.Contains(student.Id)
                        });
                    }
                }
            }
            finally { IsBusy = false; }
        }

        // NOUVEAU : Le menu "Trois points"
        [RelayCommand]
        private async Task ShowOptions()
        {
            const string ACTION_TAG = "🏷️ Identifier des élèves";
            const string ACTION_SHARE = "📤 Partager / Sauvegarder";
            const string ACTION_DELETE = "🗑️ Supprimer la photo";

            string action = await Shell.Current.DisplayActionSheet("Options de la photo", "Annuler", null,
                ACTION_TAG, ACTION_SHARE, ACTION_DELETE);

            switch (action)
            {
                case ACTION_TAG:
                    IsTaggingVisible = true; // On affiche le panneau
                    break;
                case ACTION_SHARE:
                    await SharePhoto();
                    break;
                case ACTION_DELETE:
                    await DeletePhoto();
                    break;
            }
        }

        [RelayCommand]
        private void HideTagging()
        {
            IsTaggingVisible = false; // Fermer le panneau sans sauver
        }

        [RelayCommand]
        private async Task SaveTags()
        {
            if (_currentPhoto == null) return;
            var taggedIds = Students.Where(s => s.IsTagged).Select(s => s.StudentId).ToList();
            _currentPhoto.TagsJson = JsonSerializer.Serialize(taggedIds);
            await _groupRepository.UpdatePhotoAsync(_currentPhoto);

            IsTaggingVisible = false; // On cache le panneau après sauvegarde
            await Shell.Current.DisplayAlert("Succès", "Identification enregistrée", "OK");
        }

        private async Task SharePhoto()
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Partager la photo",
                File = new ShareFile(ImagePath)
            });
        }

        private async Task DeletePhoto()
        {
            bool confirm = await Shell.Current.DisplayAlert("Supprimer ?", "Cette action est irréversible.", "Oui", "Non");
            if (confirm)
            {
                await _groupRepository.DeletePhotoAsync(_currentPhoto);
                await Shell.Current.GoToAsync("..");
            }
        }
    }

    public class StudentTagItem
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public bool IsTagged { get; set; }
    }
}