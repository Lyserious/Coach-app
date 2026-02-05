using Coach_app.ViewModels.Groups;

namespace Coach_app.Views.Groups;

public partial class GroupsView : ContentPage
{
    public GroupsView(GroupsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}