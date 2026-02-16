using Coach_app.Data.Repositories.Interfaces;
using Coach_app.Models;
using Coach_app.Services.Auth;
using Coach_app.ViewModels.Base;
using Coach_app.Views.Groups;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Coach_app.ViewModels.Home
{
    public partial class DashboardViewModel : ViewModelBase
    {
        private readonly ISessionService _sessionService;
        private readonly IGroupRepository _groupRepository;
        private readonly ISessionRepository _sessionRepository; // AJOUT

        [ObservableProperty] private string _welcomeMessage;
        [ObservableProperty] private DateTime _selectedDate;
        [ObservableProperty] private string _dateDisplay;

        public ObservableCollection<SessionItem> TodaysSessions { get; } = new();

        // Injection mise à jour
        public DashboardViewModel(ISessionService sessionService, IGroupRepository groupRepository, ISessionRepository sessionRepository)
        {
            _sessionService = sessionService;
            _groupRepository = groupRepository;
            _sessionRepository = sessionRepository;

            Title = "Tableau de bord";
            UpdateWelcomeMessage();
            SelectedDate = DateTime.Today;
        }

        private void UpdateWelcomeMessage()
        {
            var coachName = _sessionService.CurrentCoach?.Name ?? "Coach";
            WelcomeMessage = $"Bonjour, {coachName} !";
        }

        async partial void OnSelectedDateChanged(DateTime value)
        {
            if (value.Date == DateTime.Today) DateDisplay = "Aujourd'hui";
            else if (value.Date == DateTime.Today.AddDays(1)) DateDisplay = "Demain";
            else DateDisplay = value.ToString("D", new CultureInfo("fr-FR"));

            await LoadSessions();
        }

        [RelayCommand]
        public async Task LoadSessions()
        {
            IsBusy = true;
            try
            {
                TodaysSessions.Clear();

                // Appel via SessionRepository
                var rawSessions = await _sessionRepository.GetSessionsByDateAsync(SelectedDate);

                var uniqueSessions = rawSessions
                    .GroupBy(s => new { s.GroupId, s.StartTime })
                    .Select(g => g.First())
                    .ToList();

                foreach (var session in uniqueSessions)
                {
                    // GroupRepository est toujours utilisé pour les infos du groupe, c'est correct
                    var group = await _groupRepository.GetGroupByIdAsync(session.GroupId);
                    if (group != null)
                    {
                        TodaysSessions.Add(new SessionItem
                        {
                            SessionId = session.Id,
                            GroupId = group.Id,
                            TimeDisplay = $"{session.StartTime:hh\\:mm} - {session.EndTime:hh\\:mm}",
                            GroupName = group.Name,
                            PhotoPath = group.PhotoPath,
                            StatusColor = session.Status == "Cancelled" ? Colors.Red : Colors.Black
                        });
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand] private void PreviousDay() => SelectedDate = SelectedDate.AddDays(-1);
        [RelayCommand] private void NextDay() => SelectedDate = SelectedDate.AddDays(1);

        [RelayCommand]
        private async Task GoToSession(SessionItem item)
        {
            if (item != null)
                await Shell.Current.GoToAsync($"{nameof(SessionDetailView)}?Id={item.SessionId}");
        }

        [RelayCommand]
        private async Task GoToPage(string route)
        {
            if (!string.IsNullOrEmpty(route)) await Shell.Current.GoToAsync(route);
        }

        [RelayCommand]
        private async Task Logout()
        {
            _sessionService.ClearSession();
            await Shell.Current.GoToAsync("//Login");
        }
        [RelayCommand]
        private void OpenMenu()
        {
            // C'est la commande pour ouvrir le menu latéral manuellement
            Shell.Current.FlyoutIsPresented = true;
        }
    }

    public class SessionItem
    {
        public int SessionId { get; set; }
        public int GroupId { get; set; }
        public string TimeDisplay { get; set; }
        public string GroupName { get; set; }
        public string PhotoPath { get; set; }
        public Color StatusColor { get; set; }
    }
}