using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Ryujinx.Ava.Common.Locale;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Ryujinx.Ava.UI.Helpers
{
    public static class NotificationHelper
    {
        private const int MaxNotifications      = 4; 
        private const int NotificationDelayInMs = 5000;

        private static WindowNotificationManager _notificationManager;

        private static readonly ManualResetEvent                 _templateAppliedEvent = new(false);
        private static readonly BlockingCollection<Notification> _notifications        = new();

        public static void SetNotificationManager(Window host)
        {
            _notificationManager = new WindowNotificationManager(host)
            {
                Position = NotificationPosition.BottomRight,
                MaxItems = MaxNotifications,
                Margin   = new Thickness(0, 0, 15, 40)
            };

            _notificationManager.TemplateApplied += (sender, args) =>
            {
                _templateAppliedEvent.Set();
            };

            Task.Run(async () =>
            {
                _templateAppliedEvent.WaitOne();

                foreach (var notification in _notifications.GetConsumingEnumerable())
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _notificationManager.Show(notification);
                    });

                    await Task.Delay(NotificationDelayInMs / MaxNotifications);
                }
            });
        }

        public static void Show(string title, string text, NotificationType type, bool waitingExit = false, Action onClick = null, Action onClose = null)
        {
            var delay = waitingExit ? TimeSpan.FromMilliseconds(0) : TimeSpan.FromMilliseconds(NotificationDelayInMs);

            _notifications.Add(new Notification(title, text, type, delay, onClick, onClose));
        }

        public static void ShowError(string message)
        {
            Show(LocaleManager.Instance[LocaleKeys.DialogErrorTitle], $"{LocaleManager.Instance[LocaleKeys.DialogErrorMessage]}\n\n{message}", NotificationType.Error);
        }
    }
}