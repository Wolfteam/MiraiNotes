using AutoMapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MiraiNotes.UWP.ViewModels
{
    public class NewTaskViewModel : BaseViewModel
    {
        #region Members
        private readonly ICustomDialogService _dialogService;
        private readonly IMessenger _messenger;
        private readonly INavigationService _navigationService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IGoogleApiService _googleApiService;
        private readonly IMapper _mapper;

        private TaskModel _currentTask;
        private bool _showTaskProgressRing;
        #endregion

        #region Properties
        public TaskModel CurrentTask
        {
            get { return _currentTask; }
            set { SetValue(ref _currentTask, value); }
        }

        public bool ShowTaskProgressRing
        {
            get { return _showTaskProgressRing; }
            set { SetValue(ref _showTaskProgressRing, value); }
        }

        public bool IsCurrentTaskTitleFocused { get; set; }
        #endregion

        #region Commands
        public ICommand ClosePaneCommand { get; set; }
        #endregion

        #region Constructor
        public NewTaskViewModel(
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

            ClosePaneCommand = new RelayCommand(CleanPanel);

            _messenger.Register<TaskModel>(this, "NewTask", (task) => InitView(task));
        }
        #endregion

        #region Methods
        public void InitView(TaskModel task)
        {
            if (task == null)
                CurrentTask = new TaskModel();
            else
                CurrentTask = task;

            IsCurrentTaskTitleFocused = true;
        }

        private void CleanPanel()
        {
            CurrentTask.Title = string.Empty;
            CurrentTask.Notes = string.Empty;
            _messenger.Send(false, "OpenPane");
        } 
        #endregion
    }
}
