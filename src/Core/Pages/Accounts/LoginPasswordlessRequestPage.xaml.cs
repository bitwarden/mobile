﻿using Bit.App.Models;
using Bit.App.Utilities;
using Bit.Core.Enums;

namespace Bit.App.Pages
{
    public partial class LoginPasswordlessRequestPage : BaseContentPage
    {
        private LoginPasswordlessRequestViewModel _vm;
        private readonly AppOptions _appOptions;

        public LoginPasswordlessRequestPage(string email, AuthRequestType authRequestType, AppOptions appOptions = null, bool authingWithSso = false)
        {
            InitializeComponent();
            _appOptions = appOptions;
            _vm = BindingContext as LoginPasswordlessRequestViewModel;
            _vm.Page = this;
            _vm.Email = email;
            _vm.AuthRequestType = authRequestType;
            _vm.AuthingWithSso = authingWithSso;
            _vm.StartTwoFactorAction = () => MainThread.BeginInvokeOnMainThread(async () => await StartTwoFactorAsync());
            _vm.LogInSuccessAction = () => MainThread.BeginInvokeOnMainThread(async () => await LogInSuccessAsync());
            _vm.UpdateTempPasswordAction = () => MainThread.BeginInvokeOnMainThread(async () => await UpdateTempPasswordAsync());
            _vm.CloseAction = () => { Navigation.PopModalAsync(); };

            _vm.CreatePasswordlessLoginCommand.Execute(null);
        }

        protected override bool ShouldCheckToPreventOnNavigatedToCalledTwice => true;

        protected override Task InitOnNavigatedToAsync()
        {
            _vm.StartCheckLoginRequestStatus();
            return Task.CompletedTask;
        }

        protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
        {
            base.OnNavigatedFrom(args);
            _vm.StopCheckLoginRequestStatus();
        }

        private async Task StartTwoFactorAsync()
        {
            var page = new TwoFactorPage(false, _appOptions);
            await Navigation.PushModalAsync(new NavigationPage(page));
        }

        private async Task LogInSuccessAsync()
        {
            if (AppHelpers.SetAlternateMainPage(_appOptions))
            {
                return;
            }
            var previousPage = await AppHelpers.ClearPreviousPage();
            App.MainPage = new TabsPage(_appOptions, previousPage);
        }

        private async Task UpdateTempPasswordAsync()
        {
            var page = new UpdateTempPasswordPage();
            await Navigation.PushModalAsync(new NavigationPage(page));
        }
    }
}
