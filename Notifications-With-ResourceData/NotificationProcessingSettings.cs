using System;

namespace DemoApp
{
    /// <summary>
    /// This class stores your private settings related to notification handling.
    /// Make sure not to include this in your Git repo!
    /// 
    /// Use the auto-create NotificationProcessingSettingsLocal.cs to implement a static constructor that initializes the fields defined here. e.g.
    /// static NotificationProcessingSettings()
    /// {
    ///     notificationUrl = <your url>
    ///     etc.
    /// }
    /// </summary>
    static partial class NotificationProcessingSettings
    {
        public static readonly string notificationUrl = null;             // HTTPS endpoint where resource change notifications should be sent
        public static readonly string lifecycleNotificationUrl = null;    // HTTPS endpoint where subscription lifecycle notifications should be sent
        public static readonly string publicEncryptionKey = null;
        public static readonly string publicEncryptionKeyId = null;

        private static bool ValidateHttpsUrl(string url)
        {
            return !String.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
