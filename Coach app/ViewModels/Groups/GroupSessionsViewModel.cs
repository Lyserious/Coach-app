using Coach_app.Data.Repositories.Interfaces; // Important : Utiliser le namespace Interfaces
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
        private readonly IGroupRepository _groupRepository;
        private readonly ISessionRepository _sessionRepository; // AJOUT

        [ObservableProperty] private int _groupId;
        [ObservableProperty] private string _groupName;

        [ObservableProperty] private DateTime _newDate = DateTime.Today;
        [ObservableProperty] private TimeSpan _newStartTime = new TimeSpan(18, 0, 0);
        [ObservableProperty] private TimeSpan _newEndTime = new TimeSpan(20, 0, 0);

        public ObservableCollection<GroupSession> Sessions { get; } = new();

        // On injecte les deux repositories
        public GroupSessionsViewModel(IGroupRepository groupRepository, ISessionRepository sessionRepository)
        {
            _groupRepository = groupRepository;
            _sessionRepository = sessionRepository;
        }

        async partial void OnGroupIdChanged(int value)
        {
            if (value > 0)
            {
                // Info groupe -> GroupRepository
                var group = await _groupRepository.GetGroupByIdAsync(value);
                if (group != null)
                {
                    GroupName = group.Name;
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
                // Séances -> SessionRepository
                var list = await _sessionRepository.GetSessionsByGroupIdAsync(GroupId);

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

                // Ajout -> SessionRepository
                await _sessionRepository.AddSessionAsync(newSession);
                await LoadSessions();

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
                // Suppression -> SessionRepository
                await _sessionRepository.DeleteSessionAsync(session.Id);
                Sessions.Remove(session);
            }
        }

        [RelayCommand]
        private async Task GoBack() => await Shell.Current.GoToAsync("..");


        [RelayCommand]
        private async Task GoToSession(GroupSession session)
        {
            if (session == null) return;
            await Shell.Current.GoToAsync($"{nameof(SessionDetailView)}?Id={session.Id}");
        }
    }
}