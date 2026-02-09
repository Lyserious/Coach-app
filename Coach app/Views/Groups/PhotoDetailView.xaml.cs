using Coach_app.ViewModels.Groups;
namespace Coach_app.Views.Groups;

public partial class PhotoDetailView : ContentPage
{
    public PhotoDetailView(PhotoDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}