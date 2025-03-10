using Acr.UserDialogs;
using Microsoft.Extensions.Logging;
using RATBVMaui.Services;
using Refit;

namespace RATBVMaui;

public static class MauiProgram
{
    #region Fields

    private static readonly bool _isWebApiServerLocal = false;
    private static readonly bool _isSQLiteDatabaseInMemory = false;

    #endregion

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .RegisterApis()
            .RegisterSQLight()
            .RegisterServices()
            .RegisterViewModels()
            .RegisterViews()
            .ConfigureEssentials()
            .UseUserDialogs();
            //.UseMauiCommunityToolkit()

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static MauiAppBuilder RegisterApis(this MauiAppBuilder builder)
    {
        // Apis
        if (_isWebApiServerLocal)
        {
            builder.Services.AddSingleton<IHttpServiceOptions, LocalHttpServiceOptions>();
            builder.Services.AddSingleton<ICustomHttpMessageHandler, InsecureHttpMessageHandler>();

        }
        else
        {
            builder.Services.AddSingleton<IHttpServiceOptions, DefaultHttpServiceOptions>();
            builder.Services.AddSingleton<ICustomHttpMessageHandler, DefaultHttpMessageHandler>();
        }

        builder.Services.AddSingleton<IHttpService, HttpService>();

        builder.Services.AddSingleton<IBusApi>(sp =>
        {
            var httpService = sp.GetRequiredService<IHttpService>();
            return RestService.For<IBusApi>(httpService.HttpClient);
        });

        return builder;
    }

    private static MauiAppBuilder RegisterSQLight(this MauiAppBuilder builder)
    {
        // SQL services
        if (_isSQLiteDatabaseInMemory)
        {
            builder.Services.AddSingleton<ISQLiteConnectionFactory, InMemorySQLiteConnectionFactory>();
        }
        else
        {
            builder.Services.AddSingleton<ISQLiteConnectionFactory, DefaultSQLiteConnectionFactory>();
        }

        builder.Services.AddSingleton<ISQLiteAsyncConnection>(sp =>
        {
            var sqliteFactory = sp.GetRequiredService<ISQLiteConnectionFactory>();
            return sqliteFactory.GetAsyncSqlConnection();
        });

        return builder;
    }

    private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
    {
        // UI services
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
        builder.Services.AddSingleton<IBusDataService, BusDataService>();
        builder.Services.AddSingleton<IBusRepository, BusRepository>();

        return builder;
    }

    private static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<ViewModels.BusLinesViewModel>();
        builder.Services.AddTransient<ViewModels.BusStationsViewModel>();
        builder.Services.AddTransient<ViewModels.BusTimetablesViewModel>();

        return builder;
    }

    private static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<Views.BusLinesPage>();

        return builder;
    }
}
