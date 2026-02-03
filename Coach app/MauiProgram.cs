using Coach_app.Data.Repositories;
using Microsoft.Extensions.Logging;
using Coach_app.Services.Auth;
using Coach_app.ViewModels.Auth;
using Coach_app.Views.Auth;



namespace Coach_app
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif



            // Services Data
            builder.Services.AddSingleton<ICoachRepository, CoachRepository>();
            builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddTransient<LoginViewModel>(); 
            builder.Services.AddTransient<LoginPage>();
            return builder.Build();
        }
    }
}
