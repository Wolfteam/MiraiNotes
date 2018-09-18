using AutoMapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.DataService.Interfaces;
using MiraiNotes.Shared.Models;
using MiraiNotes.UWP.Extensions;
using MiraiNotes.UWP.Helpers;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MiraiNotes.UWP.ViewModels
{
    public class NavPageViewModel : ViewModelBase
    {
        #region Members
        private readonly ICustomDialogService _dialogService;
        private readonly IMessenger _messenger;
        private readonly INavigationService _navigationService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IMapper _mapper;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDispatcherHelper _dispatcher;
        private readonly IApplicationSettingsService _appSettings;

        private object _selectedItem;
        private SmartObservableCollection<TaskListItemViewModel> _taskLists = new SmartObservableCollection<TaskListItemViewModel>();
        private SmartObservableCollection<ItemModel> _taskListsAutoSuggestBoxItems = new SmartObservableCollection<ItemModel>();

        private TaskListItemViewModel _currentTaskList;

        private string _taskListAutoSuggestBoxText;

        private bool _isPaneOpen;
        private bool _showMainProgressBar;
        private string _mainProgressBarText;
        public bool _isSettingsPaneOpen;
        private bool _isSelectionInProgress;
        #endregion

        #region Properties
        public object SelectedItem
        {
            get { return _selectedItem; }
            set { Set(ref _selectedItem, value); }
        }

        public SmartObservableCollection<TaskListItemViewModel> TaskLists
        {
            get { return _taskLists; }
            set { Set(ref _taskLists, value); }
        }

        public SmartObservableCollection<ItemModel> TaskListsAutoSuggestBoxItems
        {
            get { return _taskListsAutoSuggestBoxItems; }
            set { Set(ref _taskListsAutoSuggestBoxItems, value); }
        }

        public TaskListItemViewModel CurrentTaskList
        {
            get { return _currentTaskList; }
            set { Set(ref _currentTaskList, value); }
        }

        public string TaskListkAutoSuggestBoxText
        {
            get { return _taskListAutoSuggestBoxText; }
            set { Set(ref _taskListAutoSuggestBoxText, value); }
        }

        public bool IsPaneOpen
        {
            get { return _isPaneOpen; }
            set { Set(ref _isPaneOpen, value); }
        }

        public bool ShowMainProgressBar
        {
            get { return _showMainProgressBar; }
            set { Set(ref _showMainProgressBar, value); }
        }

        public string MainProgressBarText
        {
            get { return _mainProgressBarText; }
            set { Set(ref _mainProgressBarText, value); }
        }

        public bool IsSettingsPaneOpen
        {
            get { return _isSettingsPaneOpen; }
            set { Set(ref _isSettingsPaneOpen, value); }
        }
        #endregion

        #region Commands
        public ICommand PageLoadedCommand { get; set; }

        public ICommand TaskListAutoSuggestBoxTextChangedCommand { get; set; }

        public ICommand TaskListAutoSuggestBoxQuerySubmittedCommand { get; set; }

        public ICommand NavigationViewSelectionChanged { get; set; }

        public ICommand LogoutCommand { get; set; }

        public ICommand UpdateTaskListCommand { get; set; }

        public ICommand DeleteTaskListCommand { get; set; }

        public ICommand OpenPaneCommand { get; set; }

        public ICommand ClosePaneCommand { get; set; }

        public ICommand OpenSettingsCommand { get; set; }
        #endregion

        public NavPageViewModel(
            ICustomDialogService dialogService,
            INavigationService navigationService,
            IMessenger messenger,
            IUserCredentialService userCredentialService,
            IMapper mapper,
            IMiraiNotesDataService dataService,
            IDispatcherHelper dispatcher,
            IApplicationSettingsService appSettings)
        {
            _dialogService = dialogService;
            _messenger = messenger;
            _navigationService = navigationService;
            _userCredentialService = userCredentialService;
            _mapper = mapper;
            _dataService = dataService;
            _dispatcher = dispatcher;
            _appSettings = appSettings;

            RegisterMessages();
            SetCommands();
        }

        #region Methods
        private void RegisterMessages()
        {
            _messenger.Register<TaskListItemViewModel>(
                this,
                $"{MessageType.TASK_LIST_ADDED}",
                OnTaskListAdded);
            _messenger.Register<bool>(this, $"{MessageType.OPEN_PANE}", (open) => OpenPane(open));
            _messenger.Register<bool>(
                this,
                $"{MessageType.ON_FULL_SYNC}",
                async (_) => await InitViewAsync(true));
            _messenger.Register<Tuple<bool, string>>(
                this,
                $"{MessageType.SHOW_MAIN_PROGRESS_BAR}",
                (tuple) => ShowLoading(tuple.Item1, tuple.Item2));
            _messenger.Register<TaskListSortType>(
                this, 
                $"{MessageType.DEFAULT_TASK_LIST_SORT_ORDER_CHANGED}", 
                SortTaskLists);
        }

        private void SetCommands()
        {
            PageLoadedCommand = new RelayCommand
                (async () => await InitViewAsync());

            TaskListAutoSuggestBoxTextChangedCommand = new RelayCommand<string>
                ((text) => OnTaskListAutoSuggestBoxTextChange(text));

            TaskListAutoSuggestBoxQuerySubmittedCommand = new RelayCommand<ItemModel>
                ((selectedItem) => OnTaskListAutoSuggestBoxQuerySubmitted(selectedItem));

            NavigationViewSelectionChanged = new RelayCommand<object>
                ((selectedItem) => OnNavigationViewSelectionChangeAsync(selectedItem));

            UpdateTaskListCommand = new RelayCommand<TaskListItemViewModel>
                (async (taskList) => await UpdateTaskListAsync(taskList));

            DeleteTaskListCommand = new RelayCommand<TaskListItemViewModel>
                (async (taskList) => await DeleteTaskList(taskList));

            LogoutCommand = new RelayCommand
                (async () => await LogoutAsync());

            OpenPaneCommand = new RelayCommand(() => OpenPane(true));

            ClosePaneCommand = new RelayCommand(() => OpenPane(false));

            OpenSettingsCommand = new RelayCommand(() =>
            {
                IsSettingsPaneOpen = true;
            });
        }

        private async Task InitViewAsync(bool onFullSync = false)
        {
            _messenger.Send(true, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");

            string selectedTaskListID = CurrentTaskList?.TaskListID;

            SelectedItem =
                CurrentTaskList = null;
            TaskLists.Clear();
            TaskListsAutoSuggestBoxItems.Clear();

            var dbResponse = await _dataService
                .TaskListService
                .GetAsNoTrackingAsync(
                    tl => tl.User.IsActive && tl.LocalStatus != LocalStatus.DELETED,
                    tl => tl.OrderBy(t => t.Title));

            if (!dbResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An unknown error occurred while trying to retrieve all the task lists from the db. Error = {dbResponse.Message}");
                return;
            }

            TaskLists.AddRange(_mapper.Map<IEnumerable<TaskListItemViewModel>>(dbResponse.Result));

            TaskListsAutoSuggestBoxItems.AddRange(_mapper.Map<IEnumerable<ItemModel>>(dbResponse.Result));

            SortTaskLists(_appSettings.DefaultTaskListSortOrder);

            _messenger.Send(false, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");
            //The msg send by nav vm could take longer.. so lets way a litte bit
            //with that, the progress ring animation doesnt gets swallowed 
            await Task.Delay(500);

            if (onFullSync && TaskLists.Any(tl => tl.TaskListID == selectedTaskListID))
                SelectedItem = TaskLists.FirstOrDefault(tl => tl.TaskListID == selectedTaskListID);
            else
                SelectedItem = TaskLists.FirstOrDefault();
        }

        public void OnTaskListAutoSuggestBoxTextChange(string currentText)
        {
            var filteredItems = string.IsNullOrEmpty(currentText) ?
            _mapper.Map<IEnumerable<ItemModel>>(TaskLists
                .OrderBy(t => t.Title)
                .Take(10)) :
            _mapper.Map<IEnumerable<ItemModel>>(TaskLists
                .Where(t => t.Title.ToLowerInvariant().Contains(currentText.ToLowerInvariant()))
                .OrderBy(t => t.Title)
                .Take(10));

            TaskListsAutoSuggestBoxItems.Clear();
            TaskListsAutoSuggestBoxItems.AddRange(filteredItems);
        }

        public void OnTaskListAutoSuggestBoxQuerySubmitted(ItemModel selectedItem)
        {
            if (selectedItem == null)
                return;
            SelectedItem = TaskLists.FirstOrDefault(t => t.TaskListID == selectedItem.ItemID);
        }

        public void OnNavigationViewSelectionChangeAsync(object selectedItem)
        {
            if (_isSelectionInProgress)
                return;

            if (selectedItem is null)
            {
                CurrentTaskList = null;
                SelectedItem = null;
                _messenger.Send(CurrentTaskList, $"{MessageType.NAVIGATIONVIEW_SELECTION_CHANGED}");
            }
            else if (selectedItem is TaskListItemViewModel taskList)
            {
                CurrentTaskList = taskList;
                _messenger.Send(taskList, $"{MessageType.NAVIGATIONVIEW_SELECTION_CHANGED}");
            }
            TaskListkAutoSuggestBoxText = string.Empty;
        }

        public void OnTaskListAdded(TaskListItemViewModel taskList)
        {
            TaskLists.Add(taskList);
            SelectedItem = taskList;
        }

        public async Task LogoutAsync()
        {
            bool logout = await _dialogService.ShowConfirmationDialogAsync(
                "Confirmation",
                "Are you sure you want to log out?",
                "Yes",
                "No");
            if (logout)
            {
                //TODO: DELETE ALL !!
                //delete all from the db
                //delete user settings
                //delete all view models
                OpenPane(false);
                ShowLoading(true, "Logging out... Please wait..");
                BackgroundTasksManager.UnregisterBackgroundTask();
                await _dataService
                    .UserService
                    .ChangeCurrentUserStatus(false);

                _userCredentialService.DeleteUserCredentials();
                _navigationService.GoBack();
                ShowLoading(false);
            }
        }

        public async Task UpdateTaskListAsync(TaskListItemViewModel taskList)
        {
            int index = TaskLists.IndexOf(taskList);
            string newTitle = await _dialogService.ShowInputStringDialogAsync(
                "Type the new task list name",
                taskList.Title,
                "Update",
                "Cancel");

            if (string.IsNullOrEmpty(newTitle))
                return;

            _messenger.Send(true, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");

            var dbResponse = await _dataService
                .TaskListService
                .FirstOrDefaultAsNoTrackingAsync(t => t.GoogleTaskListID == taskList.TaskListID);

            if (!dbResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An unknown error occurred while trying to retrieve the task list from db. Error = {dbResponse.Message}");
                return;
            }

            dbResponse.Result.Title = newTitle;
            dbResponse.Result.UpdatedAt = DateTime.Now;
            dbResponse.Result.ToBeSynced = true;
            if (dbResponse.Result.LocalStatus != LocalStatus.CREATED)
                dbResponse.Result.LocalStatus = LocalStatus.UPDATED;

            var response = await _dataService
                .TaskListService
                .UpdateAsync(dbResponse.Result);

            if (!response.Succeed)
            {
                _messenger.Send(false, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't update the task list {taskList.Title}. Error = {response.Message}");
                return;
            }

            TaskLists[index] = _mapper.Map<TaskListItemViewModel>(dbResponse.Result);

            _messenger.Send(false, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");
            //If the updated task is the same as the selected one
            //we update SelectedItem
            if (SelectedItem == taskList)
                SelectedItem = TaskLists[index];
        }

        public async Task DeleteTaskList(TaskListItemViewModel taskList)
        {
            bool deleteCurrentTaskList = await _dialogService.ShowConfirmationDialogAsync(
                "Confirmation",
                $"Are you sure you want to delete {taskList.Title}?",
                "Yes",
                "No");

            if (!deleteCurrentTaskList)
                return;

            _messenger.Send(true, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");

            //If we are deleting the tasklist that is selected
            //lets close the panel to avoid creating new tasks
            //in the CurrentTaskList
            if (CurrentTaskList == taskList)
                OpenPane(false);

            var dbResponse = await _dataService
                .TaskListService
                .FirstOrDefaultAsNoTrackingAsync(tl => tl.GoogleTaskListID == taskList.TaskListID);
            if (!dbResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An unknown error occurred while trying to retrieve the task list from db. Error = {dbResponse.Message}");
                return;
            }

            EmptyResponse response;
            //if the task is created but wasnt synced, we remove it from the db
            if (dbResponse.Result.LocalStatus == LocalStatus.CREATED)
            {
                response = await _dataService
                    .TaskListService
                    .RemoveAsync(dbResponse.Result);
            }
            else
            {
                dbResponse.Result.LocalStatus = LocalStatus.DELETED;
                dbResponse.Result.ToBeSynced = true;

                response = await _dataService
                    .TaskListService
                    .UpdateAsync(dbResponse.Result);
            }

            if (!response.Succeed)
            {
                _messenger.Send(false, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't delete the task list {taskList.Title}. Error = {response.Message}");
                return;
            }

            //Here we select the previoues task list only if the 
            //selected item = the task list to be removed
            if (SelectedItem == taskList)
            {
                int removedIndex = TaskLists.IndexOf(taskList);
                if (removedIndex != -1 && removedIndex > 0)
                    SelectedItem = TaskLists[removedIndex - 1];
                else
                    OnNavigationViewSelectionChangeAsync(null);
            }
            else if (TaskLists.Count == 1)
                OnNavigationViewSelectionChangeAsync(null);
            TaskLists.Remove(taskList);

            _messenger.Send(false, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");

            _messenger.Send(
                $"Sucessfully removed {taskList.Title} task list",
                $"{MessageType.SHOW_IN_APP_NOTIFICATION}");
        }

        private void OpenPane(bool isPaneOpen) => IsPaneOpen = isPaneOpen;

        private void ShowLoading(bool show, string message = null)
        {
            ShowMainProgressBar = show;
            MainProgressBarText = message;
        }

        private void SortTaskLists(TaskListSortType sortType)
        {
            if (TaskLists is null || TaskLists.Count == 0)
                return;

            _isSelectionInProgress = true;
            switch (sortType)
            {
                case TaskListSortType.BY_NAME_ASC:
                    TaskLists.SortBy(tl => tl.Title);
                    break;
                case TaskListSortType.BY_NAME_DESC:
                    TaskLists.SortByDescending(tl => tl.Title);
                    break;
                case TaskListSortType.BY_UPDATED_DATE_ASC:
                    TaskLists.SortBy(tl => tl.UpdatedAt);
                    break;
                case TaskListSortType.BY_UPDATED_DATE_DESC:
                    TaskLists.SortByDescending(tl => tl.UpdatedAt);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sortType), sortType, "The provided task list sort type does not exists");
            }
            _isSelectionInProgress = false;
        }
        #endregion
    }
}