using AutoMapper;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly IMapper _mapper;
        private readonly IDialogService _dialogService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IAppSettingsService _appSettings;
        private readonly IGoogleApiService _googleApiService;
        private readonly IUserCredentialService _userCredentialService;


        private string _currentUserName;
        private List<ItemModel> _taskLists = new List<ItemModel>();


        private readonly MvxInteraction<string> _onUserProfileImgLoaded = new MvxInteraction<string>();
        private readonly MvxInteraction _onTaskListsLoaded = new MvxInteraction();

        // need to expose it as a public property for binding (only IMvxInteraction is needed in the view)
        public IMvxInteraction<string> OnUserProfileImgLoaded => _onUserProfileImgLoaded;
        public IMvxInteraction OnTaskListsLoaded => _onTaskListsLoaded;

        public string CurrentUserName
        {
            get => _currentUserName;
            set => SetProperty(ref _currentUserName, value);
        }

        public List<ItemModel> TaskLists
        {
            get => _taskLists;
            set => SetProperty(ref _taskLists, value);
        }


        public IMvxAsyncCommand<int> OnTaskListSelectedCommand { get; private set; }


        public MenuViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IMvxNavigationService navigationService,
            IMapper mapper,
            IDialogService dialogService,
            IMiraiNotesDataService dataService,
            IAppSettingsService appSettings,
            IGoogleApiService googleApiService,
            IUserCredentialService userCredentialService)
            : base(textProvider, messenger)
        {
            _userCredentialService = userCredentialService;
            _mapper = mapper;
            _dialogService = dialogService;
            _dataService = dataService;
            _appSettings = appSettings;
            _googleApiService = googleApiService;
            _navigationService = navigationService;

            SetCommands();
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await LoadProfileInfo();

            await InitView();
        }

        private void SetCommands()
        {
            OnTaskListSelectedCommand = new MvxAsyncCommand<int>(OnTaskListSelected);
        }

        private async Task LoadProfileInfo()
        {
            var userResponse = await _dataService.UserService.GetCurrentActiveUserAsync();

            if (!userResponse.Succeed)
            {
                _dialogService.ShowErrorToast($"Current user couldnt be loaded. {userResponse.Message}");
                return;
            }

            CurrentUserName = userResponse.Result.Fullname;
            string imgPath = MiscellaneousUtils.GetUserProfileImagePath(userResponse.Result.GoogleUserID);
            _onUserProfileImgLoaded.Raise(imgPath);
        }

        private async Task InitView(bool onFullSync = false)
        {
            string selectedTaskListID;

            if (!onFullSync && _appSettings.RunSyncBackgroundTaskAfterStart)
            {
                //                _backgroundTaskManager.StartBackgroundTask(BackgroundTaskType.SYNC);
                return;
            }



            //If we have something in the init details, lets select that task list
            //            if (!onFullSync &&
            //                InitDetails is null == false &&
            //                !string.IsNullOrEmpty(InitDetails.Item1) &&
            //                !string.IsNullOrEmpty(InitDetails.Item2))
            //            {
            //                selectedTaskListID = InitDetails.Item1;
            //            }
            //            else
            //                selectedTaskListID = CurrentTaskList?.TaskListID;
            //
            ////            _messenger.Send(true, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");
            //
            //            SelectedItem =
            //                CurrentTaskList = null;
            TaskLists.Clear();
            //            TaskListsAutoSuggestBoxItems.Clear();
            //
            var dbResponse = await _dataService
                .TaskListService
                .GetAsNoTrackingAsync(
                    tl => tl.User.IsActive && tl.LocalStatus != LocalStatus.DELETED,
                    tl => tl.OrderBy(t => t.Title));

            if (!dbResponse.Succeed)
            {
                _dialogService.ShowErrorToast(
                    $"An unknown error occurred while trying to retrieve all the task lists from the db. Error = {dbResponse.Message}");
                return;
            }


            TaskLists.AddRange(_mapper.Map<List<ItemModel>>(dbResponse.Result));
            _onTaskListsLoaded.Raise();
            //
            //            TaskListsAutoSuggestBoxItems.AddRange(_mapper.Map<IEnumerable<ItemModel>>(dbResponse.Result));
            //
            //            SortTaskLists(_appSettings.DefaultTaskListSortOrder);
            //
            //            _messenger.Send(false, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");
            //            //The msg send by nav vm could take longer.. so lets way a litte bit
            //            //with that, the progress ring animation doesnt gets swallowed 
            //            await Task.Delay(500);
            //
            //            SelectedItem = TaskLists.Any(tl => tl.TaskListID == selectedTaskListID)
            //                ? TaskLists.FirstOrDefault(tl => tl.TaskListID == selectedTaskListID)
            //                : TaskLists.FirstOrDefault();
            //            //For some reason OnNavigationViewSelectionChangeAsync is not getting called
            //            //if SelectedItem is null
            //            if (SelectedItem is null)
            //                OnNavigationViewSelectionChangeAsync(SelectedItem);
        }

        private async Task OnTaskListSelected(int position)
        {
            var taskList = TaskLists[position];
            await Task.Delay(300);
            await _navigationService.Navigate<TasksViewModel, ItemModel>(taskList);
        }
    }
}