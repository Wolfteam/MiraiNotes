using AutoMapper;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.DataService.Interfaces;
using MiraiNotes.DataService.Services;
using MiraiNotes.UWP.Design;
using MiraiNotes.UWP.Handlers;
using MiraiNotes.UWP.Helpers;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Pages;
using MiraiNotes.UWP.Services;

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
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NavigationService>();
            }
        }

        public LoginPageViewModel Login
        {
            get
            {
                return ServiceLocator.Current.GetInstance<LoginPageViewModel>();
            }
        }

        public NavPageViewModel Home
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NavPageViewModel>();
            }
        } 

        public TasksPageViewModel Tasks
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TasksPageViewModel>();
            }
        }

        public NewTaskPageViewModel NewTask
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NewTaskPageViewModel>();
            }
        }
        #endregion

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            var navigation = new NavigationService();
            navigation.Configure(HOME_PAGE, typeof(MainPage));
            navigation.Configure(LOGIN_PAGE, typeof(LoginPage));

            var config = new MapperConfiguration(cfg =>
            {
                // Add all profiles in current assembly
                cfg.AddProfiles(GetType().Assembly);
            });
                
            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<IGoogleTaskListService, DesignGoogleTaskListService>();
                SimpleIoc.Default.Register<IGoogleTaskService, DesignGoogleTaskService>();
            }
            else
            {
                SimpleIoc.Default.Register<IGoogleTaskListService, GoogleTaskListService>();
                SimpleIoc.Default.Register<IGoogleTaskService, GoogleTaskService>();
            }
            SimpleIoc.Default.Register<IMessenger, Messenger>();
            SimpleIoc.Default.Register<ICustomDialogService, CustomDialogService>();
            SimpleIoc.Default.Register<INavigationService>(() => navigation);
            SimpleIoc.Default.Register<IUserCredentialService, UserCredentialService>();
            SimpleIoc.Default.Register(() => config.CreateMapper());

            SimpleIoc.Default.Register<AuthorizationHandler>();
            SimpleIoc.Default.Register<IHttpClientsFactory, HttpClientsFactory>();

            SimpleIoc.Default.Register<IGoogleAuthService, GoogleAuthService>();
            SimpleIoc.Default.Register<IGoogleUserService, GoogleUserService>();
            SimpleIoc.Default.Register<IGoogleApiService, GoogleApiService>();

            SimpleIoc.Default.Register<INetworkService, NetworkService>();
            SimpleIoc.Default.Register<ISyncService, SyncService>();

            SimpleIoc.Default.Register<IUserDataService, UserDataService>();
            SimpleIoc.Default.Register<ITaskListDataService, TaskListDataService>();
            SimpleIoc.Default.Register<ITaskDataService, TaskDataService>();
            SimpleIoc.Default.Register<IMiraiNotesDataService, MiraiNotesDataService>();
            
            SimpleIoc.Default.Register<LoginPageViewModel>();
            SimpleIoc.Default.Register<NavPageViewModel>();
            SimpleIoc.Default.Register<TasksPageViewModel>();
            SimpleIoc.Default.Register<NewTaskPageViewModel>();
        }
    }
}
