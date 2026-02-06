using Coach_app.ViewModels.Students;

namespace Coach_app.Views.Students;

public partial class StudentLibraryView : ContentPage
{
    public StudentLibraryView(StudentLibraryViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}