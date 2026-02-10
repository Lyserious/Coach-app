using Coach_app.ViewModels.Templates;

namespace Coach_app.Views.Templates;

public partial class SessionTemplatesView : ContentPage
{
    public SessionTemplatesView(SessionTemplatesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}