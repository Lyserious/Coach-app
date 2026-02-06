using Coach_app.ViewModels.Students;

namespace Coach_app.Views.Students;

public partial class StudentProfileView : ContentPage
{
    public StudentProfileView(StudentProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}