using Coach_app.Data.Repositories;
using Coach_app.Models.Domains.Groups;
using Coach_app.ViewModels.Base;
using Coach_app.Views.Groups;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Coach_app.ViewModels.Groups
{
    [QueryProperty(nameof(GroupId), "Id")]
    public partial class GroupSessionsViewModel : ViewModelBase
    {
        private readonly IGroupRepository _repository;

        [ObservableProperty] private int _groupId;
        [ObservableProperty] private string _groupName;

        // --- CHAMPS POUR AJOUTER UNE SÉANCE ---
        [ObservableProperty] private DateTime _newDate = DateTime.Today;
        [ObservableProperty] private TimeSpan _newStartTime = new TimeSpan(18, 0, 0);
        [ObservableProperty] private TimeSpan _newEndTime = new TimeSpan(20, 0, 0);

        public ObservableCollection<GroupSession> Sessions { get; } = new();

        public GroupSessionsViewModel(IGroupRepository repository)
        {
            _repository = repository;
        }

        async partial void OnGroupIdChanged(int value)
        {
            if (value > 0)
            {
                var group = await _repository.GetGroupByIdAsync(value);
                if (group != null)
                {
                    GroupName = group.Name;
                    // Pré-remplir les horaires avec ceux du groupe par défaut
                    NewStartTime = group.StartTime;
                    NewEndTime = group.EndTime;
                }
                await LoadSessions();
            }
        }

        [RelayCommand]
        private async Task LoadSessions()
        {
            IsBusy = true;
            try
            {
                Sessions.Clear();
                var list = await _repository.GetSessionsByGroupIdAsync(GroupId);

                // On filtre pour ne montrer que les séances futures ou récentes (optionnel)
                // Ici je montre tout, trié par date
                foreach (var s in list.OrderBy(x => x.Date))
                {
                    Sessions.Add(s);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddSession()
        {
            IsBusy = true;
            try
            {
                var newSession = new GroupSession
                {
                    GroupId = GroupId,
                    Date = NewDate,
                    StartTime = NewStartTime,
                    EndTime = NewEndTime,
                    Status = "Scheduled",
                    Note = "Séance ajoutée manuellement"
                };

                await _repository.AddSessionAsync(newSession);
                await LoadSessions(); // Recharger la liste

                // Petit feedback visuel
                await Shell.Current.DisplayAlert("Succès", "Séance ajoutée !", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteSession(GroupSession session)
        {
            if (session == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Attention",
                $"Supprimer la séance du {session.Date:dd/MM} ?", "Oui", "Non");

            if (confirm)
            {
                await _repository.DeleteSessionAsync(session.Id);
                Sessions.Remove(session);
            }
        }

        [RelayCommand]
        private async Task GoBack() => await Shell.Current.GoToAsync("..");


        [RelayCommand]
        private async Task GoToSession(GroupSession session)
        {
            if (session == null) return;

            // On navigue vers la page d'appel qu'on a créée avant
            await Shell.Current.GoToAsync($"{nameof(SessionDetailView)}?Id={session.Id}");
        }
    }

}