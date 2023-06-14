﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bit.App.Abstractions;
using Bit.App.Controls;
using Bit.App.Resources;
using Bit.App.Utilities;
using Bit.Core;
using Bit.Core.Abstractions;
using Bit.Core.Enums;
using Bit.Core.Exceptions;
using Bit.Core.Models.View;
using Bit.Core.Utilities;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Bit.App.Pages
{
    public class SendAddEditPageViewModel : BaseViewModel
    {
        private readonly IDeviceActionService _deviceActionService;
        private readonly IFileService _fileService;
        private readonly IPlatformUtilsService _platformUtilsService;
        private readonly IMessagingService _messagingService;
        private readonly IStateService _stateService;
        private readonly ISendService _sendService;
        private readonly ILogger _logger;
        private bool _sendEnabled = true;
        private bool _canAccessPremium;
        private bool _emailVerified;
        private SendView _send;
        private string _fileName;
        private bool _showOptions;
        private bool _showPassword;
        private int _deletionDateTypeSelectedIndex;
        private int _expirationDateTypeSelectedIndex;
        private DateTime _simpleDeletionDateTime;
        private DateTime? _simpleExpirationDateTime;
        private bool _isOverridingPickers;
        private int? _maxAccessCount;
        private string[] _additionalSendProperties = new[]
        {
            nameof(IsText),
            nameof(IsFile),
            nameof(FileTypeAccessibilityLabel),
            nameof(TextTypeAccessibilityLabel)
        };
        private bool _disableHideEmail;
        private bool _sendOptionsPolicyInEffect;
        private bool _copyInsteadOfShareAfterSaving;

        public SendAddEditPageViewModel()
        {
            _deviceActionService = ServiceContainer.Resolve<IDeviceActionService>("deviceActionService");
            _fileService = ServiceContainer.Resolve<IFileService>();
            _platformUtilsService = ServiceContainer.Resolve<IPlatformUtilsService>("platformUtilsService");
            _messagingService = ServiceContainer.Resolve<IMessagingService>("messagingService");
            _stateService = ServiceContainer.Resolve<IStateService>("stateService");
            _sendService = ServiceContainer.Resolve<ISendService>("sendService");
            _logger = ServiceContainer.Resolve<ILogger>("logger");

            TogglePasswordCommand = new Command(TogglePassword);
            ToggleOptionsCommand = new Command(ToggleOptions);

            TypeOptions = new List<KeyValuePair<string, SendType>>
            {
                new KeyValuePair<string, SendType>(AppResources.TypeText, SendType.Text),
                new KeyValuePair<string, SendType>(AppResources.TypeFile, SendType.File),
            };
            DeletionTypeOptions = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(AppResources.OneHour, AppResources.OneHour),
                new KeyValuePair<string, string>(AppResources.OneDay, AppResources.OneDay),
                new KeyValuePair<string, string>(AppResources.TwoDays, AppResources.TwoDays),
                new KeyValuePair<string, string>(AppResources.ThreeDays, AppResources.ThreeDays),
                new KeyValuePair<string, string>(AppResources.SevenDays, AppResources.SevenDays),
                new KeyValuePair<string, string>(AppResources.ThirtyDays, AppResources.ThirtyDays),
                new KeyValuePair<string, string>(AppResources.Custom, AppResources.Custom),
            };
            ExpirationTypeOptions = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(AppResources.Never, AppResources.Never),
                new KeyValuePair<string, string>(AppResources.OneHour, AppResources.OneHour),
                new KeyValuePair<string, string>(AppResources.OneDay, AppResources.OneDay),
                new KeyValuePair<string, string>(AppResources.TwoDays, AppResources.TwoDays),
                new KeyValuePair<string, string>(AppResources.ThreeDays, AppResources.ThreeDays),
                new KeyValuePair<string, string>(AppResources.SevenDays, AppResources.SevenDays),
                new KeyValuePair<string, string>(AppResources.ThirtyDays, AppResources.ThirtyDays),
                new KeyValuePair<string, string>(AppResources.Custom, AppResources.Custom),
            };

            DeletionDateTimeViewModel = new DateTimeViewModel(AppResources.DeletionDate, AppResources.DeletionTime);
            ExpirationDateTimeViewModel = new DateTimeViewModel(AppResources.ExpirationDate, AppResources.ExpirationTime)
            {
                OnDateChanged = date =>
                {
                    if (!_isOverridingPickers && !ExpirationDateTimeViewModel.Time.HasValue)
                    {
                        // auto-set time to current time upon setting date
                        ExpirationDateTimeViewModel.Time = DateTimeNow().TimeOfDay;
                    }
                },
                OnTimeChanged = time =>
                {
                    if (!_isOverridingPickers && !ExpirationDateTimeViewModel.Date.HasValue)
                    {
                        // auto-set date to current date upon setting time
                        ExpirationDateTimeViewModel.Date = DateTime.Today;
                    }
                },
                DatePlaceholder = "mm/dd/yyyy",
                TimePlaceholder = "--:-- --"
            };

            AccountSwitchingOverlayViewModel = new AccountSwitchingOverlayViewModel(_stateService, _messagingService, _logger);
        }

        public AccountSwitchingOverlayViewModel AccountSwitchingOverlayViewModel { get; }
        public Command TogglePasswordCommand { get; set; }
        public Command ToggleOptionsCommand { get; set; }
        public string SendId { get; set; }
        public int SegmentedButtonHeight { get; set; }
        public int SegmentedButtonFontSize { get; set; }
        public Thickness SegmentedButtonMargins { get; set; }
        public bool ShowEditorSeparators { get; set; }
        public Thickness EditorMargins { get; set; }
        public SendType? Type { get; set; }
        public byte[] FileData { get; set; }
        public string NewPassword { get; set; }
        public bool ShareOnSave { get; set; }
        public bool DisableHideEmailControl { get; set; }
        public bool IsAddFromShare { get; set; }
        public string ShareOnSaveText => CopyInsteadOfShareAfterSaving ? AppResources.CopySendLinkOnSave : AppResources.ShareOnSave;
        public string OptionsAccessibilityText => ShowOptions ? AppResources.OptionsExpanded : AppResources.OptionsCollapsed;
        public List<KeyValuePair<string, SendType>> TypeOptions { get; }
        public List<KeyValuePair<string, string>> DeletionTypeOptions { get; }
        public List<KeyValuePair<string, string>> ExpirationTypeOptions { get; }
        public bool SendEnabled
        {
            get => _sendEnabled;
            set => SetProperty(ref _sendEnabled, value);
        }
        public int DeletionDateTypeSelectedIndex
        {
            get => _deletionDateTypeSelectedIndex;
            set
            {
                if (SetProperty(ref _deletionDateTypeSelectedIndex, value))
                {
                    DeletionTypeChanged();
                }
            }
        }
        public bool ShowOptions
        {
            get => _showOptions;
            set => SetProperty(ref _showOptions, value,
                additionalPropertyNames: new[]
                {
                    nameof(OptionsAccessibilityText),
                    nameof(OptionsShowHideIcon)
                });
        }
        public int ExpirationDateTypeSelectedIndex
        {
            get => _expirationDateTypeSelectedIndex;
            set
            {
                if (SetProperty(ref _expirationDateTypeSelectedIndex, value))
                {
                    ExpirationTypeChanged();
                }
            }
        }

        public int? MaxAccessCount
        {
            get => _maxAccessCount;
            set
            {
                if (SetProperty(ref _maxAccessCount, value))
                {
                    MaxAccessCountChanged();
                }
            }
        }
        public bool CopyInsteadOfShareAfterSaving
        {
            get => _copyInsteadOfShareAfterSaving;
            set
            {
                SetProperty(ref _copyInsteadOfShareAfterSaving, value);
                TriggerPropertyChanged(nameof(ShareOnSaveText));
            }
        }
        public SendView Send
        {
            get => _send;
            set => SetProperty(ref _send, value, additionalPropertyNames: _additionalSendProperties);
        }
        public string FileName
        {
            get => _fileName ?? AppResources.NoFileChosen;
            set
            {
                if (SetProperty(ref _fileName, value))
                {
                    Send.File.FileName = _fileName;
                }
            }
        }
        public bool ShowPassword
        {
            get => _showPassword;
            set => SetProperty(ref _showPassword, value,
                additionalPropertyNames: new[]
                {
                    nameof(ShowPasswordIcon),
                    nameof(PasswordVisibilityAccessibilityText)
                });
        }
        public bool DisableHideEmail
        {
            get => _disableHideEmail;
            set => SetProperty(ref _disableHideEmail, value);
        }
        public bool SendOptionsPolicyInEffect
        {
            get => _sendOptionsPolicyInEffect;
            set => SetProperty(ref _sendOptionsPolicyInEffect, value);
        }
        public bool ShowTypeButtons => !EditMode && !IsAddFromShare;
        public bool EditMode => !string.IsNullOrWhiteSpace(SendId);
        public bool IsText => Send?.Type == SendType.Text;
        public bool IsFile => Send?.Type == SendType.File;
        public bool ShowDeletionCustomPickers => EditMode || DeletionDateTypeSelectedIndex == 6;
        public bool ShowExpirationCustomPickers => EditMode || ExpirationDateTypeSelectedIndex == 7;
        public DateTimeViewModel DeletionDateTimeViewModel { get; }
        public DateTimeViewModel ExpirationDateTimeViewModel { get; }
        public string ShowPasswordIcon => ShowPassword ? BitwardenIcons.EyeSlash : BitwardenIcons.Eye;
        public string PasswordVisibilityAccessibilityText => ShowPassword ? AppResources.PasswordIsVisibleTapToHide : AppResources.PasswordIsNotVisibleTapToShow;
        public string FileTypeAccessibilityLabel => IsFile ? AppResources.FileTypeIsSelected : AppResources.FileTypeIsNotSelected;
        public string TextTypeAccessibilityLabel => IsText ? AppResources.TextTypeIsSelected : AppResources.TextTypeIsNotSelected;
        public string OptionsShowHideIcon => ShowOptions ? BitwardenIcons.ChevronUp : BitwardenIcons.AngleDown;

        public async Task InitAsync()
        {
            PageTitle = EditMode ? AppResources.EditSend : AppResources.AddSend;
            _canAccessPremium = await _stateService.CanAccessPremiumAsync();
            _emailVerified = await _stateService.GetEmailVerifiedAsync();
            SendEnabled = !await AppHelpers.IsSendDisabledByPolicyAsync();
            DisableHideEmail = await AppHelpers.IsHideEmailDisabledByPolicyAsync();
            SendOptionsPolicyInEffect = SendEnabled && DisableHideEmail;
        }

        public async Task<bool> LoadAsync()
        {
            if (Send == null)
            {
                _isOverridingPickers = true;
                if (EditMode)
                {
                    var send = await _sendService.GetAsync(SendId);
                    if (send == null)
                    {
                        return false;
                    }
                    Send = await send.DecryptAsync();
                    DeletionDateTimeViewModel.DateTime = Send.DeletionDate.ToLocalTime();
                    ExpirationDateTimeViewModel.DateTime = Send.ExpirationDate?.ToLocalTime();
                }
                else
                {
                    var defaultType = _canAccessPremium && _emailVerified ? SendType.File : SendType.Text;
                    Send = new SendView
                    {
                        Type = Type.GetValueOrDefault(defaultType),
                    };
                    DeletionDateTimeViewModel.DateTime = DateTimeNow().AddDays(7);
                    DeletionDateTypeSelectedIndex = 4;
                    ExpirationDateTypeSelectedIndex = 0;
                }

                MaxAccessCount = Send.MaxAccessCount;
                _isOverridingPickers = false;
            }

            DisableHideEmailControl = !SendEnabled ||
                (!EditMode && DisableHideEmail) ||
                (EditMode && DisableHideEmail && !Send.HideEmail);

            return true;
        }

        public async Task ChooseFileAsync()
        {
            await _fileService.SelectFileAsync();
        }

        public void ClearExpirationDate()
        {
            _isOverridingPickers = true;
            ExpirationDateTimeViewModel.DateTime = null;
            _isOverridingPickers = false;
        }

        private void UpdateSendData()
        {
            // filename
            if (Send.File != null && _fileName != null)
            {
                Send.File.FileName = _fileName;
            }

            // deletion date
            if (ShowDeletionCustomPickers)
            {
                Send.DeletionDate = DeletionDateTimeViewModel.DateTime.Value.ToUniversalTime();
            }
            else
            {
                Send.DeletionDate = _simpleDeletionDateTime.ToUniversalTime();
            }

            // expiration date
            if (ShowExpirationCustomPickers && ExpirationDateTimeViewModel.DateTime.HasValue)
            {
                Send.ExpirationDate = ExpirationDateTimeViewModel.DateTime.Value.ToUniversalTime();
            }
            else if (_simpleExpirationDateTime.HasValue)
            {
                Send.ExpirationDate = _simpleExpirationDateTime.Value.ToUniversalTime();
            }
            else
            {
                Send.ExpirationDate = null;
            }
        }

        public async Task<bool> SubmitAsync()
        {
            if (Send == null || !SendEnabled)
            {
                return false;
            }
            if (Connectivity.NetworkAccess == NetworkAccess.None)
            {
                await _platformUtilsService.ShowDialogAsync(AppResources.InternetConnectionRequiredMessage,
                    AppResources.InternetConnectionRequiredTitle);
                return false;
            }
            if (string.IsNullOrWhiteSpace(Send.Name))
            {
                await Page.DisplayAlert(AppResources.AnErrorHasOccurred,
                    string.Format(AppResources.ValidationFieldRequired, AppResources.Name),
                    AppResources.Ok);
                return false;
            }
            if (IsFile)
            {
                if (!_canAccessPremium)
                {
                    await _platformUtilsService.ShowDialogAsync(AppResources.SendFilePremiumRequired);
                    return false;
                }
                if (!_emailVerified)
                {
                    await _platformUtilsService.ShowDialogAsync(AppResources.SendFileEmailVerificationRequired);
                    return false;
                }
                if (!EditMode)
                {
                    if (FileData == null)
                    {
                        await _platformUtilsService.ShowDialogAsync(
                            string.Format(AppResources.ValidationFieldRequired, AppResources.File),
                            AppResources.AnErrorHasOccurred);
                        return false;
                    }
                    if (FileData.Length > 104857600) // 100 MB
                    {
                        await _platformUtilsService.ShowDialogAsync(AppResources.MaxFileSize,
                            AppResources.AnErrorHasOccurred);
                        return false;
                    }
                }
            }

            UpdateSendData();

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                NewPassword = null;
            }

            var (send, encryptedFileData) = await _sendService.EncryptAsync(Send, FileData, NewPassword);
            if (send == null)
            {
                return false;
            }
            try
            {
                await _deviceActionService.ShowLoadingAsync(AppResources.Saving);
                var sendId = await _sendService.SaveWithServerAsync(send, encryptedFileData);
                await _deviceActionService.HideLoadingAsync();

                if (Device.RuntimePlatform == Device.Android && IsFile)
                {
                    // Workaround for https://github.com/xamarin/Xamarin.Forms/issues/5418
                    // Exiting and returning (file picker) calls OnAppearing on list page instead of this modal, and
                    // it doesn't get called again when the model is dismissed, so the list isn't updated.
                    _messagingService.Send("sendUpdated");
                }

                if (!ShareOnSave)
                {
                    _platformUtilsService.ShowToast("success", null,
                    EditMode ? AppResources.SendUpdated : AppResources.NewSendCreated);
                }

                if (!CopyInsteadOfShareAfterSaving)
                {
                    await CloseAsync();
                }

                if (ShareOnSave)
                {
                    var savedSend = await _sendService.GetAsync(sendId);
                    if (savedSend != null)
                    {
                        var savedSendView = await savedSend.DecryptAsync();
                        if (CopyInsteadOfShareAfterSaving)
                        {
                            await AppHelpers.CopySendUrlAsync(savedSendView);

                            // wait so that the user sees the message before the view gets dismissed
                            await Task.Delay(1300);
                        }
                        else
                        {
                            await AppHelpers.ShareSendUrlAsync(savedSendView);
                        }
                    }
                }

                if (CopyInsteadOfShareAfterSaving)
                {
                    await CloseAsync();
                }

                return true;
            }
            catch (ApiException e)
            {
                await _deviceActionService.HideLoadingAsync();
                if (e?.Error != null)
                {
                    await _platformUtilsService.ShowDialogAsync(e.Error.GetSingleMessage(),
                        AppResources.AnErrorHasOccurred);
                }
            }
            catch (Exception ex)
            {
                await _deviceActionService.HideLoadingAsync();
                _logger.Exception(ex);
                await _platformUtilsService.ShowDialogAsync(AppResources.AnErrorHasOccurred);
            }
            return false;
        }

        private async Task CloseAsync()
        {
            if (IsAddFromShare)
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    _deviceActionService.CloseMainApp();
                    return;
                }

                if (Page is SendAddOnlyPage sendPage && sendPage.OnClose != null)
                {
                    sendPage.OnClose();
                    return;
                }
            }

            await Page.Navigation.PopModalAsync();
        }

        public async Task<bool> RemovePasswordAsync()
        {
            return await AppHelpers.RemoveSendPasswordAsync(SendId);
        }

        public async Task CopyLinkAsync()
        {
            await AppHelpers.CopySendUrlAsync(Send);
        }

        public async Task ShareLinkAsync()
        {
            await AppHelpers.ShareSendUrlAsync(Send);
        }

        public async Task<bool> DeleteAsync()
        {
            return await AppHelpers.DeleteSendAsync(SendId);
        }

        public async Task TypeChangedAsync(SendType type)
        {
            if (!SendEnabled)
            {
                await _platformUtilsService.ShowDialogAsync(AppResources.SendDisabledWarning);
                await CloseAsync();
                return;
            }
            if (Send != null)
            {
                if (!EditMode && type == SendType.File && (!_canAccessPremium || !_emailVerified))
                {
                    if (!_canAccessPremium)
                    {
                        await _platformUtilsService.ShowDialogAsync(AppResources.SendFilePremiumRequired);
                    }
                    else if (!_emailVerified)
                    {
                        await _platformUtilsService.ShowDialogAsync(AppResources.SendFileEmailVerificationRequired);
                    }

                    if (IsAddFromShare && Device.RuntimePlatform == Device.Android)
                    {
                        _deviceActionService.CloseMainApp();
                        return;
                    }
                    type = SendType.Text;
                }
                Send.Type = type;
                TriggerPropertyChanged(nameof(Send), _additionalSendProperties);
            }
        }

        public void ToggleOptions()
        {
            ShowOptions = !ShowOptions;
        }

        private void DeletionTypeChanged()
        {
            if (Send != null && DeletionDateTypeSelectedIndex > -1)
            {
                _isOverridingPickers = true;
                switch (DeletionDateTypeSelectedIndex)
                {
                    case 0:
                        _simpleDeletionDateTime = DateTimeNow().AddHours(1);
                        break;
                    case 1:
                        _simpleDeletionDateTime = DateTimeNow().AddDays(1);
                        break;
                    case 2:
                        _simpleDeletionDateTime = DateTimeNow().AddDays(2);
                        break;
                    case 3:
                        _simpleDeletionDateTime = DateTimeNow().AddDays(3);
                        break;
                    case 4:
                        _simpleDeletionDateTime = DateTimeNow().AddDays(7);
                        break;
                    case 5:
                        _simpleDeletionDateTime = DateTimeNow().AddDays(30);
                        break;
                    case 6:
                        // custom option, initial values already set elsewhere
                        break;
                }
                _isOverridingPickers = false;
                TriggerPropertyChanged(nameof(ShowDeletionCustomPickers));
            }
        }

        private void ExpirationTypeChanged()
        {
            if (Send != null && ExpirationDateTypeSelectedIndex > -1)
            {
                _isOverridingPickers = true;
                switch (ExpirationDateTypeSelectedIndex)
                {
                    case 0:
                        _simpleExpirationDateTime = null;
                        break;
                    case 1:
                        _simpleExpirationDateTime = DateTimeNow().AddHours(1);
                        break;
                    case 2:
                        _simpleExpirationDateTime = DateTimeNow().AddDays(1);
                        break;
                    case 3:
                        _simpleExpirationDateTime = DateTimeNow().AddDays(2);
                        break;
                    case 4:
                        _simpleExpirationDateTime = DateTimeNow().AddDays(3);
                        break;
                    case 5:
                        _simpleExpirationDateTime = DateTimeNow().AddDays(7);
                        break;
                    case 6:
                        _simpleExpirationDateTime = DateTimeNow().AddDays(30);
                        break;
                    case 7:
                        // custom option, clear all expiration values
                        _simpleExpirationDateTime = null;
                        ClearExpirationDate();
                        break;
                }
                _isOverridingPickers = false;
                TriggerPropertyChanged(nameof(ShowExpirationCustomPickers));
            }
        }

        private void MaxAccessCountChanged()
        {
            Send.MaxAccessCount = _maxAccessCount;
        }

        private void TogglePassword()
        {
            ShowPassword = !ShowPassword;
        }

        private DateTime DateTimeNow()
        {
            var dtn = DateTime.Now;
            return new DateTime(
                dtn.Year,
                dtn.Month,
                dtn.Day,
                dtn.Hour,
                dtn.Minute,
                0,
                DateTimeKind.Local
            );
        }

        internal void TriggerSendTextPropertyChanged()
        {
            Device.BeginInvokeOnMainThread(() => TriggerPropertyChanged(nameof(Send)));
        }
    }
}
