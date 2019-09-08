using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MiraiNotes.Shared;
using MiraiNotes.Shared.Utils;
using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.UI;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Settings
{
    public class SettingsGeneralViewModel : BaseViewModel
    {
        #region Members
        private readonly IMvxNavigationService _navigationService;
        private readonly IDialogService _dialogService;

        private bool _isBackButtonVisible;
        private string _currentPageText;
        private bool _accentColorChanged;
        private ItemModel _selectedAppTheme;
        private MvxColor _selectedAccentColor;
        private ItemModel _selectedTaskListSortOrder;
        private ItemModel _selectedTaskSortOrder;
        private readonly MvxInteraction<SettingsPageType> _onSettingItemSelected = new MvxInteraction<SettingsPageType>();
        private readonly MvxInteraction<MvxColor> _onAccentColorSelected = new MvxInteraction<MvxColor>();
        #endregion

        #region Properties
        public IMvxInteraction<MvxColor> OnAccentColorSelected
            => _onAccentColorSelected;

        public MvxObservableCollection<ItemModel> AppThemes => new MvxObservableCollection<ItemModel>
        {
            new ItemModel
            {
                ItemId = AppThemeType.DARK.ToString(),
                Text = "Dark"
            },
            new ItemModel
            {
                ItemId = AppThemeType.LIGHT.ToString(),
                Text = "Light"
            }
        };

        public List<MvxColor> AccentColors { get; }

        public List<ItemModel> TaskListSortTypes => new List<ItemModel>
        {
            new ItemModel
            {
                ItemId = TaskListSortType.BY_NAME_ASC.ToString(),
                Text = "By name asc"
            },
            new ItemModel
            {
                ItemId = TaskListSortType.BY_NAME_DESC.ToString(),
                Text = "By name desc"
            },
            new ItemModel
            {
                ItemId = TaskListSortType.BY_UPDATED_DATE_ASC.ToString(),
                Text = "By updated date asc"
            },
            new ItemModel
            {
                ItemId = TaskListSortType.BY_UPDATED_DATE_DESC.ToString(),
                Text = "By updated date desc"
            }
        };

        public List<ItemModel> TasksSortTypes => new List<ItemModel>
        {
            new ItemModel
            {
                ItemId = TaskSortType.BY_NAME_ASC.ToString(),
                Text = "By name asc"
            },
            new ItemModel
            {
                ItemId = TaskSortType.BY_NAME_DESC.ToString(),
                Text = "By name desc"
            },
            new ItemModel
            {
                ItemId = TaskSortType.BY_UPDATED_DATE_ASC.ToString(),
                Text = "By updated date asc"
            },
            new ItemModel
            {
                ItemId = TaskSortType.BY_UPDATED_DATE_DESC.ToString(),
                Text = "By updated date desc"
            }
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
                Messenger.Publish(new AppThemeChangedMsg(this, selectedTheme, AppSettings.AppHexAccentColor));
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
                Messenger.Publish(new AppThemeChangedMsg(this, AppSettings.AppTheme, selectedColor));
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
                //if (value)
                //{
                //    //_dispatcher.CheckBeginInvokeOnUi(async () =>
                //    //{
                //    //    bool isPasswordSaved = await _dialogService.ShowCustomDialog(CustomDialogType.PASSWORD_DIALOG);
                //    //    AppSettings.AskForPasswordWhenAppStarts = isPasswordSaved;
                //    //    RaisePropertyChanged(nameof(AskForPasswordWhenAppStarts));
                //    //});
                //}
                //else
                //    AppSettings.AskForPasswordWhenAppStarts = value;
            }
        }

        public bool ShowCompletedTasks
        {
            get => AppSettings.ShowCompletedTasks;
            set => AppSettings.ShowCompletedTasks = value;
        }

        //TODO: LANGUAGE
        #endregion

        #region Commands
        public IMvxCommand<MvxColor> AccentColorChangedCommand { get; private set; }
        public IMvxCommand AskForPasswordWhenAppStartsCommand { get; private set; }
        #endregion


        public SettingsGeneralViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            IDialogService dialogService)
            : base(textProvider, messenger, appSettings)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;

            AccentColors = AppConstants.AppAccentColors
                .Select(hex => hex.ToColor())
                .Select(color => new MvxColor(color.R, color.G, color.B, color.A))
                .ToList();

            SetCommands();
        }

        private void SetCommands()
        {
            AccentColorChangedCommand = new MvxCommand<MvxColor>(color =>
            {
                _dialogService.ShowWarningToast("Hiciste click en " + color.ARGB);
                _onAccentColorSelected.Raise(color);
                SelectedAccentColor = color;
            });
            AskForPasswordWhenAppStartsCommand = new MvxCommand(
                () => AskForPasswordWhenAppStarts = !AskForPasswordWhenAppStarts);
        }
    }
}