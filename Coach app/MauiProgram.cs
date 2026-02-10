using Coach_app.Data.Repositories;
using Coach_app.Services.Auth;
using Coach_app.ViewModels.Auth;
using Coach_app.ViewModels.Exercises; 
using Coach_app.ViewModels.Groups;
using Coach_app.ViewModels.Home;
using Coach_app.ViewModels.Students;
using Coach_app.Views.Auth;
using Coach_app.Views.Exercises;
using Coach_app.Views.Exercises;
using Coach_app.Views.Groups;
using Coach_app.Views.Home;
using Coach_app.Views.Students;
using CommunityToolkit.Maui; 
using Microsoft.Extensions.Logging;




namespace Coach_app
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit() 
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif



            // Services
            builder.Services.AddSingleton<ICoachRepository, CoachRepository>();
            builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<ISessionService, SessionService>();
            builder.Services.AddTransient<Views.Settings.SettingsView>();



            // Repositories
            builder.Services.AddTransient<IGroupRepository, GroupRepository>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<IExerciseRepository, ExerciseRepository>();
            builder.Services.AddTransient<IStudentRepository, StudentRepository>();





            // Views
            builder.Services.AddTransient<Coach_app.Views.Groups.GroupsView>();
            builder.Services.AddTransient<Coach_app.Views.Home.DashboardView>();
            builder.Services.AddTransient<Coach_app.Views.Groups.GroupDetailView>();
            builder.Services.AddTransient<GroupDashboardViewModel>();
            builder.Services.AddTransient<GroupDashboardView>();
            builder.Services.AddTransient<ExerciseLibraryView>();
            builder.Services.AddTransient<ExerciseDetailView>();
            builder.Services.AddTransient<StudentDetailView>();
            builder.Services.AddTransient<AddExistingStudentView>();
            builder.Services.AddTransient<StudentLibraryView>();
            builder.Services.AddTransient<StudentProfileView>();
            builder.Services.AddTransient<SessionDetailView>();
            builder.Services.AddTransient<GroupSessionsView>();
            builder.Services.AddTransient<GroupGalleryView>();
            builder.Services.AddTransient<PhotoDetailView>();
            builder.Services.AddTransient<Views.Templates.SessionTemplatesView>();
            builder.Services.AddTransient<Views.Templates.SessionTemplateDetailView>();



            // ViewModels

            builder.Services.AddTransient<Coach_app.ViewModels.Home.DashboardViewModel>();
            builder.Services.AddTransient<Coach_app.ViewModels.Groups.GroupsViewModel>();
            builder.Services.AddTransient<Coach_app.ViewModels.Groups.GroupDetailViewModel>();
            builder.Services.AddTransient<ExerciseLibraryViewModel>();
            builder.Services.AddTransient<ExerciseDetailViewModel>();
            builder.Services.AddTransient<StudentDetailViewModel>();
            builder.Services.AddTransient<AddExistingStudentViewModel>();
            builder.Services.AddTransient<StudentLibraryViewModel>();
            builder.Services.AddTransient<ViewModels.Settings.SettingsViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<StudentProfileViewModel>();
            builder.Services.AddTransient<SessionDetailViewModel>();
            builder.Services.AddTransient<GroupSessionsViewModel>();
            builder.Services.AddTransient<GroupGalleryViewModel>();
            builder.Services.AddTransient<PhotoDetailViewModel>();
            builder.Services.AddTransient<ViewModels.Templates.SessionTemplatesViewModel>();
            builder.Services.AddTransient<ViewModels.Templates.SessionTemplateDetailViewModel>();




            //return builder.Build();
            return builder.Build();
        }
    }
}
