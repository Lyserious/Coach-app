using Coach_app.Data.Context;
using Coach_app.Data.Repositories;
using Coach_app.Data.Repositories.Interfaces;
using Coach_app.Services.Auth;
using Coach_app.Services.Data;
using Coach_app.Services.Files;
using Coach_app.Services.Training;
using Coach_app.ViewModels.Auth;
using Coach_app.ViewModels.Exercises;
using Coach_app.ViewModels.Groups;
using Coach_app.ViewModels.Home;
using Coach_app.ViewModels.Settings;
using Coach_app.ViewModels.Students;
using Coach_app.ViewModels.Templates;
using Coach_app.Views.Auth;
using Coach_app.Views.Exercises;
using Coach_app.Views.Groups;
using Coach_app.Views.Home;
using Coach_app.Views.Settings;
using Coach_app.Views.Students;
using Coach_app.Views.Templates;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;


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


            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
            {
#if WINDOWS
                handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                handler.PlatformView.Style = null; 
#elif ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            // --- SERVICES ---
            builder.Services.AddSingleton<CoachDbContext>();
            builder.Services.AddSingleton<ICoachDatabaseService, CoachDatabaseService>();
            builder.Services.AddSingleton<IFileService, FileService>();
            builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<ISessionService, SessionService>();
            builder.Services.AddTransient<ISessionComposer, SessionComposer>();

            // --- REPOSITORIES ---

            // On force le type de l'interface avec son chemin complet pour éviter toute erreur
            builder.Services.AddTransient<Coach_app.Data.Repositories.Interfaces.ISessionRepository, SessionRepository>();
            builder.Services.AddTransient<Coach_app.Data.Repositories.Interfaces.ITemplateRepository, TemplateRepository>();
            builder.Services.AddTransient<Coach_app.Data.Repositories.Interfaces.IAttendanceRepository, AttendanceRepository>();
            builder.Services.AddTransient<Coach_app.Data.Repositories.Interfaces.IPerformanceRepository, PerformanceRepository>();
            builder.Services.AddTransient<Coach_app.Data.Repositories.Interfaces.IStudentContactRepository, StudentContactRepository>();

            // Les autres repositories (pas d'ambiguïté connue mais on sécurise)
            builder.Services.AddTransient<Coach_app.Data.Repositories.Interfaces.IGroupRepository, GroupRepository>();
            builder.Services.AddTransient<Coach_app.Data.Repositories.Interfaces.IStudentRepository, StudentRepository>();
            builder.Services.AddTransient<Coach_app.Data.Repositories.Interfaces.IExerciseRepository, ExerciseRepository>();
            builder.Services.AddSingleton<Coach_app.Data.Repositories.Interfaces.ICoachRepository, CoachRepository>();
            builder.Services.AddSingleton<Coach_app.Data.Repositories.Interfaces.INoteRepository, NoteRepository>();
            builder.Services.AddSingleton<Coach_app.Data.Repositories.Interfaces.IPhotoRepository, PhotoRepository>();


            // --- VUES & VIEWMODELS ---
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<LoginViewModel>();

            builder.Services.AddTransient<DashboardView>();
            builder.Services.AddTransient<DashboardViewModel>();

            builder.Services.AddTransient<SettingsView>();
            builder.Services.AddTransient<SettingsViewModel>();

            builder.Services.AddTransient<GroupsView>();
            builder.Services.AddTransient<GroupsViewModel>();
            builder.Services.AddTransient<GroupDetailView>();
            builder.Services.AddTransient<GroupDetailViewModel>();
            builder.Services.AddTransient<GroupDashboardView>();
            builder.Services.AddTransient<GroupDashboardViewModel>();
            builder.Services.AddTransient<GroupSessionsView>();
            builder.Services.AddTransient<GroupSessionsViewModel>();
            builder.Services.AddTransient<GroupGalleryView>();
            builder.Services.AddTransient<GroupGalleryViewModel>();

            builder.Services.AddTransient<SessionDetailView>();
            builder.Services.AddTransient<SessionDetailViewModel>();
            builder.Services.AddTransient<SessionTemplatesView>();
            builder.Services.AddTransient<SessionTemplatesViewModel>();
            builder.Services.AddTransient<SessionTemplateDetailView>();
            builder.Services.AddTransient<SessionTemplateDetailViewModel>();

            builder.Services.AddTransient<StudentLibraryView>();
            builder.Services.AddTransient<StudentLibraryViewModel>();
            builder.Services.AddTransient<StudentDetailView>();
            builder.Services.AddTransient<StudentDetailViewModel>();
            builder.Services.AddTransient<StudentProfileView>();
            builder.Services.AddTransient<StudentProfileViewModel>();
            builder.Services.AddTransient<AddExistingStudentView>();
            builder.Services.AddTransient<AddExistingStudentViewModel>();
            builder.Services.AddTransient<StudentPhotoDetailView>();
            builder.Services.AddTransient<StudentPhotoDetailViewModel>();

            builder.Services.AddTransient<ExerciseLibraryView>();
            builder.Services.AddTransient<ExerciseLibraryViewModel>();
            builder.Services.AddTransient<ExerciseDetailView>();
            builder.Services.AddTransient<ExerciseDetailViewModel>();

            builder.Services.AddTransient<PhotoDetailView>();
            builder.Services.AddTransient<PhotoDetailViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}