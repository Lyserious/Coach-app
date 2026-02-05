using Coach_app.ViewModels.Home;

namespace Coach_app.Views.Home;

public partial class DashboardView : ContentPage
{
    public DashboardView(DashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}