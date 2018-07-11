using AutoMapper;
using GalaSoft.MvvmLight.Command;
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

        #region Members
        private readonly IDialogService _dialogService;
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

        #region NavigationView Commands
        public ICommand TaskListAutoSuggestBoxTextChangedCommand { get; set; }

        public ICommand TaskListAutoSuggestBoxQuerySubmittedCommand { get; set; }

        public ICommand NavigationViewSelectionChanged { get; set; }

        public ICommand LogoutCommand => new RelayCommand(LogoutAsync);
        #endregion

        #region NavigationView Content Commands
        public ICommand NewTaskListCommand
        {
            get
            {
                return new RelayCommand(CreateNewTaskList);
            }
        }

        public ICommand NewTaskCommand
        {
            get
            {
                return new RelayCommand(CreateNewTask);
            }
        }

        public ICommand TaskAutoSuggestBoxCommand
        {
            get
            {
                return new RelayCommand(TaskAutoSuggestBox);
            }
        }

        public ICommand OpenPaneCommand { get; set; }

        public ICommand ClosePaneCommand => new RelayCommand(ClosePane);

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

        public ICommand RefreshTasksCommand { get; set; }

        public ICommand SortTasksCommand { get; set; }
        #endregion

        public HomeViewModel(IDialogService dialogService,
                            INavigationService navigationService,
                            IUserCredentialService userCredentialService,
                            IGoogleApiService googleApiService,
                            IMapper mapper)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userCredentialService = userCredentialService;
            _googleApiService = googleApiService;
            _mapper = mapper;

            //NavigationView Commands
            TaskListAutoSuggestBoxTextChangedCommand = new RelayCommand<string>
                ((text) => OnTaskListAutoSuggestBoxTextChange(text));

            TaskListAutoSuggestBoxQuerySubmittedCommand = new RelayCommand<ItemModel>
                (async (selectedItem) => await OnTaskListAutoSuggestBoxQuerySubmitted(selectedItem));

            NavigationViewSelectionChanged = new RelayCommand<object>
                (async (selectedItem) => await OnNavigationViewSelectionChange(selectedItem));

            //NavigationView Content Commands
            TaskListViewSelectedItemCommand = new RelayCommand<TaskModel>
                (async (task) => await OnTaskListViewSelectedItem(task));

            TaskAutoSuggestBoxTextChangedCommand = new RelayCommand<string>
                (async (text) => await OnTaskAutoSuggestBoxTextChange(text));

            RefreshTasksCommand = new RelayCommand(async() => await RefreshTasks());

            SortTasksCommand = new RelayCommand<TaskSortType>((sortBy) => SortTasks(sortBy));

            OpenPaneCommand = new RelayCommand(OpenPane);

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
                await _dialogService.ShowMessage(
                    $"An error occurred while trying to get the tasks for the selected tasklist = {taskList.Title}",
                    "Error");
                return;
            }
            Tasks = _mapper.Map<ObservableCollection<TaskModel>>(response.Result.Items);
        }

        public void OnTaskListAutoSuggestBoxTextChange(string currentText)
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

        public async Task OnTaskListAutoSuggestBoxQuerySubmitted(ItemModel selectedItem)
        {
            if (selectedItem == null)
                return;
            _currentTaskList = TaskLists.FirstOrDefault(t => t.TaskListID == selectedItem.ItemID);
            await OnSelectedTaskListAsync(_currentTaskList);
        }

        public async Task OnNavigationViewSelectionChange(object selectedItem)
        {
            if (selectedItem is NavigationViewItem navViewItem)
                _dialogService.ShowMessage($"Seleccionaste {navViewItem.Name}", "Hola");
            else if (selectedItem is GoogleTaskListModel taskList)
            {
                _currentTaskList = taskList;
                await OnSelectedTaskListAsync(_currentTaskList);
            }
        }

        public async void LogoutAsync()
        {
            await _dialogService.ShowMessage("Are you sure you wanna log out?", "Sign out", "Yes", "No", (logout) =>
            {
                if (logout)
                {
                    //TODO: DELETE ALL !!
                    //delete all from the db
                    //delete user settings
                    //delete all view models
                    _userCredentialService.DeleteUserCredentials();
                    _navigationService.GoBack();
                }
            });
        }
        #endregion


        #region NavigationView Content Methods
        public async Task OnTaskListViewSelectedItem(TaskModel task)
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
                await _dialogService.ShowMessage("Hay algo seleccionado", "");
        }

        public async Task OnTaskAutoSuggestBoxTextChange(string currentText)
        {
            ShowTaskProgressRing = true;
            var response = await _googleApiService.TaskService.GetAllAsync(_currentTaskList.TaskListID);
            ShowTaskProgressRing = false;
            if (!response.Succeed)
            {
                await _dialogService.ShowMessage(
                    $"An error occurred while trying to get the tasks for the selected tasklist = {_currentTaskList.Title}",
                    "Error");
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

        public async Task RefreshTasks()
        {
            ShowTaskListViewProgressRing = true;
            var response = await _googleApiService.TaskService.GetAllAsync(_currentTaskList.TaskListID);
            ShowTaskListViewProgressRing = false;
            if (!response.Succeed)
            {
                await _dialogService.ShowMessage(
                    $"An error occurred while trying to refresh the tasks for the selected tasklist = {_currentTaskList.Title}",
                    "Error");
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

        private void OpenPane() => IsPaneOpen = true;

        private void ClosePane() => IsPaneOpen = false;
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
                await _dialogService.ShowMessage(
                    $"Status Code: {response.Errors.ApiError.Code}. {response.Errors.ApiError.Message}",
                    "Coudn't get the task lists");
                ShowTaskListViewProgressRing = false;
                return;
            }
            TaskLists = new ObservableCollection<GoogleTaskListModel>(response.Result.Items.OrderBy(t => t.Title));

            TaskListsAutoSuggestBoxItems = _mapper.Map<ObservableCollection<ItemModel>>(response.Result.Items.OrderBy(t => t.Title));

            var taskList = TaskLists.FirstOrDefault();
            _currentTaskList = taskList;
            if (string.IsNullOrEmpty(taskList?.Title))
            {
                return;
            }
            var response2 = await _googleApiService.TaskService.GetAllAsync(taskList.TaskListID);

            if (!response2.Succeed)
            {
                await _dialogService.ShowMessage(
                    $"Status Code: {response2.Errors.ApiError.Code}. {response2.Errors.ApiError.Message}",
                    "Couldn't get the tasks for the selected task list");
                ShowTaskListViewProgressRing = false;
                return;
            }
            ShowTaskListViewProgressRing = false;
            CurrentTaskListTitle = taskList.Title;
            Tasks = _mapper.Map<ObservableCollection<TaskModel>>(response2.Result.Items);
            TaskAutoSuggestBoxItems = _mapper.Map<ObservableCollection<ItemModel>>(response2.Result.Items.OrderBy(t => t.Title));
        }

        public void TaskAutoSuggestBox()
        {
            Debug.WriteLine($"You typed {TaskAutoSuggestBoxText}");
        }

        public void CreateNewTaskList() => _dialogService.ShowMessage("You clicked on a item", "Clicked");

        public void CreateNewTask()
        {
            _dialogService.ShowMessage("You clicked on a item", "Clicked");
        }

        public void ProcessQuery()
        {
            _dialogService.ShowMessage("You clicked on a item", "Clicked");
        }

        public void ProcessChoice()
        {
            _dialogService.ShowMessage("You clicked on a item", "Clicked");
        }

        #endregion
    }
}