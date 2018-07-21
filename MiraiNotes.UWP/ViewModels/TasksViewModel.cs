using AutoMapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Models.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MiraiNotes.UWP.ViewModels
{
    public class TasksViewModel : BaseViewModel
    {
        #region Members
        private readonly ICustomDialogService _dialogService;
        private readonly IMessenger _messenger;
        private readonly INavigationService _navigationService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IGoogleApiService _googleApiService;
        private readonly IMapper _mapper;

        private GoogleTaskListModel _currentTaskList;

        private ObservableCollection<TaskModel> _tasks;
        private ObservableCollection<ItemModel> _taskAutoSuggestBoxItems;
        private ObservableCollection<TaskModel> _selectedTasks;

        private string _taskAutoSuggestBoxText;

        private bool _isTaskListTitleVisible;
        private bool _isTaskListViewVisible;
        private bool _isAutoSuggestBoxEnabled;
        private bool _canAddMoreTaskList;
        private bool _canAddMoreTasks;
        private bool _canDeleteTasks;
        private bool _canRefreshTaskListView;
        private bool _canSortTaskListView;
        private bool _showTaskListViewProgressRing;
        private bool _isTaskListCommandBarCompact;
        #endregion

        #region Properties
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

        public GoogleTaskListModel CurrentTaskList
        {
            get { return _currentTaskList; }
            set { SetValue(ref _currentTaskList, value); }
        }

        public string TaskAutoSuggestBoxText
        {
            get { return _taskAutoSuggestBoxText; }
            set { SetValue(ref _taskAutoSuggestBoxText, value); }
        }

        public bool IsTaskListTitleVisible
        {
            get { return _isTaskListTitleVisible; }
            set { SetValue(ref _isTaskListTitleVisible, value); }
        }

        public bool IsTaskListViewVisible
        {
            get { return _isTaskListViewVisible; }
            set { SetValue(ref _isTaskListViewVisible, value); }
        }

        public bool IsAutoSuggestBoxEnabled
        {
            get { return _isAutoSuggestBoxEnabled; }
            set { SetValue(ref _isAutoSuggestBoxEnabled, value); }
        }

        public bool CanAddMoreTaskList
        {
            get { return _canAddMoreTaskList; }
            set { SetValue(ref _canAddMoreTaskList, value); }
        }

        public bool CanAddMoreTasks
        {
            get { return _canAddMoreTasks; }
            set { SetValue(ref _canAddMoreTasks, value); }
        }

        public bool CanDeleteTasks
        {
            get { return _canDeleteTasks; }
            set { SetValue(ref _canDeleteTasks, value); }
        }

        public bool CanRefreshTaskListView
        {
            get { return _canRefreshTaskListView; }
            set { SetValue(ref _canRefreshTaskListView, value); }
        }

        public bool CanSortTaskListView
        {
            get { return _canSortTaskListView; }
            set { SetValue(ref _canSortTaskListView, value); }
        }

        public bool IsTaskListCommandBarCompact
        {
            get { return _isTaskListCommandBarCompact; }
            set { SetValue(ref _isTaskListCommandBarCompact, value); }
        }

        public bool ShowTaskListViewProgressRing
        {
            get { return _showTaskListViewProgressRing; }
            set
            {
                HideControls(!value);
                DisableControls(!value);
                SetValue(ref _showTaskListViewProgressRing, value);
            }
        }
        #endregion

        #region Commands
        public ICommand NewTaskCommand { get; set; }

        public ICommand NewTaskListCommand { get; set; }

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

        public ICommand DeleteTaskCommand { get; set; }

        public ICommand RefreshTasksCommand { get; set; }

        public ICommand SortTasksCommand { get; set; }
        #endregion

        #region Constructors
        public TasksViewModel(
            ICustomDialogService dialogService,
            IMessenger messenger,
            INavigationService navigationService,
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

            _messenger.Register<GoogleTaskListModel>(this, "OnNavigationViewSelectionChange",
                async (taskList) => await GetAllTasksAsync(taskList));

            _messenger.Register<bool>(this, "ShowTaskListViewProgressRing",
                (show) => ShowTaskListViewProgressRing = show);

            _messenger.Register<TaskModel>(this, "TaskSaved", OnTaskSaved);
            _messenger.Register<string>(this, "TaskDeleted", OnTaskDeleted);

            _messenger.Register<GoogleTaskListModel>(this, "UpdatedTaskList", OnUpdatedTaskList);


            TaskListViewSelectedItemCommand = new RelayCommand<TaskModel>
                ((task) => OnTaskListViewSelectedItem(task));

            TaskAutoSuggestBoxTextChangedCommand = new RelayCommand<string>
                (async (text) => await OnTaskAutoSuggestBoxTextChangeAsync(text));

            NewTaskCommand = new RelayCommand(() => OnTaskListViewSelectedItem(new TaskModel()));

            NewTaskListCommand = new RelayCommand(async () => await SaveNewTaskListAsync());

            DeleteTaskCommand = new RelayCommand<string>(async (taskID) => await DeleteTask(taskID));

            RefreshTasksCommand = new RelayCommand(async () => await GetAllTasksAsync(CurrentTaskList));

            SortTasksCommand = new RelayCommand<TaskSortType>((sortBy) => SortTasks(sortBy));

            IsTaskListCommandBarCompact = true;
        }
        #endregion

        #region Methods
        public async Task GetAllTasksAsync(GoogleTaskListModel taskList)
        {
            if (taskList == null)
            {
                OnNoTaskListAvailable();
                return;
            }

            ShowTaskListViewProgressRing = true;
            var response = await _googleApiService.TaskService.GetAllAsync(taskList.TaskListID);
            ShowTaskListViewProgressRing = false;

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Couldn't get the tasks for the selected task list",
                    $"Status Code: {response.Errors.ApiError.Code}. {response.Errors.ApiError.Message}");
                return;
            }

            if (response.Result.Items != null && response.Result.Items.Count() > 0)
            {
                Tasks = _mapper.Map<ObservableCollection<TaskModel>>(response.Result.Items);
                TaskAutoSuggestBoxItems = _mapper.Map<ObservableCollection<ItemModel>>(response.Result.Items.OrderBy(t => t.Title));
            }
            else
            {
                Tasks.Clear();
                TaskAutoSuggestBoxItems.Clear();
            }
            CurrentTaskList = taskList;
        }

        private void OnNoTaskListAvailable()
        {
            HideControls(false);
            DisableControls(false);
            CanAddMoreTaskList = true;
            CurrentTaskList = null;
            Tasks.Clear();
            TaskAutoSuggestBoxItems.Clear();
            //this is not being instanciated
            //SelectedTasks.Clear();
            _messenger.Send(false, "OpenPane");
        }

        public void OnTaskListViewSelectedItem(TaskModel task)
        {
            //When a list contains no items, and you switch to that list
            //the event raises, thats why whe do this check here
            if (task == null)
                return;
            _messenger.Send(true, "OpenPane");
            _messenger.Send(task, "NewTask");
        }

        public async Task OnTaskAutoSuggestBoxTextChangeAsync(string currentText)
        {
            //TODO: Refactor this
            ShowTaskListViewProgressRing = true;
            var response = await _googleApiService.TaskService.GetAllAsync(CurrentTaskList.TaskListID);
            ShowTaskListViewProgressRing = false;
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

        public void OnTaskSaved(TaskModel task)
        {
            var modifiedTask = Tasks.FirstOrDefault(t => t.TaskID == task.TaskID);
            if (modifiedTask != null)
            {
                Tasks.Remove(modifiedTask);
            }
            if (task.TaskStatus == GoogleTaskStatus.NEEDS_ACTION)
                Tasks.Add(task);
        }

        public void OnTaskDeleted(string taskID)
        {
            //TODO: Fix this or rethink it
            var taskToDelete = Tasks.FirstOrDefault(t => t.TaskID == taskID);
            if (taskToDelete != null)
                Tasks.Remove(taskToDelete);
        }

        public void OnUpdatedTaskList(GoogleTaskListModel taskList)
        {
            //if (CurrentTaskList.TaskListID == taskList.TaskListID)
            //    CurrentTaskList = taskList;
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
            await _dialogService.ShowMessageDialogAsync("Succeed", "Task list created.");
            _messenger.Send(response.Result, "NewTaskListAdded");
        }

        public async Task DeleteTask(string taskID)
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

        private void DisableControls(bool isEnabled)
        {
            IsAutoSuggestBoxEnabled = isEnabled;
            CanAddMoreTaskList = isEnabled;
            CanAddMoreTasks = isEnabled;
            CanDeleteTasks = isEnabled;
            CanRefreshTaskListView = isEnabled;
            CanSortTaskListView = isEnabled;
        }

        private void HideControls(bool isVisible)
        {
            IsTaskListTitleVisible = isVisible;
            IsTaskListViewVisible = isVisible;
        }
        #endregion
    }
}