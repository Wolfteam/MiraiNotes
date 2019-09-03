using MiraiNotes.Abstractions.Services;
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
        private readonly IAppSettingsService _appSettings;
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
                var currentSelectedTheme = _appSettings.AppTheme;
                return AppThemes.First(theme => theme.ItemId == currentSelectedTheme.ToString());
                //return _selectedAppTheme;
            }
            set
            {
                var selectedTheme = (AppThemeType)Enum.Parse(typeof(AppThemeType), value.ItemId, true);
                if (_appSettings.AppTheme == selectedTheme)
                    return;
                _appSettings.AppTheme = selectedTheme;
                RaisePropertyChanged(() => SelectedAppTheme);
                //SetProperty(ref _selectedAppTheme, value);
                //MiscellaneousUtils.ChangeCurrentTheme(selectedTheme, _appSettings.AppHexAccentColor);
            }
        }

        public MvxColor SelectedAccentColor
        {
            get
            {
                var currentColor = _appSettings.AppHexAccentColor.ToColor();
                var mvxColor = new MvxColor(currentColor.R, currentColor.G, currentColor.B, currentColor.A);
                return AccentColors.First(color => color.ARGB == mvxColor.ARGB);
            }
            set
            {
                var selectedColor = Color.FromArgb(value.ARGB).ToHexString();
                if (_appSettings.AppHexAccentColor == selectedColor)
                    return;
                _appSettings.AppHexAccentColor = selectedColor;
                //MiscellaneousUtils.ChangeCurrentTheme(_appSettings.AppTheme, _appSettings.AppHexAccentColor);
                //AccentColorChanged = true;
                //RaisePropertyChanged(() => SelectedAccentColor);
            }
        }

        //public bool AccentColorChanged
        //{
        //    get => _accentColorChanged;
        //    set => Set(ref _accentColorChanged, value);
        //}

        public ItemModel SelectedTaskListSortOrder
        {
            get
            {
                var currentSelectedSortType = _appSettings.DefaultTaskListSortOrder;
                _selectedTaskListSortOrder = TaskListSortTypes.FirstOrDefault(i => i.ItemId == currentSelectedSortType.ToString());
                return _selectedTaskListSortOrder;
            }
            set
            {
                var selectedSortType = (TaskListSortType)Enum.Parse(typeof(TaskListSortType), value.ItemId, true);
                _appSettings.DefaultTaskListSortOrder = selectedSortType;
                SetProperty(ref _selectedTaskListSortOrder, value);
                //_messenger.Send(selectedSortType, $"{MessageType.DEFAULT_TASK_LIST_SORT_ORDER_CHANGED}");
            }
        }

        public ItemModel SelectedTaskSortOrder
        {
            get
            {
                var currentSelectedSortType = _appSettings.DefaultTaskSortOrder;
                _selectedTaskSortOrder = TasksSortTypes.FirstOrDefault(i => i.ItemId == currentSelectedSortType.ToString());
                return _selectedTaskSortOrder;
            }
            set
            {
                var selectedSortType = (TaskSortType)Enum.Parse(typeof(TaskSortType), value.ItemId, true);
                _appSettings.DefaultTaskSortOrder = selectedSortType;
                SetProperty(ref _selectedTaskSortOrder, value);
                //_messenger.Send(selectedSortType, $"{MessageType.DEFAULT_TASK_SORT_ORDER_CHANGED}");
            }
        }

        public bool AskForPasswordWhenAppStarts
        {
            get => _appSettings.AskForPasswordWhenAppStarts;
            set
            {
                _appSettings.AskForPasswordWhenAppStarts = value;
                //if (value)
                //{
                //    //_dispatcher.CheckBeginInvokeOnUi(async () =>
                //    //{
                //    //    bool isPasswordSaved = await _dialogService.ShowCustomDialog(CustomDialogType.PASSWORD_DIALOG);
                //    //    _appSettings.AskForPasswordWhenAppStarts = isPasswordSaved;
                //    //    RaisePropertyChanged(nameof(AskForPasswordWhenAppStarts));
                //    //});
                //}
                //else
                //    _appSettings.AskForPasswordWhenAppStarts = value;
            }
        }

        public bool ShowCompletedTasks
        {
            get => _appSettings.ShowCompletedTasks;
            set => _appSettings.ShowCompletedTasks = value;
        }

        #endregion

        #region Commands
        public IMvxCommand<MvxColor> AccentColorChangedCommand { get; private set; }
        #endregion


        public SettingsGeneralViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            IDialogService dialogService)
            : base(textProvider, messenger)
        {
            _navigationService = navigationService;
            _appSettings = appSettings;
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
        }
    }
}