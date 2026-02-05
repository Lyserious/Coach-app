using Coach_app.Data.Repositories;
using Coach_app.Models;
using Coach_app.ViewModels.Base;
using Coach_app.Views.Groups;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Coach_app.ViewModels.Groups
{
    public partial class GroupsViewModel : ViewModelBase
    {
        private readonly IGroupRepository _repository;

        public ObservableCollection<Group> Groups { get; } = new();

        // --- GESTION DU CLIC VIA SÉLECTION ---
        [ObservableProperty]
        private Group _selectedGroup;

        // Cette méthode est appelée automatiquement quand on clique sur un item
        partial void OnSelectedGroupChanged(Group value)
        {
            if (value != null)
            {
                // AVANT (Modif) :
                // Shell.Current.GoToAsync($"{nameof(GroupDetailView)}?Id={value.Id}");

                // MAINTENANT (Dashboard) :
                Shell.Current.GoToAsync($"{nameof(GroupDashboardView)}?Id={value.Id}");

                SelectedGroup = null;
            }
        }
        // -------------------------------------

        public GroupsViewModel(IGroupRepository repository)
        {
            Title = "Mes Groupes";
            _repository = repository;
        }

        [RelayCommand]
        public async Task LoadGroups()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Groups.Clear();
                var items = await _repository.GetActiveGroupsAsync();
                foreach (var item in items) Groups.Add(item);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task AddGroup()
        {
            await Shell.Current.GoToAsync(nameof(GroupDetailView));
        }
    }
}