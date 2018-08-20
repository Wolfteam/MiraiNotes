using AutoMapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.DataService.Interfaces;
using MiraiNotes.Shared.Models;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using System;
using System.Collections.ObjectModel;
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
        private readonly INetworkService _networkService;

        private object _selectedItem;
        private ObservableCollection<TaskListItemViewModel> _taskLists;
        private ObservableCollection<ItemModel> _taskListsAutoSuggestBoxItems;

        private TaskListItemViewModel _currentTaskList;

        private string _taskListAutoSuggestBoxText;

        private bool _isPaneOpen;
        #endregion

        #region Properties
        public object SelectedItem
        {
            get { return _selectedItem; }
            set { Set(ref _selectedItem, value); }
        }

        public ObservableCollection<TaskListItemViewModel> TaskLists
        {
            get { return _taskLists; }
            set { Set(ref _taskLists, value); }
        }

        public ObservableCollection<ItemModel> TaskListsAutoSuggestBoxItems
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
        #endregion

        public NavPageViewModel(
            ICustomDialogService dialogService,
            INavigationService navigationService,
            IMessenger messenger,
            IUserCredentialService userCredentialService,
            IMapper mapper,
            IMiraiNotesDataService dataService,
            INetworkService networkService)
        {
            _dialogService = dialogService;
            _messenger = messenger;
            _navigationService = navigationService;
            _userCredentialService = userCredentialService;
            _mapper = mapper;
            _dataService = dataService;
            _networkService = networkService;

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
        }

        private void SetCommands()
        {
            PageLoadedCommand = new RelayCommand
                (async () => await InitViewAsync());

            TaskListAutoSuggestBoxTextChangedCommand = new RelayCommand<string>
                ((text) => OnTaskListAutoSuggestBoxTextChangeAsync(text));

            TaskListAutoSuggestBoxQuerySubmittedCommand = new RelayCommand<ItemModel>
                ((selectedItem) => OnTaskListAutoSuggestBoxQuerySubmittedAsync(selectedItem));

            NavigationViewSelectionChanged = new RelayCommand<object>
                (async (selectedItem) => await OnNavigationViewSelectionChangeAsync(selectedItem));

            UpdateTaskListCommand = new RelayCommand<TaskListItemViewModel>
                (async (taskList) => await UpdateTaskListAsync(taskList));

            DeleteTaskListCommand = new RelayCommand<TaskListItemViewModel>
                (async (taskList) => await DeleteTaskList(taskList));

            LogoutCommand = new RelayCommand(async () => await LogoutAsync());

            OpenPaneCommand = new RelayCommand(() => OpenPane(true));
            ClosePaneCommand = new RelayCommand(() => OpenPane(false));

            //TODO: TaskAutoSuggestBoxItems should be updated when a Task list is added/updated/modified
        }

        private async Task InitViewAsync()
        {
            _messenger.Send(true, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");

            var currentUser = await _dataService
                .UserService
                .GetCurrentActiveUserAsync();

            if (currentUser == null)
            {
                _messenger.Send(false, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");
                await _dialogService
                    .ShowMessageDialogAsync("Error", "The current active user couldnt be found on db");
                return;
            }

            var taskLists = await _dataService
                .TaskListService
                .GetAsNoTrackingAsync(
                    tl => tl.User == currentUser && tl.LocalStatus != LocalStatus.DELETED,
                    tl => tl.OrderBy(t => t.Title));
            
            TaskLists = _mapper.Map<ObservableCollection<TaskListItemViewModel>>(taskLists);

            TaskListsAutoSuggestBoxItems = _mapper.Map<ObservableCollection<ItemModel>>(taskLists);

            SelectedItem = TaskLists.FirstOrDefault();

            _messenger.Send(false, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");
        }

        public void OnTaskListAutoSuggestBoxTextChangeAsync(string currentText)
        {
            var filteredItems = string.IsNullOrEmpty(currentText) ?
            _mapper.Map<ObservableCollection<ItemModel>>(TaskLists
                .OrderBy(t => t.Title)
                .Take(10)) :
            _mapper.Map<ObservableCollection<ItemModel>>(TaskLists
                .Where(t => t.Title.ToLowerInvariant().Contains(currentText.ToLowerInvariant()))
                .OrderBy(t => t.Title)
                .Take(10));

            TaskListsAutoSuggestBoxItems = filteredItems;
        }

        public void OnTaskListAutoSuggestBoxQuerySubmittedAsync(ItemModel selectedItem)
        {
            if (selectedItem == null)
                return;
            SelectedItem = TaskLists.FirstOrDefault(t => t.TaskListID == selectedItem.ItemID);
        }

        public async Task OnNavigationViewSelectionChangeAsync(object selectedItem)
        {
            if (selectedItem == null)
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
            else
            {
                await _dialogService.ShowMessageDialogAsync($"Seleccionaste a navigation item", "Hola");
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
                await _dataService
                    .UserService
                    .ChangeCurrentUserStatus(false);

                _userCredentialService.DeleteUserCredentials();
                _navigationService.GoBack();
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

            var taskListToUpdate = (await _dataService
                .TaskListService
                .GetAsync(t => t.GoogleTaskListID == taskList.TaskListID, null, string.Empty))
                .FirstOrDefault();
            taskListToUpdate.Title = newTitle;
            taskListToUpdate.UpdatedAt = DateTime.Now;
            taskListToUpdate.ToBeSynced = true;
            taskListToUpdate.LocalStatus = LocalStatus.UPDATED;

            var response = await _dataService
                .TaskListService
                .UpdateAsync(taskListToUpdate);

            _messenger.Send(false, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't update the task list {taskList.Title}. Error = {response.Message}");
                return;
            }
            TaskLists[index] = _mapper.Map<TaskListItemViewModel>(taskListToUpdate);
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

            //If we are deleting the tasklist that is selected
            //lets close the panel to avoid creating new tasks
            //in the CurrentTaskList
            if (CurrentTaskList == taskList)
                OpenPane(false);

            _messenger.Send(true, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");
            var taskListToDelete = (await _dataService
                .TaskListService
                .GetAsync(tl => tl.GoogleTaskListID == taskList.TaskListID, string.Empty));

            Result response;
            //if the task is created but wasnt synced, we remove it from the db
            if (taskListToDelete.ToBeSynced)
            {
                response = await _dataService
                    .TaskListService
                    .RemoveAsync(taskListToDelete);
            }
            else
            {
                taskListToDelete.LocalStatus = LocalStatus.DELETED;
                taskListToDelete.ToBeSynced = true;

                response = await _dataService
                    .TaskListService
                    .UpdateAsync(taskListToDelete);
            }
            _messenger.Send(false, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't delete the task list {taskList.Title}. Error = {response.Message}");
                return;
            }

            await _dialogService.ShowMessageDialogAsync(
                "Succeed",
                $"Sucessfully removed {taskList.Title} task list");

            //Here we select the previoues task list only if the 
            //selected item = the task list to be removed
            if (SelectedItem == taskList)
            {
                int removedIndex = TaskLists.IndexOf(taskList);
                if (removedIndex != -1 && removedIndex > 0)
                    SelectedItem = TaskLists[removedIndex - 1];
                else
                    await OnNavigationViewSelectionChangeAsync(null);
            }
            else if (TaskLists.Count == 1)
                await OnNavigationViewSelectionChangeAsync(null);
            TaskLists.Remove(taskList);
        }

        private void OpenPane(bool isPaneOpen) => IsPaneOpen = isPaneOpen;
        #endregion
    }
}