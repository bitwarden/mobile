﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Bit.App.Abstractions;

namespace Bit.App.Services
{
    public class NoopPushNotificationService : IPushNotificationService
    {
        public bool IsRegisteredForPush => false;

        public Task<bool> AreNotificationsSettingsEnabledAsync()
        {
            return Task.FromResult(false);
        }

        public Task<string> GetTokenAsync()
        {
            return Task.FromResult(null as string);
        }

        public Task RegisterAsync()
        {
            return Task.FromResult(0);
        }

        public Task UnregisterAsync()
        {
            return Task.FromResult(0);
        }

        public void DismissLocalNotification(string notificationId) { }

        public void SendLocalNotification(string title, string message, string notificationId, string notificationType, Dictionary<string, string> notificationData = null, int notificationTimeoutMinutes = 0) { }
    }
}
