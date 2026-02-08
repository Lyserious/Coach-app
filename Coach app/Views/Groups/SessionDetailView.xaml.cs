using Coach_app.ViewModels.Groups;

namespace Coach_app.Views.Groups
{
    public partial class SessionDetailView : ContentPage
    {
        public SessionDetailView(SessionDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}