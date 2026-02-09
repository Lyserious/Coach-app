using Coach_app.ViewModels.Groups;
namespace Coach_app.Views.Groups;

public partial class GroupSessionsView : ContentPage
{
    public GroupSessionsView(GroupSessionsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}