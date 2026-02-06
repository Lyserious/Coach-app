using Coach_app.Data.Repositories;
using Coach_app.Helpers;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Coach_app.ViewModels.Students
{
    [QueryProperty(nameof(GroupId), "GroupId")]
    [QueryProperty(nameof(StudentId), "StudentId")]
    public partial class StudentDetailViewModel : ViewModelBase
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

        // NOUVEAU : Email
        [ObservableProperty] private string _email;

        // --- LISTE DES CONTACTS (Au lieu d'un seul) ---
        public ObservableCollection<StudentContact> Contacts { get; } = new();

        public List<string> Levels => ClimbingGrades.All;
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
                OnPropertyChanged(nameof(IsEditMode));

                // 1. Charger l'élève
                var s = await _repository.GetStudentByIdAsync(value);
                if (s != null)
                {
                    FirstName = s.FirstName;
                    LastName = s.LastName;
                    Nickname = s.Nickname;
                    SelectedLevel = s.MaxLevel;
                    ProfilePhotoPath = s.ProfilePhotoPath;
                    PhoneNumber = s.PhoneNumber;
                    Email = s.Email; // Charger l'email
                }

                // 2. Charger TOUS les contacts
                Contacts.Clear();
                var list = await _repository.GetStudentContactsAsync(value);
                foreach (var c in list) Contacts.Add(c);
            }
        }

        // --- GESTION DES CONTACTS ---

        [RelayCommand]
        private void AddContact()
        {
            // Ajoute une fiche vide à la liste visuelle
            Contacts.Add(new StudentContact
            {
                Relation = "Parent", // Valeur par défaut suggérée
                StudentId = StudentId
            });
        }

        [RelayCommand]
        private async Task RemoveContact(StudentContact contact)
        {
            if (contact == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Retirer", "Supprimer ce contact ?", "Oui", "Non");
            if (!confirm) return;

            // Si le contact existe déjà en base (Id > 0), on le supprime vraiment
            if (contact.Id > 0)
            {
                await _repository.DeleteContactAsync(contact.Id);
            }

            // On le retire de la liste visuelle
            Contacts.Remove(contact);
        }

        // --- COMMANDES PRINCIPALES ---

        [RelayCommand]
        private async Task PickPhoto()
        {
            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result != null) ProfilePhotoPath = result.FullPath;
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
                // 1. Sauvegarde Élève
                var student = new Student
                {
                    Id = StudentId,
                    FirstName = FirstName,
                    LastName = LastName,
                    Nickname = Nickname,
                    MaxLevel = SelectedLevel,
                    ProfilePhotoPath = ProfilePhotoPath,
                    PhoneNumber = PhoneNumber,
                    Email = Email, // Sauvegarde Email
                    CreatedAt = StudentId > 0 ? (await _repository.GetStudentByIdAsync(StudentId)).CreatedAt : DateTime.UtcNow
                };

                int savedId = await _repository.SaveStudentAsync(student);

                // 2. Sauvegarde des Contacts de la liste
                foreach (var c in Contacts)
                {
                    // On s'assure que le contact est bien lié au bon ID élève (important si création)
                    c.StudentId = savedId;

                    // On ne sauvegarde que si y'a au moins un nom ou un numéro
                    if (!string.IsNullOrWhiteSpace(c.FirstName) || !string.IsNullOrWhiteSpace(c.PhoneNumber))
                    {
                        await _repository.SaveContactAsync(c);
                    }
                }

                // 3. Liaison Groupe (si création depuis un groupe)
                if (StudentId == 0 && GroupId > 0)
                {
                    await _repository.AddStudentToGroupAsync(savedId, GroupId);
                }

                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            bool confirm = await Shell.Current.DisplayAlert("Attention", "Voulez-vous supprimer définitivement cet élève ?", "Oui", "Non");
            if (confirm && StudentId > 0)
            {
                await _repository.DeleteStudentAsync(new Student { Id = StudentId });
                await Shell.Current.GoToAsync("//StudentLibraryView");
            }
        }

        [RelayCommand]
        private async Task Cancel() => await Shell.Current.GoToAsync("..");
    }
}