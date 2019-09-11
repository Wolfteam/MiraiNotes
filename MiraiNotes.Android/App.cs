using Android.App;
using AutoMapper;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.GoogleApi;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Services;
using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.ViewModels.Dialogs;
using MiraiNotes.Android.ViewModels.Settings;
using MiraiNotes.Shared;
using MiraiNotes.Shared.Helpers;
using MiraiNotes.Shared.Services.Data;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Refit;
using Serilog;
using Serilog.Core;
using Serilog.Filters;
using System;
using System.IO;
using System.Net.Http;

namespace MiraiNotes.Android
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            base.Initialize();

            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            Mvx.IoCProvider.RegisterSingleton<ITextProvider>(
                new ResxTextProvider(Localization.Resource.ResourceManager,
                Mvx.IoCProvider.Resolve<IMvxMessenger>()));

            var logger = SetupLogging();
            Mvx.IoCProvider.RegisterType(() => logger);

            Mvx.IoCProvider.RegisterType<IAppSettingsService, AppSettingsService>();
            Mvx.IoCProvider.RegisterType<IAndroidAppSettings, AppSettingsService>();
            Mvx.IoCProvider.RegisterType<IDialogService, DialogService>();

            Mvx.IoCProvider.RegisterType<IUserCredentialService, UserCredentialService>();
            Mvx.IoCProvider.RegisterType<INetworkService, NetworkService>();
            Mvx.IoCProvider.RegisterType<ISyncService, SyncService>();
            Mvx.IoCProvider.RegisterType(CreateMapper);

            Mvx.IoCProvider.RegisterType<IUserDataService, UserDataService>();
            Mvx.IoCProvider.RegisterType<ITaskListDataService, TaskListDataService>();
            Mvx.IoCProvider.RegisterType<ITaskDataService, TaskDataService>();
            Mvx.IoCProvider.RegisterType<IMiraiNotesDataService, MiraiNotesDataService>();

            var client = new HttpClient(
                new AuthenticatedHttpClientHandler(
                    () => Mvx.IoCProvider.Resolve<IGoogleApiService>(),
                    () => Mvx.IoCProvider.Resolve<IUserCredentialService>()))
            {
                BaseAddress = new Uri(AppConstants.BaseGoogleApiUrl)
            };

            var googleApiService = RestService.For<IGoogleApi>(client);
            Mvx.IoCProvider.RegisterSingleton(googleApiService);

            Mvx.IoCProvider.RegisterType<IGoogleApiService>(() => new GoogleApiService(
                Mvx.IoCProvider.Resolve<IGoogleApi>(),
                AppConstants.ClientId,
                string.Empty,
                AppConstants.RedirectUrl)
            );

            //since im using automapper to resolve this one, i need to explicit register it
            Mvx.IoCProvider.RegisterType<GoogleUserViewModel>();

            RegisterAppStart<LoginViewModel>();
            //RegisterCustomAppStart<CustomAppStart>();
            // if you want to use a custom AppStart, you should replace the previous line with this one:
            // RegisterCustomAppStart<MyCustomAppStart>();
        }
        //this shit is not logging to file system
        private ILogger SetupLogging()
        {
            const string fileOutputTemplate =
                "{Timestamp:dd-MM-yyyy HH:mm:ss.fff} [{Level}] {Message:lj}{NewLine}{Exception}";
            var externalFolder = Application.Context.GetExternalFilesDir(null).AbsolutePath;
            //            var path = Path.Combine(externalFolder, "log.txt");

            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                        Matching.FromSource($"{typeof(TaskListDataService).Namespace}.{nameof(TaskListDataService)}"))
                    .WriteTo.File(
                        Path.Combine(externalFolder, "Logs", "mirai_notes_tasklist_data_service_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                        Matching.FromSource($"{typeof(TaskDataService).Namespace}.{nameof(TaskDataService)}"))
                    .WriteTo.File(
                        Path.Combine(externalFolder, "Logs", "mirai_notes_task_data_service_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                        Matching.FromSource($"{typeof(UserDataService).Namespace}.{nameof(UserDataService)}"))
                    .WriteTo.File(
                        Path.Combine(externalFolder, "Logs", "mirai_notes_user_data_service_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(AccountDialogViewModel).Namespace}.{nameof(AccountDialogViewModel)}"))
                    .WriteTo.File(
                        Path.Combine(externalFolder, "Logs", "mirai_notes_account_dialog_vm_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(AccountDialogViewModel).Namespace}.{nameof(PasswordDialogViewModel)}"))
                    .WriteTo.File(
                        Path.Combine(externalFolder, "Logs", "mirai_notes_password_dialog_vm_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(AccountDialogViewModel).Namespace}.{nameof(PasswordDialogViewModel)}"))
                    .WriteTo.File(
                        Path.Combine(externalFolder, "Logs", "mirai_notes_password_dialog_vm_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(SettingsMainViewModel).Namespace}.{nameof(SettingsMainViewModel)}"))
                    .WriteTo.File(
                        Path.Combine(externalFolder, "Logs", "mirai_notes_settings_main_vm_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(MainViewModel).Namespace}.{nameof(GoogleUserViewModel)}"))
                    .WriteTo.File(
                        Path.Combine(externalFolder, "Logs", "mirai_notes_google_user_vm_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(MainViewModel).Namespace}.{nameof(LoginViewModel)}"))
                    .WriteTo.File(
                        Path.Combine(externalFolder, "Logs", "mirai_notes_login_vm_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(MainViewModel).Namespace}.{nameof(MainViewModel)}"))
                    .WriteTo.File(
                        Path.Combine(externalFolder, "Logs", "mirai_notes_main_vm_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(MainViewModel).Namespace}.{nameof(MenuViewModel)}"))
                    .WriteTo.File(
                        Path.Combine(externalFolder, "Logs", "mirai_notes_menu_vm_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(MainViewModel).Namespace}.{nameof(NewTaskViewModel)}"))
                    .WriteTo.File(
                        Path.Combine(externalFolder, "Logs", "mirai_notes_newtask_vm_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(MainViewModel).Namespace}.{nameof(TasksViewModel)}"))
                    .WriteTo.File(
                        Path.Combine(externalFolder, "Logs", "mirai_notes_tasks_vm_.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate,
                        shared: true))
                .WriteTo.AndroidLog()
                    .Enrich.WithProperty(Constants.SourceContextPropertyName, "MiraiSoft") //Sets the Tag field.
                .CreateLogger();
            Log.Logger = logger;
            return logger;
        }

        private IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                // Add all profiles in current assembly
                cfg.AddProfile<MappingProfile>();
                cfg.ConstructServicesUsing(Mvx.IoCProvider.Resolve);
            });
            //config.AssertConfigurationIsValid();
            return config.CreateMapper();
        }
    }
}