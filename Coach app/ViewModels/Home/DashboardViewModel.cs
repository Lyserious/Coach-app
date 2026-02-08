using Coach_app.Data.Repositories;
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
        private readonly IGroupRepository _groupRepository; // Nécessaire pour charger les cours

        [ObservableProperty]
        private string _welcomeMessage;

        // --- AJOUT CALENDRIER : Propriétés ---
        [ObservableProperty]
        private DateTime _selectedDate;

        [ObservableProperty]
        private string _dateDisplay;

        public ObservableCollection<SessionItem> TodaysSessions { get; } = new();
        // -------------------------------------

        public DashboardViewModel(ISessionService sessionService, IGroupRepository groupRepository)
        {
            _sessionService = sessionService;
            _groupRepository = groupRepository; // Injection du repo

            Title = "Tableau de bord";
            UpdateWelcomeMessage();

            // Initialiser le calendrier à aujourd'hui
            SelectedDate = DateTime.Today;
        }

        private void UpdateWelcomeMessage()
        {
            var coachName = _sessionService.CurrentCoach?.Name ?? "Coach";
            WelcomeMessage = $"Bonjour, {coachName} !";
        }

        // --- AJOUT CALENDRIER : Méthodes ---

        // Se déclenche automatiquement quand SelectedDate change
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
                // On récupère les séances via le Repository qu'on a créé à l'étape précédente
                var sessions = await _groupRepository.GetSessionsByDateAsync(SelectedDate);

                foreach (var session in sessions)
                {
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

        [RelayCommand]
        private void PreviousDay() => SelectedDate = SelectedDate.AddDays(-1);

        [RelayCommand]
        private void NextDay() => SelectedDate = SelectedDate.AddDays(1);

        [RelayCommand]
        private async Task GoToSession(SessionItem item)
        {
            // Redirige vers le groupe pour l'instant
            if (item != null)
                await Shell.Current.GoToAsync($"{nameof(SessionDetailView)}?Id={item.SessionId}");
        }
        // -------------------------------------


        [RelayCommand]
        private async Task GoToPage(string route)
        {
            if (!string.IsNullOrEmpty(route))
            {
                await Shell.Current.GoToAsync(route);
            }
        }

        [RelayCommand]
        private async Task Logout()
        {
            _sessionService.ClearSession();
            await Shell.Current.GoToAsync("//Login");
        }
    }

    // Petite classe pour l'affichage dans la liste
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