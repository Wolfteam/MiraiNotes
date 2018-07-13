using AutoMapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Models.API;
using MiraiNotes.UWP.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace MiraiNotes.UWP.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        #region NavigationView Members
        private ObservableCollection<GoogleTaskListModel> _taskLists;
        private ObservableCollection<ItemModel> _taskListsAutoSuggestBoxItems;

        private string _taskListAutoSuggestBoxText;
        #endregion

        #region NavigationView Content Members
        private ObservableCollection<TaskModel> _tasks;
        private ObservableCollection<ItemModel> _taskAutoSuggestBoxItems;

        private GoogleTaskListModel _currentTaskList;

        private ObservableCollection<TaskModel> _selectedTasks;

        private string _taskAutoSuggestBoxText;
        private string _currentTaskListTitle;
        private string _currentTaskTitle;
        private string _currentTaskText;

        private bool _showTaskListViewProgressRing;
        private bool _showTaskProgressRing;
        private bool _isTaskListCommandBarOpen;
        private bool _isTaskListCommandBarCompact;
        private bool _isPaneOpen;
        #endregion

        private bool _isCurrentTaskTitleFocused;

        #region Members
        private readonly ICustomDialogService _dialogService;
        private readonly IMessenger _messenger;
        private readonly INavigationService _navigationService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IGoogleApiService _googleApiService;
        private readonly IMapper _mapper;
        #endregion

        #region NavigationView Properties
        public ObservableCollection<GoogleTaskListModel> TaskLists
        {
            get { return _taskLists; }
            set { SetValue(ref _taskLists, value); }
        }

        public ObservableCollection<ItemModel> TaskListsAutoSuggestBoxItems
        {
            get { return _taskListsAutoSuggestBoxItems; }
            set { SetValue(ref _taskListsAutoSuggestBoxItems, value); }
        }

        public GoogleTaskListModel CurrentTaskList
        {
            get { return _currentTaskList; }
            set { SetValue(ref _currentTaskList, value); }
        }

        public string TaskListkAutoSuggestBoxText
        {
            get { return _taskListAutoSuggestBoxText; }
            set { SetValue(ref _taskListAutoSuggestBoxText, value); }
        }
        #endregion

        #region NavigationView Content Properties
        public ObservableCollection<TaskModel> Tasks
        {
            get { return _tasks; }
            set { SetValue(ref _tasks, value); }
        }

        public ObservableCollection<ItemModel> TaskAutoSuggestBoxItems
        {
            get { return _taskAutoSuggestBoxItems; }
            set { SetValue(ref _taskAutoSuggestBoxItems, value); }
        }

        public ObservableCollection<TaskModel> SelectedTasks
        {
            get { return _selectedTasks; }
            set { SetValue(ref _selectedTasks, value); }
        }

        public string TaskAutoSuggestBoxText
        {
            get { return _taskAutoSuggestBoxText; }
            set { SetValue(ref _taskAutoSuggestBoxText, value); }
        }

        public string CurrentTaskListTitle
        {
            get { return _currentTaskTitle; }
            set { SetValue(ref _currentTaskTitle, value); }
        }

        public string CurrentTaskTitle
        {
            get { return _currentTaskListTitle; }
            set { SetValue(ref _currentTaskListTitle, value); }
        }

        public string CurrentTaskText
        {
            get { return _currentTaskText; }
            set { SetValue(ref _currentTaskText, value); }
        }

        public bool IsTaskListCommandBarCompact
        {
            get { return _isTaskListCommandBarCompact; }
            set { SetValue(ref _isTaskListCommandBarCompact, value); }
        }

        public bool IsTaskListCommandBarOpen
        {
            get { return _isTaskListCommandBarOpen; }
            set { SetValue(ref _isTaskListCommandBarOpen, value); }
        }

        public bool IsPaneOpen
        {
            get { return _isPaneOpen; }
            set { SetValue(ref _isPaneOpen, value); }
        }

        public bool ShowTaskListViewProgressRing
        {
            get { return _showTaskListViewProgressRing; }
            set { SetValue(ref _showTaskListViewProgressRing, value); }
        }

        public bool ShowTaskProgressRing
        {
            get { return _showTaskProgressRing; }
            set { SetValue(ref _showTaskProgressRing, value); }
        }
        #endregion

        #region NavigationView Pane Properties
        public bool IsCurrentTaskTitleFocused
        {
            get { return _isCurrentTaskTitleFocused; }
            set { SetValue(ref _isCurrentTaskTitleFocused, value); }
        }
        #endregion

        #region NavigationView Commands
        public ICommand TaskListAutoSuggestBoxTextChangedCommand { get; set; }

        public ICommand TaskListAutoSuggestBoxQuerySubmittedCommand { get; set; }

        public ICommand NavigationViewSelectionChanged { get; set; }

        public ICommand LogoutCommand { get; set; }
        #endregion

        #region NavigationView Content Commands
        public ICommand NewTaskListCommand { get; set; }

        public ICommand NewTaskCommand { get; set; }

        public ICommand OpenPaneCommand { get; set; }

        public ICommand ClosePaneCommand { get; set; } 

        public ICommand TaskListViewSelectedItemCommand { get; set; }

        public ICommand TaskListViewSelectedItemsCommand => new RelayCommand<object>((selectedTasks) =>
        {
            if (selectedTasks == null)
                return;
            //SelectedTasks = selectedTasks.ToList();
        });

        public ICommand TaskAutoSuggestBoxTextChangedCommand { get; set; }

        public ICommand TaskAutoSuggestBoxQuerySubmittedCommand => new RelayCommand<ItemModel>((itemSelected) =>
        {
        });

        public ICommand SelectAllTaskCommand => new RelayCommand(() =>
        {
            SelectedTasks = Tasks;
            //foreach (var task in Tasks)
            //{
            //    task.IsSelected = true;
            //}
        });

        public ICommand DeleteTaskListCommand { get; set; }

        public ICommand DeleteCurrentTaskListCommand { get; set; }

        public ICommand DeleteTaskCommand { get; set; }

        public ICommand RefreshTasksCommand { get; set; }

        public ICommand SortTasksCommand { get; set; }
        #endregion

        public HomeViewModel(ICustomDialogService dialogService,
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

            //NavigationView Commands
            TaskListAutoSuggestBoxTextChangedCommand = new RelayCommand<string>
                ((text) => OnTaskListAutoSuggestBoxTextChangeAsync(text));

            TaskListAutoSuggestBoxQuerySubmittedCommand = new RelayCommand<ItemModel>
                (async (selectedItem) => await OnTaskListAutoSuggestBoxQuerySubmittedAsync(selectedItem));

            TaskListViewSelectedItemCommand = new RelayCommand<TaskModel>
                (async (task) => await OnTaskListViewSelectedItemAsync(task));

            NavigationViewSelectionChanged = new RelayCommand<object>
                (async (selectedItem) => await OnNavigationViewSelectionChangeAsync(selectedItem));

            LogoutCommand = new RelayCommand(LogoutAsync);

            //NavigationView Content Commands
            TaskAutoSuggestBoxTextChangedCommand = new RelayCommand<string>
                (async (text) => await OnTaskAutoSuggestBoxTextChangeAsync(text));

            NewTaskListCommand = new RelayCommand(async () => await SaveNewTaskListAsync());

            NewTaskCommand = new RelayCommand(OpenPane);

            DeleteTaskListCommand = new RelayCommand<string>(async (taskListID) => await DeleteTaskList(taskListID));

            DeleteCurrentTaskListCommand = new RelayCommand(async () => await DeleteCurrentTaskListAsync());

            DeleteTaskCommand = new RelayCommand<string>(async (taskID) => await DeleteTask(taskID));

            RefreshTasksCommand = new RelayCommand(async () => await RefreshTasksAsync());

            SortTasksCommand = new RelayCommand<TaskSortType>((sortBy) => SortTasks(sortBy));

            OpenPaneCommand = new RelayCommand(OpenPane);
            ClosePaneCommand = new RelayCommand(ClosePane);
            
            //Others
            InitView();
        }


        #region NavigationView Methods
        public async Task OnSelectedTaskListAsync(GoogleTaskListModel taskList)
        {
            ShowTaskProgressRing = true;
            var response = await _googleApiService.TaskService.GetAllAsync(taskList.TaskListID);
            ShowTaskProgressRing = false;
            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An error occurred while trying to get the tasks for the selected tasklist = {taskList.Title}");
                return;
            }
            Tasks = _mapper.Map<ObservableCollection<TaskModel>>(response.Result.Items);
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

        public async Task OnTaskListAutoSuggestBoxQuerySubmittedAsync(ItemModel selectedItem)
        {
            if (selectedItem == null)
                return;
            CurrentTaskList = TaskLists.FirstOrDefault(t => t.TaskListID == selectedItem.ItemID);
            await OnSelectedTaskListAsync(CurrentTaskList);
        }

        public async Task OnNavigationViewSelectionChangeAsync(object selectedItem)
        {
            if (selectedItem is NavigationViewItem navViewItem)
                await _dialogService.ShowMessageDialogAsync($"Seleccionaste {navViewItem.Name}", "Hola");
            else if (selectedItem is GoogleTaskListModel taskList)
            {
                CurrentTaskList = taskList;
                await OnSelectedTaskListAsync(CurrentTaskList);
            }
        }

        public async void LogoutAsync()
        {
            bool logout = await _dialogService
                .ShowConfirmationDialogAsync("Are you sure you wanna log out?", "Yes", "No");
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
        #endregion

        #region NavigationView Content Methods
        public async Task OnTaskListViewSelectedItemAsync(TaskModel task)
        {
            if (task == null)
            {
                IsPaneOpen = false;
                CurrentTaskTitle = string.Empty;
                CurrentTaskText = string.Empty;
                return;
            }
            CurrentTaskText = task.Notes;
            CurrentTaskTitle = task.Title;
            IsPaneOpen = true;

            if (Tasks.Any(t => t.IsSelected))
                await _dialogService.ShowMessageDialogAsync("Hay algo seleccionado", "");
        }

        public async Task OnTaskAutoSuggestBoxTextChangeAsync(string currentText)
        {
            ShowTaskProgressRing = true;
            var response = await _googleApiService.TaskService.GetAllAsync(CurrentTaskList.TaskListID);
            ShowTaskProgressRing = false;
            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An error occurred while trying to get the tasks for the selected tasklist = {CurrentTaskList.Title}");
                return;
            }
            var filteredItems = string.IsNullOrEmpty(currentText) ?
                _mapper.Map<ObservableCollection<ItemModel>>(response.Result.Items
                    .OrderBy(t => t.Title)
                    .Take(10)) :
                _mapper.Map<ObservableCollection<ItemModel>>(response.Result.Items
                    .Where(t => t.Title.ToLowerInvariant().Contains(currentText.ToLowerInvariant()))
                    .OrderBy(t => t.Title)
                    .Take(10));

            TaskAutoSuggestBoxItems = filteredItems;
            Tasks = _mapper.Map<ObservableCollection<TaskModel>>(response.Result.Items
                .Where(t => filteredItems.Any(fi => fi.ItemID == t.TaskID)));
        }

        public async Task SaveNewTaskListAsync()
        {
            string taskListName = await _dialogService
                .ShowInputStringDialogAsync("Type the task list name", string.Empty, "Save", "Cancel");

            if (string.IsNullOrEmpty(taskListName))
                return;

            ShowTaskListViewProgressRing = true;
            var response = await _googleApiService
                .TaskListService
                .SaveAsync(new GoogleTaskListModel
                {
                    Title = taskListName,
                    UpdatedAt = DateTime.Now
                });
            ShowTaskListViewProgressRing = false;
            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't create the task list. Error code = {response.Errors.ApiError.Code}," +
                    $" message = {response.Errors.ApiError.Message}");
                return;
            }

            TaskLists.Add(response.Result);
            await _dialogService.ShowMessageDialogAsync("Succeed", "Task list created.");
        }

        public async Task DeleteTaskList(string taskListID)
        {
            var taskListToDelete = TaskLists
                .FirstOrDefault(tl => tl.TaskListID == taskListID);

            if (taskListToDelete == null)
                throw new NullReferenceException("The selected task list doesnt exists");

            bool deleteCurrentTaskList = await _dialogService
                .ShowConfirmationDialogAsync($"Are you sure you wanna delete {taskListToDelete.Title} task list?", "Yes", "No");

            if (!deleteCurrentTaskList)
                return;

            ShowTaskProgressRing = true;
            var response = await _googleApiService
                .TaskListService.DeleteAsync(taskListID);
            ShowTaskProgressRing = false;

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't delete the task list {taskListToDelete.Title}. Error code = {response.Errors.ApiError.Code}," +
                    $" message = {response.Errors.ApiError.Message}");
                return;
            }

            await _dialogService
                .ShowMessageDialogAsync("Succeed", $"Sucessfully removed {taskListToDelete.Title} task list");
            TaskLists.Remove(taskListToDelete);
        }

        public async Task DeleteCurrentTaskListAsync()
        {
            if (CurrentTaskList == null)
                throw new NullReferenceException("There is no current selected task list");

            var taskListToDelete = TaskLists
                .FirstOrDefault(tl => tl.TaskListID == CurrentTaskList.TaskListID);

            if (taskListToDelete == null)
                throw new NullReferenceException("The selected task list doesnt exists");

            bool deleteCurrentTaskList = await _dialogService
                .ShowConfirmationDialogAsync($"Are you sure you wanna delete {taskListToDelete.Title} task list?", "Yes", "No");

            if (!deleteCurrentTaskList)
                return;

            ShowTaskProgressRing = true;
            var response = await _googleApiService
                .TaskListService.DeleteAsync(taskListToDelete.TaskListID);
            ShowTaskProgressRing = false;

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't delete the task list {taskListToDelete.Title}. Error code = {response.Errors.ApiError.Code}," +
                    $" message = {response.Errors.ApiError.Message}");
                return;
            }

            await _dialogService
                .ShowMessageDialogAsync("Succeed", $"Sucessfully removed {taskListToDelete.Title} task list");
            CurrentTaskList = null;
            TaskLists.Remove(taskListToDelete);
        }

        public async Task DeleteTask (string taskID)
        {
            bool deleteTask = await _dialogService
                .ShowConfirmationDialogAsync("Are you sure you wanna delete this tasks?", "Yes", "No");

            if (!deleteTask)
                return;
            var taskToDelete = Tasks.FirstOrDefault(t => t.TaskID == taskID);
            if (taskToDelete == null)
                throw new KeyNotFoundException($"Couldn't find a task with the id {taskID}");

            ShowTaskListViewProgressRing = true;
            var response = await _googleApiService
                .TaskService.DeleteAsync(CurrentTaskList.TaskListID, taskID);
            ShowTaskListViewProgressRing = false;

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't delete the selected task. Error code = {response.Errors.ApiError.Code}," +
                    $" message = {response.Errors.ApiError.Message}");
                return;
            }

            Tasks.Remove(taskToDelete);
        }

        public async Task RefreshTasksAsync()
        {
            ShowTaskListViewProgressRing = true;
            var response = await _googleApiService.TaskService.GetAllAsync(CurrentTaskList.TaskListID);
            ShowTaskListViewProgressRing = false;
            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An error occurred while trying to refresh the tasks for the selected tasklist = {CurrentTaskList.Title}");
                return;
            }
            Tasks = _mapper.Map<ObservableCollection<TaskModel>>(response.Result.Items);
        }

        public void SortTasks(TaskSortType sortType)
        {
            switch (sortType)
            {
                case TaskSortType.BY_NAME_ASC:
                    Tasks = new ObservableCollection<TaskModel>(Tasks.OrderBy(t => t.Title));
                    break;
                case TaskSortType.BY_NAME_DESC:
                    Tasks = new ObservableCollection<TaskModel>(Tasks.OrderByDescending(t => t.Title));
                    break;
                case TaskSortType.BY_UPDATED_DATE_ASC:
                    Tasks = new ObservableCollection<TaskModel>(Tasks.OrderBy(t => t.UpdatedAt));
                    break;
                case TaskSortType.BY_UPDATED_DATE_DESC:
                    Tasks = new ObservableCollection<TaskModel>(Tasks.OrderByDescending(t => t.UpdatedAt));
                    break;
                case TaskSortType.CUSTOM_ASC:
                    Tasks = new ObservableCollection<TaskModel>(Tasks.OrderBy(t => t.Position));
                    break;
                case TaskSortType.CUSTOM_DESC:
                    Tasks = new ObservableCollection<TaskModel>(Tasks.OrderByDescending(t => t.Position));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("The TaskSortType doesnt have a default sort type");
            }
        }

        private void OpenPane()
        {
            CleanPanel();
            IsPaneOpen = true;
            IsCurrentTaskTitleFocused = true;
        }

        private void ClosePane()
        {
            CleanPanel();
            IsPaneOpen = false;
        }

        private void CleanPanel()
        {
            if (!string.IsNullOrEmpty(CurrentTaskText))
                CurrentTaskText = string.Empty;

            if (!string.IsNullOrEmpty(CurrentTaskTitle))
                CurrentTaskTitle = string.Empty;
        }
        #endregion

        #region NavigationView Pane Methods
        #endregion

        #region Methods
        private async void InitView()
        {
            ShowTaskListViewProgressRing = true;
            var response = await _googleApiService.TaskListService.GetAllAsync();
            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Coudn't get the task lists",
                    $"Status Code: {response.Errors.ApiError.Code}. {response.Errors.ApiError.Message}");
                ShowTaskListViewProgressRing = false;
                return;
            }
            TaskLists = new ObservableCollection<GoogleTaskListModel>(response.Result.Items.OrderBy(t => t.Title));

            TaskListsAutoSuggestBoxItems = _mapper.Map<ObservableCollection<ItemModel>>(response.Result.Items.OrderBy(t => t.Title));

            var taskList = TaskLists.FirstOrDefault();
            CurrentTaskList = taskList;
            if (string.IsNullOrEmpty(taskList?.Title))
            {
                return;
            }
            var response2 = await _googleApiService.TaskService.GetAllAsync(taskList.TaskListID);

            if (!response2.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Couldn't get the tasks for the selected task list",
                    $"Status Code: {response2.Errors.ApiError.Code}. {response2.Errors.ApiError.Message}");
                ShowTaskListViewProgressRing = false;
                return;
            }
            ShowTaskListViewProgressRing = false;
            CurrentTaskListTitle = taskList.Title;
            Tasks = _mapper.Map<ObservableCollection<TaskModel>>(response2.Result.Items);
            TaskAutoSuggestBoxItems = _mapper.Map<ObservableCollection<ItemModel>>(response2.Result.Items.OrderBy(t => t.Title));
        }

        public void CreateNewTaskList() => _dialogService.ShowMessageDialogAsync("You clicked on a item", "Clicked");

        public void CreateNewTask()
        {
            _dialogService.ShowMessageDialogAsync("You clicked on a item", "Clicked");
        }

        public void ProcessQuery()
        {
            _dialogService.ShowMessageDialogAsync("You clicked on a item", "Clicked");
        }

        public void ProcessChoice()
        {
            _dialogService.ShowMessageDialogAsync("You clicked on a item", "Clicked");
        }

        #endregion
    }
}