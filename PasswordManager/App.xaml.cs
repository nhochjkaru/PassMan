using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using PasswordManager.Authorization.Brokers;
using PasswordManager.Authorization.Holders;
using PasswordManager.Authorization.Responses;
using PasswordManager.Clouds.Services;
using PasswordManager.Helpers;
using PasswordManager.Hotkeys;
using PasswordManager.RestApiHelper;
using PasswordManager.Services;
using PasswordManager.Settings;
using PasswordManager.ViewModels;
using PasswordManager.Views;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static byte[] credSt { get; set; }
        public static string userName { get; set; }
        public static string searchtext { get; set; } = "";
        public static string apitoken { get; set; }
        private readonly Mutex _mutex;
        private static IConfiguration _configuration;

        private Logger _logger;
        private TrayIcon _trayIcon;

        public IHost Host { get; private set; }
        private bool IsFirstInstance { get; }

        public App()
        {
            _mutex = new Mutex(true, "PurplePassword_CBD9AADE-1A82-48A2-9F7F-4F0EAAABEA30", out bool isFirstInstance);
            IsFirstInstance = isFirstInstance;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.Error(e.Exception, "Dispatcher unhandled exception");
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger?.Error(e.ExceptionObject as Exception, "Domain unhandled exception");
        }
      
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            InitializeComponent();
            bool isStartup = false;
            if (e.Args.Length > 0)
            {
                if(e.Args[0].ToString().ToLower() == "autostart")
                {
                    isStartup = true;
                }    
            }
            // Override culture
            //PasswordManager.Language.Properties.Resources.Culture = new System.Globalization.CultureInfo("en-US");

            if (IsFirstInstance)
            {
                // Welcome window
                var welcomeWindow = new WelcomeWindow();
                if (!isStartup) { welcomeWindow.Show(); };

                _configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                _logger = LogManager.Setup()
                    .LoadConfigurationFromSection(_configuration)
                    .GetCurrentClassLogger();

                Host = CreateHostBuilder().Build();
                _logger.Info("Log session started!");


                //Autoupdate
                
               var updaterService = Host.Services.GetService<AutoUpdaterService>();
                updaterService.update();
                // Resolve theme
                var themeService = Host.Services.GetService<ThemeService>();
                themeService.Init();

                // Create tray icon
                _trayIcon = new TrayIcon();
                bool validtoken = false;
                using (var checkTokenScope = Host.Services.CreateScope())
                {
                    try
                    {
                        var _tokenHolder = Host.Services.GetService<RestApiTokenHolder>();
                        var _callApi = Host.Services.GetService<CallApi>();
                        ApiTokenResponse token = _tokenHolder.accesstoken;
                        if (token != null)
                        {
                            App.apitoken = token.AccessToken;
                            try
                            {
                                var task = Task.Run(async () => await _callApi.Get("api/v1.1/Authentication/ValidToken"));
                                App.userName = task.Result;
                                Constants.PasswordsFilePath= Path.Combine(Constants.LocalAppDataDirectoryPath, App.userName+Constants.PasswordsFileName);
                                if (_callApi.IsSuccessStatusCode)
                                {
                                    validtoken = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
                                System.Windows.Application.Current.Shutdown();
                            }
                        }
                    }catch (Exception ex) { validtoken = false; }
                    if(!validtoken && isStartup)
                    {

                    }    
                }// Login
                using (var loginScope = Host.Services.CreateScope())
                {
                    if (!validtoken)
                    {
                        var userloginWindow = Host.Services.GetService<UserLoginWindow>();
                        
                        bool? userdialogResult = userloginWindow.ShowDialog(); // Stop here

                        if (userdialogResult != true)
                        {
                            Shutdown();
                            return;
                        }
                        userloginWindow.Close();
                    }
                    if(welcomeWindow.IsVisible)
                    {
                        welcomeWindow.Close();
                    }    
                    
                    
                    var loginWindow = Host.Services.GetService<LoginWindow>();
                    
                    bool? dialogResult = loginWindow.ShowDialog(); // Stop here

                    if (dialogResult != true)
                    {
                        Shutdown();
                        return;
                    }
                }

                // Open main window
                var mainWindow = Host.Services.GetService<MainWindow>();
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }

        private IHostBuilder CreateHostBuilder() =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // NLog
                services.AddLogging(lb =>
                {
                    lb.ClearProviders();
                    lb.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    lb.AddNLog(_configuration);
                });

                services.AddHttpClient();

                // Clouds
                // Google
                services.Configure<GoogleDriveConfig>(_configuration.GetSection("Settings:GoogleDriveConfig"));
                services.Configure<ApiConfig>(_configuration.GetSection("Settings:ApiConfig"));
                services.AddTransient<CallApi>();
                
                services.AddTransient<GoogleAuthorizationBroker>();
                services.AddTransient<GoogleDriveTokenHolder>();
                services.AddTransient<GoogleDriveCloudService>();
                services.AddTransient<RestApiCloudService>();
                services.AddTransient<CryptoService>();
                services.AddSingleton<CloudServiceProvider>();
                services.AddSingleton<RestApiTokenHolder>();

                //updater
                
                services.AddSingleton<PasswordGenerater>();
                services.AddSingleton<AutoUpdaterService>();
                //Key detect

                services.AddSingleton<UserActivityHook>();
                // Windows
                services.AddScoped<LoginWindow>();
                services.AddScoped<LoginWindowViewModel>();

                services.AddScoped<UserLoginWindow>();
                services.AddScoped<UserLoginWindowViewModel>();

                services.AddScoped<MainWindow>();
                services.AddScoped<MainWindowViewModel>();
                services.AddScoped<PasswordsViewModel>();
                services.AddScoped<CloudSyncViewModel>();
                services.AddScoped<TPCloudSyncViewModel>();
                services.AddScoped<SettingsViewModel>();
                services.AddScoped<CredentialsDialogViewModel>();

                services.AddTransient<PopupControl>();
                services.AddSingleton<PopupViewModel>();

                // Main services
                services.AddSingleton<CredentialsCryptoService>();
                services.AddSingleton<ThemeService>();
                services.AddSingleton<AppSettingsService>();
                services.AddSingleton<SyncService>();
                services.AddSingleton<FavIconService>();
                services.AddSingleton<HotkeysService>();
                services.AddSingleton<ImageService>();
                services.AddSingleton<CredentialViewModelFactory>();
            });

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _logger?.Info($"The application is shutting down...{Environment.NewLine}");
        }
    }
}
