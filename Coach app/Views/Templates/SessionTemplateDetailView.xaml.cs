using Coach_app.ViewModels.Templates;

namespace Coach_app.Views.Templates;

public partial class SessionTemplateDetailView : ContentPage
{
    public SessionTemplateDetailView(SessionTemplateDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}