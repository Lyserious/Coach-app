using Coach_app.ViewModels.Groups;

namespace Coach_app.Views.Groups;

public partial class GroupDashboardView : ContentPage
{
    public GroupDashboardView(GroupDashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}