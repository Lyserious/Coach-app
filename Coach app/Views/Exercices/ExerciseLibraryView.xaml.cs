using Coach_app.ViewModels.Exercises;

namespace Coach_app.Views.Exercises;

public partial class ExerciseLibraryView : ContentPage
{
    public ExerciseLibraryView(ExerciseLibraryViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}