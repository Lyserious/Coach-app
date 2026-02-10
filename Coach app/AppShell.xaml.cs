using Coach_app.Views.Exercises; 
using Coach_app.Views.Groups;
using Coach_app.Views.Settings;
using Coach_app.Views.Students;
using Coach_app.Views.Home;


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
        Routing.RegisterRoute(nameof(StudentLibraryView), typeof(StudentLibraryView));
        Routing.RegisterRoute(nameof(SettingsView), typeof(SettingsView));
        Routing.RegisterRoute(nameof(ExerciseLibraryView), typeof(ExerciseLibraryView));
        Routing.RegisterRoute(nameof(StudentProfileView), typeof(StudentProfileView));
        Routing.RegisterRoute(nameof(Views.Groups.SessionDetailView), typeof(Views.Groups.SessionDetailView));
        Routing.RegisterRoute(nameof(Views.Groups.GroupSessionsView), typeof(Views.Groups.GroupSessionsView));
        Routing.RegisterRoute(nameof(Views.Groups.GroupGalleryView), typeof(Views.Groups.GroupGalleryView));
        Routing.RegisterRoute(nameof(PhotoDetailView), typeof(PhotoDetailView));
        Routing.RegisterRoute(nameof(Views.Templates.SessionTemplateDetailView), typeof(Views.Templates.SessionTemplateDetailView));


    }
}