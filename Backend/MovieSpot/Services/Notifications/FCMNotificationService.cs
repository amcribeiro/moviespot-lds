using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.FirebaseCloudMessaging.v1;
using Google.Apis.FirebaseCloudMessaging.v1.Data;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieSpot.Services.Notifications
{
    public class FcmNotificationService : IFcmNotificationService
    {
        private readonly FirebaseCloudMessagingService _fcmService;
        private readonly string _projectId;

        public FcmNotificationService(IOptions<FcmOptions> options)
        {
            var opt = options.Value;
            _projectId = opt.ProjectId;

            var credential = GoogleCredential
                .FromFile(opt.CredentialsPath)
                .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

            _fcmService = new FirebaseCloudMessagingService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential
                });
        }

        public async Task SendToTokenAsync(string deviceToken, string title, string body, object? data = null)
        {
            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification { Title = title, Body = body },
                Data = data.ToDictionaryOrEmpty()
            };

            await SendAsync(message);
        }

        public async Task SendToTopicAsync(string topic, string title, string body, object? data = null)
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification { Title = title, Body = body },
                Data = data.ToDictionaryOrEmpty()
            };

            await SendAsync(message);
        }

        private async Task SendAsync(Message message)
        {
            var request = new SendMessageRequest { Message = message };

            await _fcmService.Projects.Messages
                .Send(request, $"projects/{_projectId}")
                .ExecuteAsync();
        }
    }
}
