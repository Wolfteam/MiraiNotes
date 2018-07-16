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

        private bool _showTaskListViewProgressRing;
        private bool _showTaskProgressRing;
        private bool _isTaskListCommandBarOpen;
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

        public void OnTaskSaved(TaskModel task)
        {
            var modifiedTask = Tasks.FirstOrDefault(t => t.TaskID == task.TaskID);
            if (modifiedTask != null)
            {
                Tasks.Remove(modifiedTask);
            }
            Tasks.Add(task);
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
            _messenger.Send(response.Result, "NewTaskListAdded");
            await _dialogService.ShowMessageDialogAsync("Succeed", "Task list created.");
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
        #endregion
    }
}