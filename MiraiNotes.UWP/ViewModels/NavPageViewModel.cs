using AutoMapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Models.API;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MiraiNotes.UWP.ViewModels
{
    public class NavPageViewModel : BaseViewModel
    {
        #region Members
        private readonly ICustomDialogService _dialogService;
        private readonly IMessenger _messenger;
        private readonly INavigationService _navigationService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IGoogleApiService _googleApiService;
        private readonly IMapper _mapper;

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
            set { SetValue(ref _selectedItem, value); }
        }

        public ObservableCollection<TaskListItemViewModel> TaskLists
        {
            get { return _taskLists; }
            set { SetValue(ref _taskLists, value); }
        }

        public ObservableCollection<ItemModel> TaskListsAutoSuggestBoxItems
        {
            get { return _taskListsAutoSuggestBoxItems; }
            set { SetValue(ref _taskListsAutoSuggestBoxItems, value); }
        }

        public TaskListItemViewModel CurrentTaskList
        {
            get { return _currentTaskList; }
            set { SetValue(ref _currentTaskList, value); }
        }

        public string TaskListkAutoSuggestBoxText
        {
            get { return _taskListAutoSuggestBoxText; }
            set { SetValue(ref _taskListAutoSuggestBoxText, value); }
        }

        public bool IsPaneOpen
        {
            get { return _isPaneOpen; }
            set { SetValue(ref _isPaneOpen, value); }
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
            IGoogleApiService googleApiService,
            IMapper mapper)
        {
            _dialogService = dialogService;
            _messenger = messenger;
            _navigationService = navigationService;
            _userCredentialService = userCredentialService;
            _googleApiService = googleApiService;
            _mapper = mapper;

            _messenger.Register<TaskListItemViewModel>(this, $"{MessageType.TASK_LIST_ADDED}", OnTaskListAdded);
            _messenger.Register<bool>(this, $"{MessageType.OPEN_PANE}", (open) => OpenPane(open));
            //if (IsInDesignMode)
            //{
            //    var task = _googleApiService.TaskListService.GetAllAsync();
            //    task.Wait();
            //    TaskLists = new ObservableCollection<GoogleTaskListModel>(task.Result.Result.Items);
            //    var task2 = _googleApiService.TaskService.GetAllAsync(TaskLists.First().TaskListID);
            //    task2.Wait();

            //    Tasks = new ObservableCollection<TaskModel>(_mapper.Map<TaskModel>())
            //}
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

        #region Methods
        private async Task InitViewAsync()
        {
            _messenger.Send(true, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");
            var response = await _googleApiService.TaskListService.GetAllAsync();
            _messenger.Send(false, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Coudn't get the task lists",
                    $"Status Code: {response.Errors.ApiError.Code}. {response.Errors.ApiError.Message}");
                return;
            }
            var orderedItems = response.Result.Items.OrderBy(t => t.Title);
            TaskLists = _mapper.Map<ObservableCollection<TaskListItemViewModel>>(orderedItems);

            TaskListsAutoSuggestBoxItems = _mapper.Map<ObservableCollection<ItemModel>>(orderedItems);

            SelectedItem = TaskLists.FirstOrDefault();
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

            var taskListToUpdate = _mapper.Map<GoogleTaskListModel>(taskList);

            taskListToUpdate.Title = newTitle;
            taskListToUpdate.UpdatedAt = DateTime.Now;

            _messenger.Send(true, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");
            var response = await _googleApiService
                .TaskListService
                .UpdateAsync(taskListToUpdate.TaskListID, taskListToUpdate);
            _messenger.Send(false, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't update the task list {taskList.Title}. Error code = {response.Errors.ApiError.Code}," +
                    $" message = {response.Errors.ApiError.Message}");
                return;
            }
            TaskLists[index] = _mapper.Map<TaskListItemViewModel>(response.Result);
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
            var response = await _googleApiService
                .TaskListService.DeleteAsync(taskList.TaskListID);
            _messenger.Send(false, $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}");

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't delete the task list {taskList.Title}. Error code = {response.Errors.ApiError.Code}," +
                    $" message = {response.Errors.ApiError.Message}");
                return;
            }

            await _dialogService
                .ShowMessageDialogAsync("Succeed", $"Sucessfully removed {taskList.Title} task list");

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