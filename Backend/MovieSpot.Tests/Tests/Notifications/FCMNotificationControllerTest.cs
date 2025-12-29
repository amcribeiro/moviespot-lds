using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MovieSpot.Controllers;
using MovieSpot.DTO_s;
using MovieSpot.Services.Notifications;

namespace MovieSpot.Tests.Controllers.Notifications
{
    public class NotificationsControllerTests
    {
        [Fact]
        public async Task SendToToken_ReturnsOk_AndCallsService()
        {

            var mockService = new Mock<IFcmNotificationService>();
            var controller = new NotificationController(mockService.Object);

            var dto = new SendToTokenRequest { Token = "token123", Title = "Título", Body = "Mensagem" };

            var result = await controller.SendToToken(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            mockService.Verify(s => s.SendToTokenAsync("token123", "Título", "Mensagem", null), Times.Once);
        }

        [Fact]
        public async Task SendToTopic_ReturnsOk_AndCallsService()
        {
            var mockService = new Mock<IFcmNotificationService>();
            var controller = new NotificationController(mockService.Object);

            var dto = new SendToTopicRequest { Topic = "reservas", Title = "Título", Body = "Mensagem" };

            var result = await controller.SendToTopic(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            mockService.Verify(s => s.SendToTopicAsync("reservas", "Título", "Mensagem", null), Times.Once);
        }

        [Fact]
        public async Task SendToToken_WhenTokenIsMissing_ReturnsBadRequest()
        {
            var mockService = new Mock<IFcmNotificationService>();
            var controller = new NotificationController(mockService.Object);

            var dto = new SendToTokenRequest { Token = "", Title = "Título", Body = "Mensagem" };

            var result = await controller.SendToToken(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
