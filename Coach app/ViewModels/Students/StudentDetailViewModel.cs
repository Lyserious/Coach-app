using Coach_app.Data.Repositories;
using Coach_app.Helpers;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using Coach_app.Views.Students; // Nécessaire pour nameof(StudentProfileView)
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Coach_app.ViewModels.Students
{
    public partial class StudentDetailViewModel : ViewModelBase, IQueryAttributable
    {
        private readonly IStudentRepository _repository;

        [ObservableProperty] private int _groupId;
        [ObservableProperty] private int _studentId;

        // --- INFOS ÉLÈVE ---
        [ObservableProperty] private string _firstName;
        [ObservableProperty] private string _lastName;
        [ObservableProperty] private string _nickname;
        [ObservableProperty] private string _selectedLevel = "5a";
        [ObservableProperty] private string _profilePhotoPath;
        [ObservableProperty] private string _phoneNumber;
        [ObservableProperty] private string _email;
        [ObservableProperty] private DateTime _birthDate = DateTime.Today; // Par défaut aujourd'hui
        // --- LISTE DES CONTACTS ---
        public ObservableCollection<StudentContact> Contacts { get; } = new();

        public List<string> Levels => ClimbingGrades.All;
        public bool IsEditMode => StudentId > 0;

        public StudentDetailViewModel(IStudentRepository repository)
        {
            _repository = repository;
            Title = "Nouvel Élève";
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("StudentId"))
            {
                if (int.TryParse(query["StudentId"].ToString(), out int sId))
                {
                    StudentId = sId;
                }
            }

            if (query.ContainsKey("GroupId"))
            {
                if (int.TryParse(query["GroupId"].ToString(), out int gId))
                {
                    GroupId = gId;
                }
            }
        }

        async partial void OnStudentIdChanged(int value)
        {
            if (value > 0)
            {
                Title = "Modifier le profil";
                OnPropertyChanged(nameof(IsEditMode));

                var s = await _repository.GetStudentByIdAsync(value);
                if (s != null)
                {
                    FirstName = s.FirstName;
                    LastName = s.LastName;
                    Nickname = s.Nickname;
                    SelectedLevel = s.MaxLevel;
                    ProfilePhotoPath = s.ProfilePhotoPath;
                    PhoneNumber = s.PhoneNumber;
                    Email = s.Email;
                    if (s.BirthDate != DateTime.MinValue)
                        BirthDate = s.BirthDate;
                }

                Contacts.Clear();
                var list = await _repository.GetStudentContactsAsync(value);
                foreach (var c in list) Contacts.Add(c);
            }
            else
            {
                Title = "Nouvel Élève";
                FirstName = "";
                LastName = "";
                Contacts.Clear();
                OnPropertyChanged(nameof(IsEditMode));
            }
        }

        [RelayCommand]
        private void AddContact()
        {
            Contacts.Add(new StudentContact
            {
                Relation = "Parent",
                StudentId = StudentId
            });
        }

        [RelayCommand]
        private async Task RemoveContact(StudentContact contact)
        {
            if (contact == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Retirer", "Supprimer ce contact ?", "Oui", "Non");
            if (!confirm) return;

            if (contact.Id > 0)
            {
                await _repository.DeleteContactAsync(contact.Id);
            }
            Contacts.Remove(contact);
        }

        [RelayCommand]
        private async Task PickPhoto()
        {
            try
            {
                var result = await MediaPicker.Default.PickPhotoAsync();
                if (result != null) ProfilePhotoPath = result.FullPath;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", "Impossible d'accéder aux photos.", "OK");
            }
        }

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
                    PhoneNumber = PhoneNumber,
                    Email = Email,
                    BirthDate = BirthDate,
                    CreatedAt = StudentId > 0 ? (await _repository.GetStudentByIdAsync(StudentId))?.CreatedAt ?? DateTime.UtcNow : DateTime.UtcNow
                };

                int savedId = await _repository.SaveStudentAsync(student);

                foreach (var c in Contacts)
                {
                    c.StudentId = savedId;
                    if (!string.IsNullOrWhiteSpace(c.FirstName) || !string.IsNullOrWhiteSpace(c.PhoneNumber))
                    {
                        await _repository.SaveContactAsync(c);
                    }
                }

                if (StudentId == 0 && GroupId > 0)
                {
                    await _repository.AddStudentToGroupAsync(savedId, GroupId);
                }

                // --- CORRECTION DU BUG AMBIGUOUS ROUTES ---
                // On force le retour à l'annuaire PUIS au profil de l'élève mis à jour
                await Shell.Current.GoToAsync($"//StudentLibraryView/{nameof(StudentProfileView)}?Id={savedId}");
            }
            catch (Exception ex)
            {
                // En cas de pépin, retour à la liste principale
                await Shell.Current.GoToAsync("//StudentLibraryView");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            bool confirm = await Shell.Current.DisplayAlert("Attention", "Supprimer définitivement ?", "Oui", "Non");
            if (confirm && StudentId > 0)
            {
                var s = await _repository.GetStudentByIdAsync(StudentId);
                if (s != null) await _repository.DeleteStudentAsync(s);

                await Shell.Current.GoToAsync("//StudentLibraryView");
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            // Pour annuler, on retourne simplement à la page précédente.
            // Si ça plante aussi, remplace par : await Shell.Current.GoToAsync("..");
            try
            {
                await Shell.Current.Navigation.PopAsync();
            }
            catch
            {
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}