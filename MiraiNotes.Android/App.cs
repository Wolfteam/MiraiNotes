﻿using Android.App;
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
using System.Collections.Generic;
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

            var logger = new LoggerConfiguration().MinimumLevel
                .Verbose();
            var logs = new Dictionary<string, string>
            {
                //data services
                {$"{typeof(TaskListDataService).FullName}", "data_tasklist_service_.txt" },
                {$"{typeof(TaskDataService).FullName}" , "data_task_service_.txt"},
                {$"{typeof(UserDataService).FullName}", "data_user_service_.txt" },
                {$"{typeof(MiraiNotesDataService).FullName}", "data_main_service_.txt" },
                //view models
                {$"{typeof(AccountDialogViewModel).FullName}", "vm_account_dialog.txt" },
                {$"{typeof(AddSubTaskDialogViewModel).FullName}", "vm_addsubtask_dialog" },
                {$"{typeof(ChangeTaskStatusDialogViewModel).FullName}",  "vm_changetaskstatus_dialog.txt"},
                {$"{typeof(DeleteTaskDialogViewModel).FullName}",  "vm_deletetask_dialog.txt"},
                {$"{typeof(MoveTaskDialogViewModel).FullName}", "vm_movetask_dialog.txt" },
                {$"{typeof(MoveToTaskListDialogViewModel).FullName}",  "vm_movetotasklist_dialog.txt"},
                {$"{typeof(PasswordDialogViewModel).FullName}", "vm_password_dialog.txt" },
                {$"{typeof(AddEditTaskListDialogViewModel).FullName}",  "vm_add_edit_tasklists_dialog.txt"},
                {$"{typeof(TaskMenuOptionsViewModel).FullName}", "vm_taskmenuoptions_dialog.txt" },
                {$"{typeof(TaskDateDialogViewModel).FullName}",  "vm_taskdate_dialog.txt"},
                {$"{typeof(DeleteAccountDialogViewModel).FullName}",  "vm_deleteaccount_dialog.txt"},
                {$"{typeof(LogoutDialogViewModel).FullName}",  "vm_logout_dialog.txt"},
                {$"{typeof(SettingsMainViewModel).FullName}", "vm_settings_main.txt" },
                {$"{typeof(GoogleUserViewModel).FullName}",  "vm_google_user.txt"},
                {$"{typeof(LoginViewModel).FullName}", "vm_login.txt" },
                {$"{typeof(MainViewModel).FullName}", "vm_main.txt" },
                {$"{typeof(MenuViewModel).FullName}",  "vm_menu.txt"},
                {$"{typeof(NewTaskViewModel).FullName}","vm_newtask.txt"},
                {$"{typeof(TasksViewModel).FullName}",  "vm_tasks.txt"},
                {$"{typeof(DeleteTaskListDialogViewModel).FullName}",  "vm_deletetasklist_dialog.txt"},
                {$"{typeof(ManageTaskListsDialogViewModel).FullName}",  "vm_managetasklists_dialog.txt"},
                //others
                {$"{typeof(AuthenticatedHttpClientHandler).FullName}", "auth_http_handler_.txt" },
                {$"{typeof(SyncBackgroundTask).FullName}",  "bg_sync_.txt"},
                {$"{typeof(MarkTaskAsCompletedReceiver.MarkAsCompletedTask).FullName}",  "bg_marktaskascompleted_.txt"},
                {$"{typeof(GoogleApiService).FullName}", "api_google.txt" },
                {$"{typeof(SyncService).FullName}",  "sync_service_.txt"}
            };

            foreach (var kvp in logs)
            {
                logger.WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(Matching.FromSource(kvp.Key))
                    .WriteTo.File(
                        Path.Combine(basePath, kvp.Value),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        outputTemplate: fileOutputTemplate));
            }

            //for some reason, .log format doesnt work.. but no problem
            //i can use .txt or .json
            Log.Logger = logger.WriteTo.AndroidLog()
                //Sets the Tag field.
                .Enrich.WithProperty(Constants.SourceContextPropertyName, "MiraiSoft")
                .CreateLogger();

            return Log.Logger;
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

#if DEBUG
        public static void RegisterMockServices()
        {
            Mvx.IoCProvider.RegisterType<IGoogleApiService, MockedGoogleApiService>();
        }
#endif
    }
}