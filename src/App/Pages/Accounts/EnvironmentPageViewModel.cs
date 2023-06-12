using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Bit.App.Resources;
using Bit.Core.Abstractions;
using Bit.Core.Models.Data;
using Bit.Core.Utilities;
using Newtonsoft.Json;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Bit.App.Pages
{
    public class EnvironmentPageViewModel : BaseViewModel
    {
        private readonly IEnvironmentService _environmentService;
        readonly LazyResolve<ILogger> _logger = new LazyResolve<ILogger>("logger");

        public EnvironmentPageViewModel()
        {
            _environmentService = ServiceContainer.Resolve<IEnvironmentService>("environmentService");

            PageTitle = AppResources.Settings;
            BaseUrl = _environmentService.BaseUrl == EnvironmentUrlData.DefaultEU.Base || EnvironmentUrlData.DefaultUS.Base == _environmentService.BaseUrl ?
                string.Empty : _environmentService.BaseUrl;
            WebVaultUrl = _environmentService.WebVaultUrl;
            ApiUrl = _environmentService.ApiUrl;
            IdentityUrl = _environmentService.IdentityUrl;
            IconsUrl = _environmentService.IconsUrl;
            NotificationsUrls = _environmentService.NotificationsUrl;
            SubmitCommand = new AsyncCommand(SubmitAsync, onException: ex => OnSubmitException(ex), allowsMultipleExecutions: false);
            LoadFromFileCommand = new AsyncCommand(LoadEnvironmentsFromFile, onException: ex => OnSubmitException(ex), allowsMultipleExecutions: false);
            ClearCommand = new Command(ClearAllUrls);
        }

        public ICommand SubmitCommand { get; }
        public ICommand LoadFromFileCommand { get; }
        public ICommand ClearCommand { get; }
        public string BaseUrl { get; set; }
        public string ApiUrl { get; set; }
        public string IdentityUrl { get; set; }
        public string WebVaultUrl { get; set; }
        public string IconsUrl { get; set; }
        public string NotificationsUrls { get; set; }
        public Action SubmitSuccessAction { get; set; }
        public Action CloseAction { get; set; }

        public async Task SubmitAsync()
        {
            if (!ValidateUrls())
            {
                await Page.DisplayAlert(AppResources.AnErrorHasOccurred, AppResources.EnvironmentPageUrlsError, AppResources.Ok);
                return;
            }

            var resUrls = await _environmentService.SetUrlsAsync(new Core.Models.Data.EnvironmentUrlData
            {
                Base = BaseUrl,
                Api = ApiUrl,
                Identity = IdentityUrl,
                WebVault = WebVaultUrl,
                Icons = IconsUrl,
                Notifications = NotificationsUrls
            });

            // re-set urls since service can change them, ex: prefixing https://
            BaseUrl = resUrls.Base;
            WebVaultUrl = resUrls.WebVault;
            ApiUrl = resUrls.Api;
            IdentityUrl = resUrls.Identity;
            IconsUrl = resUrls.Icons;
            NotificationsUrls = resUrls.Notifications;

            SubmitSuccessAction?.Invoke();
        }

        public bool ValidateUrls()
        {
            bool IsUrlValid(string url)
            {
                return string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
            }

            return IsUrlValid(BaseUrl)
                && IsUrlValid(ApiUrl)
                && IsUrlValid(IdentityUrl)
                && IsUrlValid(WebVaultUrl)
                && IsUrlValid(IconsUrl);
        }

        private void OnSubmitException(Exception ex)
        {
            _logger.Value.Exception(ex);
            Page.DisplayAlert(AppResources.AnErrorHasOccurred, AppResources.GenericErrorMessage, AppResources.Ok);
        }

        private async Task LoadEnvironmentsFromFile()
        {
            try
            {
                string jsonString;
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "This a test to pick files"
                });
                if (result != null)
                {
                    if (result.FileName.EndsWith("json", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("txt", StringComparison.OrdinalIgnoreCase))
                    {
                        var stream = await result.OpenReadAsync();
                        using (var reader = new System.IO.StreamReader(stream))
                        {
                            jsonString = reader.ReadToEnd();
                        }
                        var envUrls = JsonConvert.DeserializeObject<EnvironmentsData>(jsonString);
                        BaseUrl = envUrls.Base;
                        ApiUrl = envUrls.Api;
                        IdentityUrl = envUrls.Identity;
                        WebVaultUrl = envUrls.Vault;
                        IconsUrl = envUrls.Icons;
                        NotificationsUrls = envUrls.Notifications;
                        NotifyUrlsChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void ClearAllUrls()
        {
            BaseUrl = string.Empty;
            ApiUrl = string.Empty;
            IdentityUrl = string.Empty;
            WebVaultUrl = string.Empty;
            IconsUrl = string.Empty;
            NotificationsUrls = string.Empty;
            NotifyUrlsChanged();
        }

        private void NotifyUrlsChanged() {
            TriggerPropertyChanged(nameof(BaseUrl), new[]
                        {
                            nameof(ApiUrl),
                            nameof(IdentityUrl),
                            nameof(WebVaultUrl),
                            nameof(IconsUrl),
                            nameof(NotificationsUrls)
                        });
        }
    }

    public class EnvironmentsData
    {
        public string Base { get; set; }
        public string Admin { get; set; }
        public string Api { get; set; }
        public string Identity { get; set; }
        public string Icons { get; set; }
        public string Notifications { get; set; }
        public string Sso { get; set; }
        public string Vault { get; set; }
    }
}
