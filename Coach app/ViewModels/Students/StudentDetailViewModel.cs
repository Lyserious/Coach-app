using Coach_app.Data.Repositories;
using Coach_app.Helpers;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Coach_app.ViewModels.Students
{
    [QueryProperty(nameof(GroupId), "GroupId")]
    [QueryProperty(nameof(StudentId), "StudentId")]
    public partial class StudentDetailViewModel : ViewModelBase
    {
        private readonly IStudentRepository _repository;

        [ObservableProperty] private int _groupId;
        [ObservableProperty] private int _studentId;

        [ObservableProperty] private string _firstName;
        [ObservableProperty] private string _lastName;
        [ObservableProperty] private string _nickname;
        [ObservableProperty] private string _selectedLevel = "5a";
        [ObservableProperty] private string _profilePhotoPath;
        [ObservableProperty] private string _quickContactPhone;

        public List<string> Levels => ClimbingGrades.All;

        // Propriété pour savoir si on est en modif (pour afficher le bouton supprimer)
        public bool IsEditMode => StudentId > 0;

        public StudentDetailViewModel(IStudentRepository repository)
        {
            _repository = repository;
            Title = "Nouvel Élève";
        }

        async partial void OnStudentIdChanged(int value)
        {
            if (value > 0)
            {
                Title = "Modifier le profil";
                OnPropertyChanged(nameof(IsEditMode)); // Met à jour l'affichage du bouton supprimer

                var s = await _repository.GetStudentByIdAsync(value);
                if (s != null)
                {
                    FirstName = s.FirstName;
                    LastName = s.LastName;
                    Nickname = s.Nickname;
                    SelectedLevel = s.MaxLevel;
                    ProfilePhotoPath = s.ProfilePhotoPath;
                }
            }
        }

        // --- GESTION PHOTO ---
        [RelayCommand]
        private async Task PickPhoto()
        {
            try
            {
                // Ouvre la galerie du téléphone/PC
                var result = await MediaPicker.Default.PickPhotoAsync();
                if (result != null)
                {
                    // On garde le chemin local du fichier
                    ProfilePhotoPath = result.FullPath;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", "Impossible d'accéder aux photos : " + ex.Message, "OK");
            }
        }
        // ---------------------

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                await Shell.Current.DisplayAlert("Erreur", "Le prénom est obligatoire.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var student = new Student
                {
                    Id = StudentId,
                    FirstName = FirstName,
                    LastName = LastName,
                    Nickname = Nickname,
                    MaxLevel = SelectedLevel,
                    ProfilePhotoPath = ProfilePhotoPath,
                    CreatedAt = StudentId > 0 ? (await _repository.GetStudentByIdAsync(StudentId)).CreatedAt : DateTime.UtcNow
                };

                // Sauvegarde en base
                int savedId = await _repository.SaveStudentAsync(student);

                // Liaison au groupe (Uniquement si création et qu'on vient d'un groupe)
                if (StudentId == 0 && GroupId > 0)
                {
                    await _repository.AddStudentToGroupAsync(savedId, GroupId);
                }

                // Contact (Uniquement à la création)
                if (StudentId == 0 && !string.IsNullOrWhiteSpace(QuickContactPhone))
                {
                    await _repository.SaveContactAsync(new StudentContact
                    {
                        StudentId = savedId,
                        Type = ContactType.Phone,
                        Value = QuickContactPhone
                    });
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", "Problème lors de la sauvegarde : " + ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Delete() // Fonctionnalité "Retirer un élève" (Archivage simple ici)
        {
            bool confirm = await Shell.Current.DisplayAlert("Attention", "Voulez-vous retirer cet élève du groupe ?", "Oui", "Non");
            if (confirm && GroupId > 0)
            {
                await _repository.RemoveStudentFromGroupAsync(StudentId, GroupId);
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        private async Task Cancel() => await Shell.Current.GoToAsync("..");
    }
}