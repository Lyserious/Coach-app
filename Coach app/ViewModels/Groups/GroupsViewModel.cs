using Coach_app.Data.Repositories;
using Coach_app.Data.Repositories.Interfaces;
using Coach_app.Models.Domains.Groups;
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
        async partial void OnSelectedGroupChanged(Group value)
        {
            if (value != null)
            {
                // On navigue vers le Dashboard du groupe
                await Shell.Current.GoToAsync($"{nameof(GroupDashboardView)}?Id={value.Id}");
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
                // CORRECTION ICI : On utilise la méthode standard GetGroupsAsync
                var items = await _repository.GetGroupsAsync();
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