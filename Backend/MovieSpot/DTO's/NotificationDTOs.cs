namespace MovieSpot.DTO_s
{
    /// <summary>
    /// Represents the HTTP request body used to send a notification
    /// to a specific device (via FCM token).
    /// </summary>
    /// <example>
    /// Example of JSON payload sent from the client:
    /// {
    ///   "token": "fcm_token_abc123",
    ///   "title": "Booking confirmed",
    ///   "body": "Your booking has been successfully confirmed!"
    /// }
    /// </example>
    public record SendToTokenRequest
    {
        public string Token { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
    }

    /// <summary>
    /// Represents the HTTP request body used to send a notification
    /// to all devices subscribed to a specific topic.
    /// </summary>
    /// <example>
    /// Example of JSON payload sent from the client:
    /// {
    ///   "topic": "staff",
    ///   "title": "New booking created",
    ///   "body": "Court 3 has been booked for tomorrow at 7 PM."
    /// }
    /// </example>
    public record SendToTopicRequest
    {
        public string Topic { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
    }
}