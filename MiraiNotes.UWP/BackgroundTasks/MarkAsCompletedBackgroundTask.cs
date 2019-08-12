﻿using System;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using AutoMapper;
using GalaSoft.MvvmLight.Messaging;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Core.Enums;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.ViewModels;
using Serilog;

namespace MiraiNotes.UWP.BackgroundTasks
{
    public class MarkAsCompletedBackgroundTask : IBackgroundTask
    {
        private readonly ICustomToastNotificationManager _toastNotificationManager;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IMessenger _messenger;
        private readonly IMiraiNotesDataService _dataService;
        private readonly bool _isAppRunning;
        private BackgroundTaskDeferral _deferral;

        public MarkAsCompletedBackgroundTask()
        {
            var vml = new ViewModelLocator();
            _toastNotificationManager = vml.ToastNotificationManager;
            _dataService = vml.DataService;
            _logger = vml.Logger.ForContext<MarkAsCompletedBackgroundTask>();
            _mapper = vml.Mapper;
            _messenger = vml.Messenger;
            _isAppRunning = ViewModelLocator.IsAppRunning;
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            _logger.Information($"{nameof(MarkAsCompletedBackgroundTask)}: Started");

            if (taskInstance.TriggerDetails is ToastNotificationActionTriggerDetail == false)
            {
                _logger.Warning($"{nameof(MarkAsCompletedBackgroundTask)}: was not started by a toast notification");
                return;
            }

            var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
            var queryParams = details.Argument
                .Split('&')
                .ToDictionary(c => c.Split('=')[0], c => Uri.UnescapeDataString(c.Split('=')[1]));

            string taskID = queryParams["taskID"];
            var response = await _dataService
                .TaskService
                .ChangeTaskStatusAsync(taskID, GoogleTaskStatus.COMPLETED);

            if (response.Succeed)
            {
                if (_isAppRunning)
                {
                    var task = _mapper.Map<TaskItemViewModel>(response.Result);
                    _messenger.Send(
                        new Tuple<TaskItemViewModel, bool>(task, task.HasParentTask),
                        $"{MessageType.TASK_STATUS_CHANGED_FROM_PANE_FRAME}");
                    _messenger.Send(
                        new Tuple<TaskItemViewModel, bool>(task, task.HasParentTask),
                        $"{MessageType.TASK_STATUS_CHANGED_FROM_CONTENT_FRAME}");
                }
                _toastNotificationManager.ShowSimpleToastNotification(
                    "Succeess",
                    $"Task {response.Result.Title} was marked as completed",
                    "TaskReminderToastTag",
                    true,
                    true);
            }
            else
            {
                _toastNotificationManager.ShowSimpleToastNotification(
                    "Error",
                    $"Task {response.Result.Title} could not be marked as completed. Error = {response.Message}",
                    "TaskReminderToastTag",
                    true,
                    true);
                _logger.Error($"{nameof(MarkAsCompletedBackgroundTask)}: an unknown error ocurred = {response.Message}");
            }
            _logger.Information($"{nameof(MarkAsCompletedBackgroundTask)}: completed successfully");
            _deferral.Complete();
        }
    }
}
