using AutoMapper;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.GoogleApi;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Shared;
using MiraiNotes.Shared.Helpers;
using MiraiNotes.Shared.Services;
using MiraiNotes.Shared.Services.Data;
using MiraiNotes.UWP.BackgroundTasks;
using MiraiNotes.UWP.Design;
using MiraiNotes.UWP.Handlers;
using MiraiNotes.UWP.Helpers;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Pages;
using MiraiNotes.UWP.Services;
using MiraiNotes.UWP.Utils;
using MiraiNotes.UWP.ViewModels.Dialogs;
using Refit;
using Serilog;
using Serilog.Filters;
using System;
using System.IO;
using System.Net.Http;

namespace MiraiNotes.UWP.ViewModels
{
    public class ViewModelLocator
    {
        #region Constants
        public const string HOME_PAGE = "HomePage";
        public const string LOGIN_PAGE = "LoginPage";
        #endregion

        #region Properties
        public NavigationService NavigationService
            => ServiceLocator.Current.GetInstance<NavigationService>();

        public LoginPageViewModel Login
            => ServiceLocator.Current.GetInstance<LoginPageViewModel>();

        public NavPageViewModel Home
            => ServiceLocator.Current.GetInstance<NavPageViewModel>();

        public TasksPageViewModel Tasks
            => ServiceLocator.Current.GetInstance<TasksPageViewModel>();

        public NewTaskPageViewModel NewTask
            => ServiceLocator.Current.GetInstance<NewTaskPageViewModel>();

        public SettingsPageViewModel Settings
            => ServiceLocator.Current.GetInstance<SettingsPageViewModel>();

        public SettingsPasswordDialogViewModel SettingsPasswordDialog
            => ServiceLocator.Current.GetInstance<SettingsPasswordDialogViewModel>();

        public LoginPasswordDialogViewModel LoginPasswordDialog
            => ServiceLocator.Current.GetInstance<LoginPasswordDialogViewModel>();

        public AccountsDialogViewModel AccountsDialog
            => ServiceLocator.Current.GetInstance<AccountsDialogViewModel>();

        public IAppSettingsService AppSettingsService
            => ServiceLocator.Current.GetInstance<IAppSettingsService>();

        public ISyncService SyncService
            => ServiceLocator.Current.GetInstance<ISyncService>();

        public ILogger Logger
            => ServiceLocator.Current.GetInstance<ILogger>();

        public IMapper Mapper
            => ServiceLocator.Current.GetInstance<IMapper>();

        public IMessenger Messenger
            => ServiceLocator.Current.GetInstance<IMessenger>();

        public IMiraiNotesDataService DataService
            => ServiceLocator.Current.GetInstance<IMiraiNotesDataService>();

        public IUserCredentialService UserCredentialService
            => ServiceLocator.Current.GetInstance<IUserCredentialService>();

        public INotificationService NotificationService
            => ServiceLocator.Current.GetInstance<INotificationService>();

        public ITelemetryService TelemetryService
            => ServiceLocator.Current.GetInstance<ITelemetryService>();

        public Abstractions.Services.IGoogleApiService GoogleApiService
            => ServiceLocator.Current.GetInstance<IGoogleApiService>();

        public static bool IsAppRunning { get; set; }
        #endregion

        public ViewModelLocator()
        {
            if (IsAppDependenciesRegistered())
                return;

            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register(SetupLogging);
            SimpleIoc.Default.Register<IMessenger, Messenger>();
            SimpleIoc.Default.Register<IDispatcherHelper, DispatcherHelperEx>();
            SimpleIoc.Default.Register<ICustomDialogService, CustomDialogService>();
            SimpleIoc.Default.Register(SetupNavigation);
            SimpleIoc.Default.Register<IUserCredentialService, UserCredentialService>();
            SimpleIoc.Default.Register(SetupMapper);

            SimpleIoc.Default.Register<AuthorizationHandler>();

            SimpleIoc.Default.Register<IApplicationSettingsServiceBase, ApplicationSettingsServiceBase>();
            SimpleIoc.Default.Register<IAppSettingsService, AppSettingsService>();
            SimpleIoc.Default.Register<IBackgroundTaskManagerService, BackgroundTaskManagerService>();

            if (ViewModelBase.IsInDesignModeStatic)
                SimpleIoc.Default.Register<IGoogleApiService, DesignGoogleApiService>();
            else
                SimpleIoc.Default.Register<IGoogleApiService, GoogleApiService>();

            var handler = new AuthenticatedHttpClientHandler(
                Logger.ForContext<AuthenticatedHttpClientHandler>(),
                () => GoogleApiService,
                () => UserCredentialService);
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(AppConstants.BaseGoogleApiUrl)
            };

