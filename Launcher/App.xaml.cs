using System.Windows;
using Launcher.Models;
using Launcher.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();

            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var apiSection = configuration.GetSection("Api");
            var apiSettings = new ApiSettings
            {
                ServerSideUrl = apiSection["ServerSideUrl"] ?? ""
            };

            services.AddSingleton(apiSettings);
            services.AddSingleton(LauncherSettings.Load());
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<GameLaunchService, GameLaunchService>();
            services.AddSingleton<IJavaLocatorService, JavaLocatorService>();
            services.AddSingleton<RestService>();
            services.AddSingleton<MainWindow>();
        }
    }
}
