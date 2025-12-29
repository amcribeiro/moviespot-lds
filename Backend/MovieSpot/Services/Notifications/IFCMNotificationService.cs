using System.Threading.Tasks;

namespace MovieSpot.Services.Notifications
{
    /// <summary>
    /// Defines the contract for sending push notifications through Firebase Cloud Messaging (FCM).
    /// </summary>
    public interface IFcmNotificationService
    {
        /// <summary>
        /// Sends a push notification to a specific device identified by its Firebase token.
        /// </summary>
        /// <param name="deviceToken">The unique device token provided by Firebase on the client app.</param>
        /// <param name="title">The title of the notification.</param>
        /// <param name="body">The body text of the notification.</param>
        /// <param name="data">
        /// Optional additional data (custom payload) sent along with the notification,
        /// useful for opening specific screens or carrying extra information.
        /// </param>
        /// <returns>An asynchronous task representing the send operation.</returns>
        Task SendToTokenAsync(string deviceToken, string title, string body, object? data = null);

        /// <summary>
        /// Sends a push notification to all devices subscribed to a specific Firebase topic.
        /// </summary>
        /// <param name="topic">The name of the topic (e.g., "bookings", "staff", "promotions").</param>
        /// <param name="title">The title of the notification.</param>
        /// <param name="body">The body text of the notification.</param>
        /// <param name="data">
        /// Optional additional data (custom payload) sent along with the notification.
        /// </param>
        /// <returns>An asynchronous task representing the send operation.</returns>
        Task SendToTopicAsync(string topic, string title, string body, object? data = null);
    }
}
