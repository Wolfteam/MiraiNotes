using MiraiNotes.Core.Enums;
using MiraiNotes.UWP.BackgroundTasks;
using MiraiNotes.UWP.Helpers;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Pages;
using MiraiNotes.UWP.Utils;
using MiraiNotes.UWP.ViewModels;
using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MiraiNotes.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private bool _initialized;
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            //var jsonSettings = new JsonSerializerSettings
            //{
            //    Formatting = Formatting.Indented,
            //    TypeNameHandling = TypeNameHandling.Objects,
            //    ContractResolver = new CamelCasePropertyNamesContractResolver(),
            //    DateFormatHandling = DateFormatHandling.IsoDateFormat,
            //    DateTimeZoneHandling = DateTimeZoneHandling.Local,
            //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            //};
            //jsonSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            //JsonConvert.DefaultSettings = () => jsonSettings;
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (e.PrelaunchActivated)
                return;
            OnLaunchedOrActivated(e);
            AfterLaunchedOrActivated();
        }

        /// <summary>
        /// Invoked when the application is activated by some means other than normal launching.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
        protected override void OnActivated(IActivatedEventArgs e)
        {
            OnLaunchedOrActivated(e);
            AfterLaunchedOrActivated();
        }

        private void OnLaunchedOrActivated(IActivatedEventArgs e)
        {
            // Initialize things like registering background tasks before the app is loaded
            BackgroundTasksManager.RegisterBackgroundTask(BackgroundTaskType.MARK_AS_COMPLETED, restart: false);
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            // Handle toast activation
            if (e is ToastNotificationActivatedEventArgs toastActivationArgs)
            {
                if (toastActivationArgs.Argument.Length != 0)
                {
                    // If we're loading the app for the first time, place the main page on the back stack
                    // so that user can go back after they've been navigated to the specific page
                    //if (rootFrame.BackStack.Count == 0)
                    //rootFrame.BackStack.Add(new PageStackEntry(typeof(LoginPage), null, null));

                    var queryParams = toastActivationArgs.Argument
                        .Split('&')
                        .ToDictionary(c => c.Split('=')[0], c => Uri.UnescapeDataString(c.Split('=')[1]));

                    var actionType = (NotificationActionType)Enum.Parse(typeof(NotificationActionType), queryParams["action"]);

                    switch (actionType)
                    {
                        case NotificationActionType.OPEN_TASK:
                            BaseViewModel.InitDetails = new Tuple<string, string>(queryParams["taskListID"], queryParams["taskID"]);
                            break;
                        case NotificationActionType.MARK_AS_COMPLETED:
                        default:
                            throw new ArgumentOutOfRangeException(nameof(actionType), actionType, "The provided toast action type is not valid");
                    }
                }
                if (rootFrame.Content == null)
                    rootFrame.Navigate(typeof(LoginPage));

                //kinda hack but works..
                if (ViewModelLocator.IsAppRunning)
                {
                    var vml = new ViewModelLocator();
                    vml.Home.PageLoadedCommand.Execute(null);
                }
            }
            // Handle launch activation
            else if (e is LaunchActivatedEventArgs launchActivationArgs)
            {
                // If launched with arguments (not a normal primary tile/applist launch)
                if (launchActivationArgs.Arguments.Length > 0)
                {
                    // TODO: Handle arguments for cases like launching from secondary Tile, so we navigate to the correct page
                    throw new NotImplementedException("Launched with arguments type of of activation is not currently implemented");
                }
                // Otherwise if launched normally
                else
                {
                    // If we're currently not on a page, navigate to the login page
                    if (rootFrame.Content == null)
                        rootFrame.Navigate(typeof(LoginPage));
                }
            }
            else
            {
                // TODO: Handle other types of activation
                throw new NotImplementedException("This type of of activation is not currently implemented");
            }
            //If the app is already initialized just return. Not sure if this is needed
            if (_initialized)
                return;

            ViewModelLocator.IsAppRunning = true;

            // Ensure the current window is active
            Window.Current.Activate();
            GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize();
            _initialized = true;
        }

        private void AfterLaunchedOrActivated()
        {
            var vml = new ViewModelLocator();
            MiscellaneousUtils.ChangeCurrentTheme(
                vml.AppSettingsService.AppTheme, 
                vml.AppSettingsService.AppHexAccentColor);
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        // Event fired when a Background Task is activated (in Single Process Model)
        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            var deferral = args.TaskInstance.GetDeferral();

            switch (args.TaskInstance.Task.Name)
            {
                case nameof(SyncBackgroundTask):
                    new SyncBackgroundTask().Run(args.TaskInstance);
                    break;
                case nameof(MarkAsCompletedBackgroundTask):
                    new MarkAsCompletedBackgroundTask().Run(args.TaskInstance);
                    break;
            }

            deferral.Complete();
        }
    }
}
