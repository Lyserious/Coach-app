using Coach_app.Views.Exercises; 
using Coach_app.Views.Groups;
using Coach_app.Views.Students;



namespace Coach_app;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Enregistrement des routes de détail
        
        Routing.RegisterRoute(nameof(GroupDetailView), typeof(GroupDetailView));
        Routing.RegisterRoute(nameof(GroupDashboardView), typeof(GroupDashboardView));
        Routing.RegisterRoute(nameof(ExerciseDetailView), typeof(ExerciseDetailView));
        Routing.RegisterRoute(nameof(StudentDetailView), typeof(StudentDetailView));
        Routing.RegisterRoute(nameof(AddExistingStudentView), typeof(AddExistingStudentView));


    }
}