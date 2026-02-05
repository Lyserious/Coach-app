using Coach_app.ViewModels.Students;
namespace Coach_app.Views.Students;

public partial class AddExistingStudentView : ContentPage
{
    public AddExistingStudentView(AddExistingStudentViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}