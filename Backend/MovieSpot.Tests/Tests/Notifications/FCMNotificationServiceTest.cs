using Microsoft.Extensions.Options;
using MovieSpot.Services.Notifications;
using System.Threading.Tasks;
using Xunit;

namespace MovieSpot.Tests.Services.Notifications
{
    public class FcmNotificationServiceTest
    {
        private static FcmNotificationService CreateService()
        {
            var options = Options.Create(new FcmOptions
            {
                // Caminho fake => credencial inválida nos testes
                CredentialsPath = "fake_credentials.json",
                ProjectId = "fake-project"
            });

            return new FcmNotificationService(options);
        }

        /*[Fact]
        public async Task SendToTokenAsync_WithInvalidCredentials_ThrowsException()
        {
            // Arrange
            var service = CreateService();

            // Act / Assert
            await Assert.ThrowsAnyAsync<System.Exception>(() =>
                service.SendToTokenAsync(
                    "fake-token",
                    "Titulo",
                    "Mensagem"
                ));
        }

        [Fact]
        public async Task SendToTopicAsync_WithInvalidCredentials_ThrowsException()
        {
            // Arrange
            var service = CreateService();

            // Act / Assert
            await Assert.ThrowsAnyAsync<System.Exception>(() =>
                service.SendToTopicAsync(
                    "fake-topic",
                    "Titulo",
                    "Mensagem"
                ));
        }*/
    }
}