            var googleApiService = RestService.For<IGoogleApi>(client);
            SimpleIoc.Default.Register(() => googleApiService);

            SimpleIoc.Default.Register<INetworkService, NetworkService>();
            SimpleIoc.Default.Register<ISyncService, SyncService>();

            SimpleIoc.Default.Register<IUserDataService, UserDataService>();
            SimpleIoc.Default.Register<ITaskListDataService, TaskListDataService>();
            SimpleIoc.Default.Register<ITaskDataService, TaskDataService>();
            SimpleIoc.Default.Register<IMiraiNotesDataService, MiraiNotesDataService>();

            SimpleIoc.Default.Register<INotificationService, NotificationService>();
            SimpleIoc.Default.Register<ITelemetryService, TelemetryService>();

            SimpleIoc.Default.Register<LoginPageViewModel>();
            SimpleIoc.Default.Register<NavPageViewModel>();
            SimpleIoc.Default.Register<TasksPageViewModel>();
            SimpleIoc.Default.Register<NewTaskPageViewModel>();
            SimpleIoc.Default.Register<SettingsPageViewModel>();

            SimpleIoc.Default.Register<AccountsDialogViewModel>();
            SimpleIoc.Default.Register<SettingsPasswordDialogViewModel>();
            SimpleIoc.Default.Register<LoginPasswordDialogViewModel>();

            SimpleIoc.Default.Register<GoogleUserViewModel>();
        }

        /// <summary>
        /// By using <see cref="ServiceLocator"/> we know if the LocationProviderSet
        /// has been set, if it is, it means that the app is already running, otherwise
        /// it is not
        /// </summary>
        /// <returns>True in case the app is already running</returns>
        public static bool IsAppDependenciesRegistered()
            => ServiceLocator.IsLocationProviderSet;

        private INavigationService SetupNavigation()
        {
            var navigation = new NavigationService();
            navigation.Configure(HOME_PAGE, typeof(MainPage));
            navigation.Configure(LOGIN_PAGE, typeof(LoginPage));
            return navigation;
        }

        private ILogger SetupLogging()
        {
            const string fileOutputTemplate = "{Timestamp:dd-MM-yyyy HH:mm:ss.fff} [{Level}] {Message:lj}{NewLine}{Exception}";
            string basePath = Path.Combine(MiscellaneousUtils.GetApplicationPath(), "Logs");

            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(Matching.FromSource(typeof(NavPageViewModel).Namespace))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(Matching.FromSource($"{typeof(SyncService).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "sync_service_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(Matching.FromSource($"{typeof(TaskListDataService).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "data_tasklist_service_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(Matching.FromSource($"{typeof(TaskDataService).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "data_task_service_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(Matching.FromSource($"{typeof(UserDataService).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "data_user_service_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(Matching.FromSource($"{typeof(MiraiNotesDataService).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "data_main_service_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(Matching.FromSource($"{typeof(SyncBackgroundTask).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "bg_sync_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(Matching.FromSource($"{typeof(MarkAsCompletedBackgroundTask).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "bg_marktaskascompleted_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(Matching.FromSource($"{typeof(AuthenticatedHttpClientHandler).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "auth_http_handler_.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .CreateLogger();
            Log.Logger = logger;
            return logger;
        }

        private IMapper SetupMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                // Add all profiles in current assembly
                cfg.AddProfile<MappingProfile>();
                cfg.ConstructServicesUsing(t =>
                {
                    //ConstructServicesUsing gets called if you used it in the
                    //mapping profile
                    if (t == typeof(GoogleUserViewModel))
                    {
                        return SimpleIoc.Default.GetInstanceWithoutCaching(t);
                    }
                    return SimpleIoc.Default.GetInstance(t);
                });
            });

            return config.CreateMapper();
        }
    }
}
