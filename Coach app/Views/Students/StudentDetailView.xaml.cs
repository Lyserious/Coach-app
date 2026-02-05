using Coach_app.ViewModels.Students;
namespace Coach_app.Views.Students;

public partial class StudentDetailView : ContentPage
{
    public StudentDetailView(StudentDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}