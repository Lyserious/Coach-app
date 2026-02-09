using Coach_app.ViewModels.Groups;
namespace Coach_app.Views.Groups;

public partial class GroupGalleryView : ContentPage
{
    public GroupGalleryView(GroupGalleryViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}