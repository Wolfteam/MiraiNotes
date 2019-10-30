using Android.App;
using AutoMapper;
using FluentValidation;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.GoogleApi;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Background;
using MiraiNotes.Android.Common;
using MiraiNotes.Android.Common.Validators;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Services;
using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.ViewModels.Dialogs;
using MiraiNotes.Android.ViewModels.Settings;
using MiraiNotes.Android.Views.Fragments.Dialogs;
using MiraiNotes.Shared;
using MiraiNotes.Shared.Helpers;
using MiraiNotes.Shared.Services;
using MiraiNotes.Shared.Services.Data;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Platforms.Android;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Plugin.Fingerprint;
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

            Mvx.IoCProvider.RegisterSingleton(() => SetupLogging());

            Mvx.IoCProvider.RegisterType<IAppSettingsService, AppSettingsService>();
            Mvx.IoCProvider.RegisterType<IAndroidAppSettings, AppSettingsService>();
            Mvx.IoCProvider.RegisterType<IDialogService, DialogService>();

            Mvx.IoCProvider.RegisterType<IUserCredentialService, UserCredentialService>();
            Mvx.IoCProvider.RegisterType<INetworkService, NetworkService>();
            Mvx.IoCProvider.RegisterType<ISyncService, SyncService>();
            Mvx.IoCProvider.RegisterType<IBackgroundTaskManagerService, BackgroundTaskManagerService>();
            Mvx.IoCProvider.RegisterType<INotificationService, NotificationService>();
            Mvx.IoCProvider.RegisterType<ITelemetryService, TelemetryService>();

            Mvx.IoCProvider.RegisterSingleton(CreateMapper);

            Mvx.IoCProvider.ConstructAndRegisterSingleton<IUserDataService, UserDataService>();
            Mvx.IoCProvider.ConstructAndRegisterSingleton<ITaskListDataService, TaskListDataService>();
            Mvx.IoCProvider.ConstructAndRegisterSingleton<ITaskDataService, TaskDataService>();
            Mvx.IoCProvider.ConstructAndRegisterSingleton<IMiraiNotesDataService, MiraiNotesDataService>();

            CrossFingerprint.SetCurrentActivityResolver(() =>
            {
                var top = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
                return top.Activity;
            });

            Mvx.IoCProvider.RegisterType(() => CrossFingerprint.Current);
            CrossFingerprint.SetDialogFragmentType<FingerprintCustomDialogFragment>();

            var client = new HttpClient(
                new AuthenticatedHttpClientHandler(
                    Mvx.IoCProvider.Resolve<ILogger>().ForContext<AuthenticatedHttpClientHandler>(),
                    () => Mvx.IoCProvider.Resolve<IGoogleApiService>(),
                    () => Mvx.IoCProvider.Resolve<IUserCredentialService>()))
            {
                BaseAddress = new Uri(AppConstants.BaseGoogleApiUrl)
            };

            var googleApiService = RestService.For<IGoogleApi>(client);
            Mvx.IoCProvider.RegisterSingleton(googleApiService);

            Mvx.IoCProvider.RegisterType<IGoogleApiService, GoogleApiService>();

            AssemblyScanner
                .FindValidatorsInAssembly(typeof(PasswordDialogViewModelValidator).Assembly)
                .ForEach(scanResult => Mvx.IoCProvider.RegisterType(scanResult.InterfaceType, scanResult.ValidatorType));
            Mvx.IoCProvider.RegisterType<IValidatorFactory, ValidatorFactory>();

            //since im using automapper to resolve this one, i need to explicit register it
            Mvx.IoCProvider.RegisterType<GoogleUserViewModel>();
            Mvx.IoCProvider.RegisterType<TaskItemViewModel>();
            Mvx.IoCProvider.RegisterType<TaskListItemViewModel>();

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
            var basePath = Path.Combine(externalFolder, "Logs");

            //for some reason, .log format doesnt work.. but no problem
            //i can use .txt or .json
            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                //data services
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                        Matching.FromSource($"{typeof(TaskListDataService).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "data_tasklist_service_.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                        Matching.FromSource($"{typeof(TaskDataService).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "data_task_service_.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                        Matching.FromSource($"{typeof(UserDataService).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "data_user_service_.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(MiraiNotesDataService).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "data_main_service_.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                //view models
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(AccountDialogViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_account_dialog.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(AddSubTaskDialogViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_addsubtask_dialog.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(ChangeTaskStatusDialogViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_changetaskstatus_dialog.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(DeleteTaskDialogViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_deletetask_dialog.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(MoveTaskDialogViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_movetask_dialog.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(MoveToTaskListDialogViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_movetotasklist_dialog.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(PasswordDialogViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_password_dialog.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(AddEditTaskListDialogViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_add_edit_tasklists_dialog.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(TaskMenuOptionsViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_taskmenuoptions_dialog.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(TaskDateDialogViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_taskdate_dialog.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(DeleteAccountDialogViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_deleteaccount_dialog.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(LogoutDialogViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_logout_dialog.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(SettingsMainViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_settings_main.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(GoogleUserViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_google_user.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(LoginViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_login.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(MainViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_main.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(MenuViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_menu.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(NewTaskViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_newtask.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(TasksViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_tasks.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(DeleteTaskListDialogViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_deletetasklist_dialog.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(ManageTaskListsDialogViewModel).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "vm_managetasklists_dialog.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                //others
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(AuthenticatedHttpClientHandler).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "auth_http_handler_.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(SyncBackgroundTask).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "bg_sync_.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(MarkTaskAsCompletedReceiver.MarkAsCompletedTask).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "bg_marktaskascompleted_.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(GoogleApiService).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "api_google.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(
                            Matching.FromSource($"{typeof(SyncService).FullName}"))
                    .WriteTo.File(
                        Path.Combine(basePath, "sync_service_.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate))
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