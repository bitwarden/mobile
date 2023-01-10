﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Bit.App.Abstractions;
using Bit.App.Resources;
using Bit.Core.Abstractions;
using Bit.Core.Models.Response;
using Bit.Core.Utilities;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace Bit.App.Pages
{
    public class LoginPasswordlessRequestsListViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IStateService _stateService;
        private readonly IDeviceActionService _deviceActionService;
        private readonly IPlatformUtilsService _platformUtilsService;
        private readonly ILogger _logger;
        private ObservableRangeCollection<PasswordlessLoginResponse> _loginRequestsList;
        private bool _isRefreshing;

        public LoginPasswordlessRequestsListViewModel()
        {
            _authService = ServiceContainer.Resolve<IAuthService>();
            _stateService = ServiceContainer.Resolve<IStateService>();
            _deviceActionService = ServiceContainer.Resolve<IDeviceActionService>();
            _platformUtilsService = ServiceContainer.Resolve<IPlatformUtilsService>();
            _logger = ServiceContainer.Resolve<ILogger>();

            PageTitle = AppResources.PendingLogInRequests;
            LoginRequestsList = new ObservableRangeCollection<PasswordlessLoginResponse>();

            AnswerRequestCommand = new AsyncCommand<PasswordlessLoginResponse>(PasswordlessLoginAsync,
               onException: ex => HandleException(ex),
                allowsMultipleExecutions: false);

            DeclineAllRequestsCommand = new AsyncCommand(DeclineAllRequestsAsync,
                onException: ex => HandleException(ex),
                allowsMultipleExecutions: false);

            RefreshCommand = new AsyncCommand(RefreshAsync,
                onException: ex => HandleException(ex),
                allowsMultipleExecutions: false);
        }

        public ICommand RefreshCommand { get; }

        public AsyncCommand<PasswordlessLoginResponse> AnswerRequestCommand { get; }

        public AsyncCommand DeclineAllRequestsCommand { get; }

        public ObservableRangeCollection<PasswordlessLoginResponse> LoginRequestsList
        {
            get => _loginRequestsList;
            set => SetProperty(ref _loginRequestsList, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public async Task RefreshAsync()
        {
            try
            {
                await _deviceActionService.ShowLoadingAsync(AppResources.Loading);
                LoginRequestsList.ReplaceRange(await _authService.GetActivePasswordlessLoginRequestsAsync());
                await _deviceActionService.HideLoadingAsync();

                if (!LoginRequestsList.Any())
                {
                    Page.Navigation.PopModalAsync().FireAndForget();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private async Task PasswordlessLoginAsync(PasswordlessLoginResponse request)
        {
            if (request.IsExpired)
            {
                await _platformUtilsService.ShowDialogAsync(AppResources.LoginRequestHasAlreadyExpired);
                await Page.Navigation.PopModalAsync();
                return;
            }

            var loginRequestData = await _authService.GetPasswordlessLoginRequestByIdAsync(request.Id);
            if (loginRequestData.IsAnswered)
            {
                await _platformUtilsService.ShowDialogAsync(AppResources.ThisRequestIsNoLongerValid);
                return;
            }

            var page = new LoginPasswordlessPage(new LoginPasswordlessDetails()
            {
                PubKey = loginRequestData.PublicKey,
                Id = loginRequestData.Id,
                IpAddress = loginRequestData.RequestIpAddress,
                Email = await _stateService.GetEmailAsync(),
                FingerprintPhrase = loginRequestData.RequestFingerprint,
                RequestDate = loginRequestData.CreationDate,
                DeviceType = loginRequestData.RequestDeviceType,
                Origin = loginRequestData.Origin
            });

            await Device.InvokeOnMainThreadAsync(() => Application.Current.MainPage.Navigation.PushModalAsync(new NavigationPage(page)));
        }

        private async Task DeclineAllRequestsAsync()
        {
            try
            {
                if (!await _platformUtilsService.ShowDialogAsync(AppResources.AreYouSureYouWantToDeclineAllPendingLogInRequests, null, AppResources.Yes, AppResources.No))
                {
                    return;
                }

                await _deviceActionService.ShowLoadingAsync(AppResources.Loading);
                var taskList = new List<Task>();
                foreach (var request in LoginRequestsList)
                {
                    taskList.Add(_authService.PasswordlessLoginAsync(request.Id, request.PublicKey, false));
                }
                await Task.WhenAll(taskList);
                await _deviceActionService.HideLoadingAsync();
                RefreshCommand.Execute(null);
                _platformUtilsService.ShowToast("info", null, AppResources.RequestsDeclined);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
    }
}

