using InputKit.Handlers;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using Mopups.Services;
using Shiny;
using Shiny.BluetoothLE;
using SukaliCareApp.Data;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace SukaliCareApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .UseMauiCommunityToolkit()
                .UseSkiaSharp()
                .RegisterServices()
                .ConfigureMopups()
                .ConfigureMauiHandlers(handlers=> handlers.AddInputKitHandlers())
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("InterRegular.ttf", "InterRegular");
                    fonts.AddFont("InterSemiBold.ttf", "InterSemiBold");
                    fonts.AddFont("InterLight.ttf", "InterLight");
                    fonts.AddFont("InterBold.ttf", "InterBold");
                    fonts.AddMaterialIconFonts();
                })
                .RegisterViewModels()
                .RegisterViews();
#if ANDROID || IOS
            builder.UseLocalNotification();
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        public static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
        {
            builder.Services.AddDbContext<AppDbContext>(ServiceLifetime.Scoped);
            builder.Services.AddScoped<CgmDataService>();
            builder.Services.AddScoped<FirebaseService>();
            builder.Services.AddSingleton(MopupService.Instance);
            builder.Services.AddTransient<HttpProxyService>();
            builder.Services.AddScoped<AppNotificationService>();

            return builder;
        }
        public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
        {
            builder.Services.AddScoped<CGMReadingsViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<DeviceMgtViewModel>();
            builder.Services.AddScoped<LibraryViewModel>();
            builder.Services.AddTransient<PreferencesViewModel>();
            builder.Services.AddTransient<SignupViewModel>();
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddSingleton<IntroViewModel>();
            builder.Services.AddTransient<CompleteAccountViewModel>();

            return builder;
        }
        public static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<IntroView>();
            builder.Services.AddSingleton<LoginView>();
            builder.Services.AddTransient<AddDeviceView>();
            builder.Services.AddTransient<CompleteAccountView>();
            builder.Services.AddTransient<DevicesView>();
            builder.Services.AddScoped<LibraryView>();
            builder.Services.AddTransient<PreferencesView>();
            builder.Services.AddScoped<ReadingsView>();
            builder.Services.AddTransient<SignupView>();
            builder.Services.AddTransient<DashboardView>();

            return builder;
        }
    }
}
