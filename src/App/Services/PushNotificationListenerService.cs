﻿#if !FDROID
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bit.App.Abstractions;
using Bit.App.Pages;
using Bit.App.Resources;
using Bit.Core;
using Bit.Core.Abstractions;
using Bit.Core.Enums;
using Bit.Core.Exceptions;
using Bit.Core.Models.Response;
using Bit.Core.Services;
using Bit.Core.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace Bit.App.Services
{
    public class PushNotificationListenerService : IPushNotificationListenerService
    {
        const string TAG = "##PUSH NOTIFICATIONS";

        private bool _showNotification;
        private bool _resolved;
        private ISyncService _syncService;
        private IStateService _stateService;
        private IAppIdService _appIdService;
        private IApiService _apiService;
        private IMessagingService _messagingService;
        private IPushNotificationService _pushNotificationService;

        public async Task OnMessageAsync(JObject value, string deviceType)
        {
            Debug.WriteLine($"{TAG} OnMessageAsync called");

            Resolve();
            if (value == null)
            {
                return;
            }

            _showNotification = false;
            Debug.WriteLine($"{TAG} Message Arrived: {JsonConvert.SerializeObject(value)}");

            NotificationResponse notification = null;
            if (deviceType == Device.Android)
            {
                notification = value.ToObject<NotificationResponse>();
            }
            else
            {
                if (!value.TryGetValue("data", StringComparison.OrdinalIgnoreCase, out JToken dataToken) ||
                    dataToken == null)
                {
                    return;
                }
                notification = dataToken.ToObject<NotificationResponse>();
            }

            Debug.WriteLine($"{TAG} - Notification object created: t:{notification?.Type} - p:{notification?.Payload}");

            var appId = await _appIdService.GetAppIdAsync();
            if (notification?.Payload == null || notification.ContextId == appId)
            {
                return;
            }

            var myUserId = await _stateService.GetActiveUserIdAsync();
            var isAuthenticated = await _stateService.IsAuthenticatedAsync();
            switch (notification.Type)
            {
                case NotificationType.SyncCipherUpdate:
                case NotificationType.SyncCipherCreate:
                    var cipherCreateUpdateMessage = JsonConvert.DeserializeObject<SyncCipherNotification>(
                        notification.Payload);
                    if (isAuthenticated && cipherCreateUpdateMessage.UserId == myUserId)
                    {
                        await _syncService.SyncUpsertCipherAsync(cipherCreateUpdateMessage,
                            notification.Type == NotificationType.SyncCipherUpdate);
                    }
                    break;
                case NotificationType.SyncFolderUpdate:
                case NotificationType.SyncFolderCreate:
                    var folderCreateUpdateMessage = JsonConvert.DeserializeObject<SyncFolderNotification>(
                        notification.Payload);
                    if (isAuthenticated && folderCreateUpdateMessage.UserId == myUserId)
                    {
                        await _syncService.SyncUpsertFolderAsync(folderCreateUpdateMessage,
                            notification.Type == NotificationType.SyncFolderUpdate);
                    }
                    break;
                case NotificationType.SyncLoginDelete:
                case NotificationType.SyncCipherDelete:
                    var loginDeleteMessage = JsonConvert.DeserializeObject<SyncCipherNotification>(
                        notification.Payload);
                    if (isAuthenticated && loginDeleteMessage.UserId == myUserId)
                    {
                        await _syncService.SyncDeleteCipherAsync(loginDeleteMessage);
                    }
                    break;
                case NotificationType.SyncFolderDelete:
                    var folderDeleteMessage = JsonConvert.DeserializeObject<SyncFolderNotification>(
                        notification.Payload);
                    if (isAuthenticated && folderDeleteMessage.UserId == myUserId)
                    {
                        await _syncService.SyncDeleteFolderAsync(folderDeleteMessage);
                    }
                    break;
                case NotificationType.SyncCiphers:
                case NotificationType.SyncVault:
                case NotificationType.SyncSettings:
                    if (isAuthenticated)
                    {
                        await _syncService.FullSyncAsync(false);
                    }
                    break;
                case NotificationType.SyncOrgKeys:
                    if (isAuthenticated)
                    {
                        await _apiService.RefreshIdentityTokenAsync();
                        await _syncService.FullSyncAsync(true);
                    }
                    break;
                case NotificationType.LogOut:
                    if (isAuthenticated)
                    {
                        _messagingService.Send("logout");
                    }
                    break;
                case NotificationType.AuthRequest:
                    var passwordlessLoginMessage = JsonConvert.DeserializeObject<PasswordlessRequestNotification>(notification.Payload);

                    // if the user has not enabled passwordless logins ignore requests
                    if (!await _stateService.GetApprovePasswordlessLoginsAsync(passwordlessLoginMessage?.UserId))
                    {
                        return;
                    }

                    // if there is a request modal opened ignore all incoming requests
                    if (App.Current.MainPage.Navigation.ModalStack.Any(p => p is NavigationPage navPage && navPage.CurrentPage is LoginPasswordlessPage))
                    {
                        return;
                    }

                    await _stateService.SetPasswordlessLoginNotificationAsync(passwordlessLoginMessage, passwordlessLoginMessage?.UserId);
                    var userEmail = await _stateService.GetEmailAsync(passwordlessLoginMessage?.UserId);
                    var notificationData = new Dictionary<string, string>();
                    notificationData.Add("userEmail", userEmail);
                    notificationData.Add("notificationId", passwordlessLoginMessage.Id);

                    _pushNotificationService.SendLocalNotification(AppResources.LogInRequested, String.Format(AppResources.ConfimLogInAttempForX, userEmail), Constants.PasswordlessNotificationId, Constants.PasswordlessNotificationType, notificationData, Constants.PasswordlessNotificationTimeoutInMinutes);
                    _messagingService.Send("passwordlessLoginRequest", passwordlessLoginMessage);
                    break;
                default:
                    break;
            }
        }

        public async Task OnRegisteredAsync(string token, string deviceType)
        {
            Resolve();
            Debug.WriteLine($"{TAG} - Device Registered - Token : {token}");
            var isAuthenticated = await _stateService.IsAuthenticatedAsync();
            if (!isAuthenticated)
            {
                Debug.WriteLine($"{TAG} - not auth");
                return;
            }

            var appId = await _appIdService.GetAppIdAsync();
            try
            {
#if DEBUG
                await _stateService.SetPushInstallationRegistrationErrorAsync(null);
#endif

                await _apiService.PutDeviceTokenAsync(appId,
                    new Core.Models.Request.DeviceTokenRequest { PushToken = token });

                Debug.WriteLine($"{TAG} Registered device with server.");

                await _stateService.SetPushLastRegistrationDateAsync(DateTime.UtcNow);
                if (deviceType == Device.Android)
                {
                    await _stateService.SetPushCurrentTokenAsync(token);
                }
            }
#if DEBUG
            catch (ApiException apiEx)
            {
                Debug.WriteLine($"{TAG} Failed to register device.");

                await _stateService.SetPushInstallationRegistrationErrorAsync(apiEx.Error?.Message);
            }
            catch (Exception e)
            {
                await _stateService.SetPushInstallationRegistrationErrorAsync(e.Message);
                throw;
            }
#else
            catch (ApiException)
            {
            }
#endif
        }

        public void OnUnregistered(string deviceType)
        {
            Debug.WriteLine($"{TAG} - Device Unnregistered");
        }

        public void OnError(string message, string deviceType)
        {
            Debug.WriteLine($"{TAG} error - {message}");
        }

        public async Task OnNotificationTapped(string type, string dataJson)
        {
            try
            {
                if (type == Constants.PasswordlessNotificationType)
                {
                    var notificationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(dataJson);
                    var userEmail = notificationData["userEmail"];
                    var notificationId = notificationData["notificationId"];
                    var notificationUserId = await _stateService.GetUserIdAsync(userEmail);
                    if (notificationUserId != null)
                    {
                        await _stateService.SetActiveUserAsync(notificationUserId);
                        _messagingService.Send(AccountsManagerMessageCommands.SWITCHED_ACCOUNT);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogEvenIfCantBeResolved(ex);
            }
        }

        public bool ShouldShowNotification()
        {
            return _showNotification;
        }

        private void Resolve()
        {
            if (_resolved)
            {
                return;
            }
            _syncService = ServiceContainer.Resolve<ISyncService>("syncService");
            _stateService = ServiceContainer.Resolve<IStateService>("stateService");
            _appIdService = ServiceContainer.Resolve<IAppIdService>("appIdService");
            _apiService = ServiceContainer.Resolve<IApiService>("apiService");
            _messagingService = ServiceContainer.Resolve<IMessagingService>("messagingService");
            _pushNotificationService = ServiceContainer.Resolve<IPushNotificationService>();
            _resolved = true;
        }
    }
}
#endif
