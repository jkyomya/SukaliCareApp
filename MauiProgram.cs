using InputKit.Handlers;
using Mopups.Hosting;
using Mopups.Services;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Shiny;
using Shiny.BluetoothLE;
using Shiny.Jobs;
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
                .UseShiny()
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

            return builder.Build();
        }

        public static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
        {
            builder.Services.AddScoped<CgmReadingDatabase>();
            builder.Services.AddScoped<FirebaseService>();
            builder.Services.AddSingleton(MopupService.Instance);
            builder.Services.AddTransient<HttpProxyService>();
            builder.Services.AddSingleton<HealthContentService>();
            builder.Services.AddSingleton<CommunityService>();
#if ANDROID || IOS
            builder.Services.AddScoped<AppNotificationService>();
#endif
            builder.Services.AddSingleton<IBluetoothLE>(CrossBluetoothLE.Current);
            builder.Services.AddSingleton<IAdapter>(CrossBluetoothLE.Current.Adapter);
            builder.Services.AddSingleton<IBluetoothService, BluetoothService>();
            builder.Services.AddBluetoothLE();
            builder.Services.AddScoped<BluetoothConnectionService>();

#if __MOBILE__

            //JobInfo bluetoothJob = new JobInfo(
            //    Identifier: "DataBackgroundJob",
            //    JobType: typeof(DataBackgroundJob),
            //    true,
            //    new Dictionary<string, string>
            //    {
            //        { "loops", "100" }
            //    },
            //    InternetAccess.Any,
            //    false,
            //    false,
            //    false
            //);
            //builder.Services.AddJob(bluetoothJob);

            builder.Services.AddJob(typeof(DataBackgroundJob));

#endif

            return builder;
        }
        public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
        {
            builder.Services.AddScoped<CGMReadingsViewModel>();
            builder.Services.AddSingleton<DashboardViewModel>();
            builder.Services.AddTransient<DeviceMgtViewModel>();
            builder.Services.AddScoped<AddDeviceViewModel>();
            builder.Services.AddScoped<LibraryViewModel>();
            builder.Services.AddTransient<PreferencesViewModel>();
            builder.Services.AddTransient<SignupViewModel>();
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddSingleton<IntroViewModel>();
            builder.Services.AddTransient<CompleteAccountViewModel>();
            builder.Services.AddSingleton<HealthInfoViewModel>();
            builder.Services.AddTransient<HealthArticleViewModel>();
            builder.Services.AddSingleton<CommunityViewModel>();
            builder.Services.AddTransient<GroupDetailViewModel>();

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
            builder.Services.AddSingleton<DashboardView>();
            builder.Services.AddSingleton<HealthInfoView>();
            builder.Services.AddTransient<HealthArticleView>();
            builder.Services.AddSingleton<CommunityView>();
            builder.Services.AddTransient<GroupDetailView>();

            return builder;
        }
    }
}
