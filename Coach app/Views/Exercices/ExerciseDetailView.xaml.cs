using Coach_app.ViewModels.Exercises;

namespace Coach_app.Views.Exercises;

public partial class ExerciseDetailView : ContentPage
{
    public ExerciseDetailView(ExerciseDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}