using Microsoft.AspNetCore.Mvc;
using MovieSpot.DTO_s;
using MovieSpot.Services.Notifications;

namespace MovieSpot.Controllers
{
    /// <summary>
    /// Controller responsible for exposing endpoints that send push notifications
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IFcmNotificationService _fcmService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/>.
        /// </summary>
        /// <param name="fcmService">Service responsible for sending notifications through FCM.</param>
        public NotificationController(IFcmNotificationService fcmService)
        {
            _fcmService = fcmService;
        }

        /// <summary>
        /// Sends a push notification to a specific device.
        /// </summary>
        /// <param name="request">The request data containing the device token, title, and body.</param>
        /// <returns>HTTP response indicating the result of the notification delivery.</returns>
        /// <response code="200">Notification successfully sent to the device.</response>
        /// <response code="400">Invalid request (missing or incorrect data).</response>
        /// <response code="500">Internal server error while attempting to send the notification.</response>
        [HttpPost("token")]
        public async Task<IActionResult> SendToToken([FromBody] SendToTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return BadRequest("Device token is required.");

            try
            {
                await _fcmService.SendToTokenAsync(request.Token, request.Title, request.Body);
                return Ok(new { message = "Notification successfully sent to the device." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error while sending notification.", error = ex.Message });
            }
        }

        /// <summary>
        /// Sends a push notification to all devices subscribed to a specific topic.
        /// </summary>
        /// <param name="request">The request data containing the topic name, title, and body.</param>
        /// <returns>HTTP response indicating the result of the notification delivery.</returns>
        /// <response code="200">Notification successfully sent to the topic.</response>
        /// <response code="400">Invalid request (missing or incorrect data).</response>
        /// <response code="500">Internal server error while attempting to send the notification.</response>
        [HttpPost("topic")]
        public async Task<IActionResult> SendToTopic([FromBody] SendToTopicRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Topic))
                return BadRequest("Topic name is required.");

            try
            {
                await _fcmService.SendToTopicAsync(request.Topic, request.Title, request.Body);
                return Ok(new { message = "Notification successfully sent to the topic." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error while sending notification.", error = ex.Message });
            }
        }
    }
}
