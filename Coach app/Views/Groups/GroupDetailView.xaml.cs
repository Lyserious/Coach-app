using Coach_app.ViewModels.Groups;

namespace Coach_app.Views.Groups;

public partial class GroupDetailView : ContentPage
{
    public GroupDetailView(GroupDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}