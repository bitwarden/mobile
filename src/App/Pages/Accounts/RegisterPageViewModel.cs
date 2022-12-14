﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Bit.App.Abstractions;
using Bit.App.Controls;
using Bit.App.Resources;
using Bit.Core;
using Bit.Core.Abstractions;
using Bit.Core.Enums;
using Bit.Core.Exceptions;
using Bit.Core.Models.Request;
using Bit.Core.Utilities;
using Xamarin.Forms;

namespace Bit.App.Pages
{
    public class RegisterPageViewModel : CaptchaProtectedViewModel
    {
        private readonly IDeviceActionService _deviceActionService;
        private readonly II18nService _i18nService;
        private readonly IEnvironmentService _environmentService;
        private readonly IApiService _apiService;
        private readonly ICryptoService _cryptoService;
        private readonly IPlatformUtilsService _platformUtilsService;
        private string _email;
        private string _masterPassword;
        private bool _showPassword;
        private bool _acceptPolicies;

        public RegisterPageViewModel()
        {
            _deviceActionService = ServiceContainer.Resolve<IDeviceActionService>("deviceActionService");
            _apiService = ServiceContainer.Resolve<IApiService>("apiService");
            _cryptoService = ServiceContainer.Resolve<ICryptoService>("cryptoService");
            _platformUtilsService = ServiceContainer.Resolve<IPlatformUtilsService>("platformUtilsService");
            _i18nService = ServiceContainer.Resolve<II18nService>("i18nService");
            _environmentService = ServiceContainer.Resolve<IEnvironmentService>("environmentService");

            PageTitle = AppResources.CreateAccount;
            TogglePasswordCommand = new Command(TogglePassword);
            ToggleConfirmPasswordCommand = new Command(ToggleConfirmPassword);
            SubmitCommand = new Command(async () => await SubmitAsync());
            ShowTerms = !_platformUtilsService.IsSelfHost();
        }

        public ICommand PoliciesClickCommand => new Command<string>((url) =>
        {
            _platformUtilsService.LaunchUri(url);
        });

        public bool ShowPassword
        {
            get => _showPassword;
            set => SetProperty(ref _showPassword, value,
                additionalPropertyNames: new string[]
                {
                    nameof(ShowPasswordIcon),
                    nameof(PasswordVisibilityAccessibilityText)
                });
        }

        public bool AcceptPolicies
        {
            get => _acceptPolicies;
            set => SetProperty(ref _acceptPolicies, value);
        }

