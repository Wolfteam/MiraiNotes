using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.ViewModels.Dialogs;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MiraiNotes.Shared;
using MiraiNotes.Shared.Utils;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.UI;
using MvvmCross.ViewModels;
using Plugin.Fingerprint.Abstractions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MiraiNotes.Android.ViewModels.Settings
{
    public class SettingsGeneralViewModel : BaseViewModel
    {
        #region Members
        private ItemModel _selectedTaskListSortOrder;
        private ItemModel _selectedTaskSortOrder;
        private readonly MvxInteraction<MvxColor> _onAccentColorSelected = new MvxInteraction<MvxColor>();
        private readonly IFingerprint _fingerprintService;
        private readonly IDialogService _dialogService;
        #endregion

        #region Interactors
        public IMvxInteraction<MvxColor> OnAccentColorSelected
            => _onAccentColorSelected;
        #endregion        

        #region Properties
        public MvxObservableCollection<ItemModel> AppThemes => new MvxObservableCollection<ItemModel>
        {
            new ItemModel
            {
                ItemId = AppThemeType.DARK.ToString(),
                Text = GetText("Dark")
            },
            new ItemModel
            {
                ItemId = AppThemeType.LIGHT.ToString(),
                Text = GetText("Light")
            }
        };

        public List<MvxColor> AccentColors { get; }

        public List<ItemModel> TaskListSortTypes => new List<ItemModel>
        {
            new ItemModel
            {
                ItemId = TaskListSortType.BY_NAME_ASC.ToString(),
                Text = GetText("SortByNameAsc")
            },
            new ItemModel
            {
                ItemId = TaskListSortType.BY_NAME_DESC.ToString(),
                Text = GetText("SortByNameDesc")
            },
            new ItemModel
            {
                ItemId = TaskListSortType.BY_UPDATED_DATE_ASC.ToString(),
                Text = GetText("SortByUpdatedDateAsc")
            },
            new ItemModel
            {
                ItemId = TaskListSortType.BY_UPDATED_DATE_DESC.ToString(),
                Text = GetText("SortByUpdatedDateDesc")
            }
        };

        public List<ItemModel> TasksSortTypes => new List<ItemModel>
        {
            new ItemModel
            {
                ItemId = TaskSortType.BY_NAME_ASC.ToString(),
                Text = GetText("SortByNameAsc")
            },
            new ItemModel
            {
                ItemId = TaskSortType.BY_NAME_DESC.ToString(),
                Text = GetText("SortByNameDesc")
            },
            new ItemModel
            {
                ItemId = TaskSortType.BY_UPDATED_DATE_ASC.ToString(),
                Text = GetText("SortByUpdatedDateAsc")
            },
            new ItemModel
            {
                ItemId = TaskSortType.BY_UPDATED_DATE_DESC.ToString(),
                Text = GetText("SortByUpdatedDateDesc")
            }
        };

        public List<ItemModel> AppLanguges => new List<ItemModel>
        {
            new ItemModel
            {
                ItemId = AppLanguageType.English.ToString(),
                Text = GetText("English")
            },
            new ItemModel
            {
                ItemId = AppLanguageType.Spanish.ToString(),
                Text = GetText("Spanish")
            },
        };

        public ItemModel SelectedAppTheme
        {
            get
            {
                var currentSelectedTheme = AppSettings.AppTheme;
                return AppThemes.First(theme => theme.ItemId == currentSelectedTheme.ToString());
            }
            set
            {
                var selectedTheme = (AppThemeType)Enum.Parse(typeof(AppThemeType), value.ItemId, true);
                if (AppSettings.AppTheme == selectedTheme)
                    return;
                AppSettings.AppTheme = selectedTheme;
                RaisePropertyChanged(() => SelectedAppTheme);
                RaisePropertyChanged(() => CanUseDarkAmoledTheme);
                Messenger.Publish(new AppThemeChangedMsg(this, selectedTheme, AppSettings.AppHexAccentColor, AppSettings.UseDarkAmoledTheme));
            }
        }

        public bool CanUseDarkAmoledTheme 
            => AppSettings.AppTheme == AppThemeType.DARK;

        public bool UseDarkAmoledTheme
        {
            get => AppSettings.UseDarkAmoledTheme;
            set
            {
                AppSettings.UseDarkAmoledTheme = value;
                RaisePropertyChanged(() => UseDarkAmoledTheme);
                Messenger.Publish(new AppThemeChangedMsg(this, AppSettings.AppTheme, AppSettings.AppHexAccentColor, value));
            }
        }

        public MvxColor SelectedAccentColor
        {
            get
            {
                var currentColor = AppSettings.AppHexAccentColor.ToColor();
                var mvxColor = new MvxColor(currentColor.R, currentColor.G, currentColor.B, currentColor.A);
                return AccentColors.First(color => color.ARGB == mvxColor.ARGB);
            }
            set
            {
                var selectedColor = Color.FromArgb(value.ARGB).ToHexString();
                if (AppSettings.AppHexAccentColor == selectedColor)
                    return;
                AppSettings.AppHexAccentColor = selectedColor;
                Messenger.Publish(new AppThemeChangedMsg(this, AppSettings.AppTheme, selectedColor, AppSettings.UseDarkAmoledTheme));
                //AccentColorChanged = true;
                //RaisePropertyChanged(() => SelectedAccentColor);
            }
        }
        //TODO: USE THIS PROPERTY TO LET THE USER NOW THAT IT NEEDS TO RESTART THE APP ?
        //public bool AccentColorChanged
        //{
        //    get => _accentColorChanged;
        //    set => Set(ref _accentColorChanged, value);
        //}

        public ItemModel SelectedTaskListSortOrder
        {
            get
            {
                var currentSelectedSortType = AppSettings.DefaultTaskListSortOrder;
                _selectedTaskListSortOrder = TaskListSortTypes.FirstOrDefault(i => i.ItemId == currentSelectedSortType.ToString());
                return _selectedTaskListSortOrder;
            }
            set
            {
                var selectedSortType = (TaskListSortType)Enum.Parse(typeof(TaskListSortType), value.ItemId, true);
                AppSettings.DefaultTaskListSortOrder = selectedSortType;
                SetProperty(ref _selectedTaskListSortOrder, value);
                Messenger.Publish(new TaskListSortOrderChangedMsg(this, selectedSortType));
            }
        }

        public ItemModel SelectedTaskSortOrder
        {
            get
            {
                var currentSelectedSortType = AppSettings.DefaultTaskSortOrder;
                _selectedTaskSortOrder = TasksSortTypes.FirstOrDefault(i => i.ItemId == currentSelectedSortType.ToString());
                return _selectedTaskSortOrder;
            }
            set
            {
                var selectedSortType = (TaskSortType)Enum.Parse(typeof(TaskSortType), value.ItemId, true);
                AppSettings.DefaultTaskSortOrder = selectedSortType;
                SetProperty(ref _selectedTaskSortOrder, value);
                Messenger.Publish(new TaskSortOrderChangedMsg(this, selectedSortType));
            }
        }

        public bool AskForPasswordWhenAppStarts
        {
            get => AppSettings.AskForPasswordWhenAppStarts;
            set
            {
                AppSettings.AskForPasswordWhenAppStarts = value;
                RaisePropertyChanged(() => AskForPasswordWhenAppStarts);
            }
        }

        public bool AskForFingerPrintWhenAppStarts
        {
            get => AppSettings.AskForFingerPrintWhenAppStarts;
            set
            {
                AppSettings.AskForFingerPrintWhenAppStarts = value;
                RaisePropertyChanged(() => AskForFingerPrintWhenAppStarts);
            }
        }

        public ItemModel SelectedAppLanguage
        {
            get
            {
                var currentAppLangue = AppSettings.AppLanguage;
                return AppLanguges.First(l => l.ItemId == currentAppLangue.ToString());
            }
            set
            {
                var selectedLanguage = (AppLanguageType)Enum.Parse(typeof(AppLanguageType), value.ItemId, true);
                if (AppSettings.AppLanguage == selectedLanguage)
                    return;
                AppSettings.AppLanguage = selectedLanguage;
                TextProvider.SetLanguage(selectedLanguage);
            }
        }

        public bool ShowCompletedTasks
        {
            get => AppSettings.ShowCompletedTasks;
            set => AppSettings.ShowCompletedTasks = value;
        }

        public bool AskBeforeDiscardChanges
        {
            get => AppSettings.AskBeforeDiscardChanges;
            set
            {
                AppSettings.AskBeforeDiscardChanges = value;
                RaisePropertyChanged(() => AskBeforeDiscardChanges);
            }
        }
        #endregion

        #region Commands
        public IMvxCommand<MvxColor> AccentColorChangedCommand { get; private set; }
        public IMvxAsyncCommand AskForPasswordWhenAppStartsCommand { get; private set; }
        public IMvxAsyncCommand AskForFingerPrintWhenAppStartsCommand { get; private set; }
        public IMvxCommand AskBeforeDiscardChangesCommand { get; private set; }
        public IMvxCommand UseDarkAmoledThemeCommand { get; private set; }
        #endregion

        public SettingsGeneralViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxNavigationService navigationService,
            ILogger logger,
            IAppSettingsService appSettings,
            IFingerprint fingerprint,
            IDialogService dialogService,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.ForContext<SettingsMainViewModel>(), navigationService, appSettings, telemetryService)
        {
            AccentColors = AppConstants.AppAccentColors
                .Select(hex => hex.ToColor())
                .Select(color => new MvxColor(color.R, color.G, color.B, color.A))
                .ToList();

            _fingerprintService = fingerprint;
            _dialogService = dialogService;
        }

        public override void SetCommands()
        {
            base.SetCommands();
            AccentColorChangedCommand = new MvxCommand<MvxColor>(color =>
            {
                _onAccentColorSelected.Raise(color);
                SelectedAccentColor = color;
            });
            AskForPasswordWhenAppStartsCommand = new MvxAsyncCommand(async () =>
            {
                var prompt = !AskForPasswordWhenAppStarts;

                if (prompt)
                {
                    var result = await NavigationService.Navigate<PasswordDialogViewModel, bool, bool>(false);
                    AskForPasswordWhenAppStarts = result;

                    if (result)
                        AskForFingerPrintWhenAppStarts = false;
                }
                else
                {
                    AskForPasswordWhenAppStarts = prompt;
                }
                Messenger.Publish(new HideKeyboardMsg(this));
            });
            AskForFingerPrintWhenAppStartsCommand = new MvxAsyncCommand(async () =>
            {
                var value = !AskForFingerPrintWhenAppStarts;
                var fingerprintAvailability = await _fingerprintService.GetAvailabilityAsync();

                if (value && fingerprintAvailability != FingerprintAvailability.Available)
                {
                    Logger.Warning($"Fingerprint is not available. Current value = {fingerprintAvailability}");
                    _dialogService.ShowWarningToast(GetText("FingerprintIsNotAvailable"));
                    AskForPasswordWhenAppStarts = false;
                }
                else
                {
                    AskForFingerPrintWhenAppStarts = value;
                }
            });
            AskBeforeDiscardChangesCommand = new MvxCommand(
                () => AskBeforeDiscardChanges = !AskBeforeDiscardChanges);
            UseDarkAmoledThemeCommand = new MvxCommand(
                () => UseDarkAmoledTheme = !UseDarkAmoledTheme);
        }
    }
}