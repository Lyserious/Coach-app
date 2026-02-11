using Coach_app.Core.Helpers;
using Coach_app.Data.Repositories;
using Coach_app.Models;
using Coach_app.Models.Domains.Students;
using Coach_app.ViewModels.Base;
using Coach_app.Views.Students;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Coach_app.ViewModels.Students
{
    public partial class StudentDetailViewModel : ViewModelBase, IQueryAttributable
    {
        private readonly IStudentRepository _repository;
        private readonly INoteRepository _noteRepository; // AJOUT : Repository des notes

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
        [ObservableProperty] private DateTime _birthDate = DateTime.Today;
        [ObservableProperty] private string _photoConsent = "Interne uniquement";

        // --- LISTE DES CONTACTS ---
        public ObservableCollection<StudentContact> Contacts { get; } = new();

        // --- NOTES (AJOUT) ---
        public ObservableCollection<AppNote> Notes { get; } = new(); // Liste des notes affichée
        [ObservableProperty] private string _newNoteText; // Champ de saisie

        public List<string> Levels => ClimbingGrades.All;
        public bool IsEditMode => StudentId > 0;

        public List<string> ConsentOptions { get; } = new()
        {
            "Autorisé (Réseaux/Web)",
            "Interne uniquement",
            "Refusé (Pas de photo)"
        };

        // Constructeur mis à jour avec INoteRepository
        public StudentDetailViewModel(IStudentRepository repository, INoteRepository noteRepository)
        {
            _repository = repository;
            _noteRepository = noteRepository;
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
                    if (!string.IsNullOrEmpty(s.PhotoConsent))
                        PhotoConsent = s.PhotoConsent;
                }

                Contacts.Clear();
                var list = await _repository.GetStudentContactsAsync(value);
                foreach (var c in list) Contacts.Add(c);

                await LoadNotes(); // AJOUT : Charger les notes
            }
            else
            {
                Title = "Nouvel Élève";
                FirstName = "";
                LastName = "";
                Contacts.Clear();
                Notes.Clear(); // AJOUT : Vider les notes
                OnPropertyChanged(nameof(IsEditMode));
            }
        }

        // --- METHODES NOTES (AJOUT) ---

        private async Task LoadNotes()
        {
            if (StudentId == 0) return;
            var noteList = await _noteRepository.GetNotesAsync(NoteTargetType.Student, StudentId);
            Notes.Clear();
            foreach (var n in noteList) Notes.Add(n);
        }

        [RelayCommand]
        private async Task AddNote()
        {
            if (string.IsNullOrWhiteSpace(NewNoteText) || StudentId == 0) return;

            var note = new AppNote
            {
                TargetType = NoteTargetType.Student,
                TargetId = StudentId,
                Content = NewNoteText,
                Date = DateTime.Now
            };

            await _noteRepository.SaveNoteAsync(note);
            Notes.Insert(0, note); // Ajoute au début de la liste
            NewNoteText = string.Empty; // Vide le champ
        }

        [RelayCommand]
        private async Task DeleteNote(AppNote note)
        {
            if (note == null) return;
            bool confirm = await Shell.Current.DisplayAlert("Supprimer", "Effacer cette note ?", "Oui", "Non");
            if (confirm)
            {
                await _noteRepository.DeleteNoteAsync(note);
                Notes.Remove(note);
            }
        }

        // --- METHODES ORIGINALES INCHANGÉES ---

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
                    PhotoConsent = PhotoConsent,
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