        public string MasterPassword
        {
            get => _masterPassword;
            set => SetProperty(ref _masterPassword, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public FormattedString MasterPasswordDescription
        {
            get
            {
                var fs = new FormattedString();
                fs.Spans.Add(new Span
                {
                    Text = string.Format("{0}: ", AppResources.Important),
                    TextColor = Utilities.ThemeManager.GetResourceColor("InfoColor"),

                });
                fs.Spans.Add(new Span
                {
                    Text = AppResources.YourMasterPasswordCannotBeRecoveredIfYouForgetIt8CharacterMinimum,
                    TextColor = Utilities.ThemeManager.GetResourceColor("MutedColor"),
                    FontAttributes = FontAttributes.None
                });
                return fs;
            }
        }

        public bool ShowTerms { get; set; }
        public Command SubmitCommand { get; }
        public Command TogglePasswordCommand { get; }
        public Command ToggleConfirmPasswordCommand { get; }
        public string ShowPasswordIcon => ShowPassword ? BitwardenIcons.EyeSlash : BitwardenIcons.Eye;
        public string PasswordVisibilityAccessibilityText => ShowPassword ? AppResources.PasswordIsVisibleTapToHide : AppResources.PasswordIsNotVisibleTapToShow;
        public string Name { get; set; }
        public PasswordStrengthValue MasterPasswordStrength { get; set; }
        public string ConfirmMasterPassword { get; set; }
        public string Hint { get; set; }
        public Action RegistrationSuccess { get; set; }
        public Action CloseAction { get; set; }
        protected override II18nService i18nService => _i18nService;
        protected override IEnvironmentService environmentService => _environmentService;
        protected override IDeviceActionService deviceActionService => _deviceActionService;
        protected override IPlatformUtilsService platformUtilsService => _platformUtilsService;

        public async Task SubmitAsync(bool showLoading = true)
        {
            if (Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.None)
            {
                await _platformUtilsService.ShowDialogAsync(AppResources.InternetConnectionRequiredMessage,
                    AppResources.InternetConnectionRequiredTitle, AppResources.Ok);
                return;
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                await _platformUtilsService.ShowDialogAsync(
                    string.Format(AppResources.ValidationFieldRequired, AppResources.EmailAddress),
                    AppResources.AnErrorHasOccurred,
                    AppResources.Ok);
                return;
            }
            if (!Email.Contains("@"))
            {
                await _platformUtilsService.ShowDialogAsync(AppResources.InvalidEmail, AppResources.AnErrorHasOccurred,
                    AppResources.Ok);
                return;
            }
            if (string.IsNullOrWhiteSpace(MasterPassword))
            {
                await _platformUtilsService.ShowDialogAsync(
                    string.Format(AppResources.ValidationFieldRequired, AppResources.MasterPassword),
                    AppResources.AnErrorHasOccurred,
                    AppResources.Ok);
                return;
            }
            if (MasterPassword.Length < 8)
            {
                await _platformUtilsService.ShowDialogAsync(AppResources.MasterPasswordLengthValMessage,
                    AppResources.AnErrorHasOccurred, AppResources.Ok);
                return;
            }
            if (MasterPassword != ConfirmMasterPassword)
            {
                await _platformUtilsService.ShowDialogAsync(AppResources.MasterPasswordConfirmationValMessage,
                    AppResources.AnErrorHasOccurred, AppResources.Ok);
                return;
            }
            if (ShowTerms && !AcceptPolicies)
            {
                await _platformUtilsService.ShowDialogAsync(AppResources.AcceptPoliciesError,
                    AppResources.AnErrorHasOccurred, AppResources.Ok);
                return;
            }
            if (MasterPasswordStrength == PasswordStrengthValue.Weak)
            {
                var accepted = await _platformUtilsService.ShowDialogAsync(AppResources.WeakPasswordIdentifiedUseAStrongPasswordToProtectYourAccount,
                    AppResources.WeakMasterPassword, AppResources.Yes, AppResources.No);
                if (!accepted)
                {
                    return;
                }
            }

            if (showLoading)
            {
                await _deviceActionService.ShowLoadingAsync(AppResources.CreatingAccount);
            }

            Name = string.IsNullOrWhiteSpace(Name) ? null : Name;
            Email = Email.Trim().ToLower();
            var kdf = KdfType.PBKDF2_SHA256;
            var kdfIterations = 100_000;
            var key = await _cryptoService.MakeKeyAsync(MasterPassword, Email, kdf, kdfIterations);
            var encKey = await _cryptoService.MakeEncKeyAsync(key);
            var hashedPassword = await _cryptoService.HashPasswordAsync(MasterPassword, key);
            var keys = await _cryptoService.MakeKeyPairAsync(encKey.Item1);
            var request = new RegisterRequest
            {
                Email = Email,
                Name = Name,
                MasterPasswordHash = hashedPassword,
                MasterPasswordHint = Hint,
                Key = encKey.Item2.EncryptedString,
                Kdf = kdf,
                KdfIterations = kdfIterations,
                Keys = new KeysRequest
                {
                    PublicKey = keys.Item1,
                    EncryptedPrivateKey = keys.Item2.EncryptedString
                },
                CaptchaResponse = _captchaToken,
            };
            // TODO: org invite?

            try
            {
                await _apiService.PostRegisterAsync(request);
                await _deviceActionService.HideLoadingAsync();
                _platformUtilsService.ShowToast("success", null, AppResources.AccountCreated,
                    new System.Collections.Generic.Dictionary<string, object>
                    {
                        ["longDuration"] = true
                    });
                RegistrationSuccess?.Invoke();
            }
            catch (ApiException e)
            {
                if (e?.Error != null && e.Error.CaptchaRequired)
                {
                    if (await HandleCaptchaAsync(e.Error.CaptchaSiteKey))
                    {
                        await SubmitAsync(false);
                        _captchaToken = null;
                    }
                    return;
                }
                await _deviceActionService.HideLoadingAsync();
                if (e?.Error != null)
                {
                    await _platformUtilsService.ShowDialogAsync(e.Error.GetSingleMessage(),
                        AppResources.AnErrorHasOccurred, AppResources.Ok);
                }
            }
        }

        public void TogglePassword()
        {
            ShowPassword = !ShowPassword;
            var entry = (Page as RegisterPage).MasterPasswordEntry;
            entry.Focus();
            entry.CursorPosition = String.IsNullOrEmpty(MasterPassword) ? 0 : MasterPassword.Length;
        }

        public void ToggleConfirmPassword()
        {
            ShowPassword = !ShowPassword;
            var entry = (Page as RegisterPage).ConfirmMasterPasswordEntry;
            entry.Focus();
            entry.CursorPosition = String.IsNullOrEmpty(ConfirmMasterPassword) ? 0 : ConfirmMasterPassword.Length;
        }
    }
}
