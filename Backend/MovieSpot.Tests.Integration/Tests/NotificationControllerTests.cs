using Moq;
using MovieSpot.DTO_s;
using MovieSpot.Tests.Integration.Config;
using System.Net;
using System.Net.Http.Json;

namespace MovieSpot.Tests.Integration.Tests
{
    public class NotificationControllerTests : IntegrationTestBase
    {
        private readonly CustomWebApplicationFactory _factory;

        public NotificationControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _factory = factory;
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(FakeAuthHandler.AuthenticationScheme);
        }

        #region POST /Notification/token
        [Fact]
        public async Task SendToToken_ReturnsOk_WhenValidRequest()
        {
            _factory.FcmMock.Invocations.Clear();

            var request = new SendToTokenRequest
            {
                Token = "abc123",
                Title = "Teste",
                Body = "Mensagem"
            };

            _factory.FcmMock
                .Setup(s => s.SendToTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null))
                .Returns(Task.CompletedTask);

            var response = await _client.PostAsJsonAsync("/Notification/token", request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _factory.FcmMock.Verify(s => s.SendToTokenAsync("abc123", "Teste", "Mensagem", null), Times.Once);
        }

        [Fact]
        public async Task SendToToken_ReturnsBadRequest_WhenTokenMissing()
        {
            var request = new SendToTokenRequest { Token = "", Title = "Teste", Body = "Mensagem" };

            var response = await _client.PostAsJsonAsync("/Notification/token", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendToToken_ReturnsInternalServerError_WhenServiceThrows()
        {
            var request = new SendToTokenRequest
            {
                Token = "abc123",
                Title = "Teste",
                Body = "Mensagem"
            };

            _factory.FcmMock
                .Setup(s => s.SendToTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null))
                .ThrowsAsync(new Exception("Erro simulado"));

            var response = await _client.PostAsJsonAsync("/Notification/token", request);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
        #endregion

        #region POST /Notification/topic
        [Fact]
        public async Task SendToTopic_ReturnsOk_WhenValidRequest()
        {
            var request = new SendToTopicRequest
            {
                Topic = "geral",
                Title = "Título",
                Body = "Mensagem"
            };

            _factory.FcmMock
                .Setup(s => s.SendToTopicAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null))
                .Returns(Task.CompletedTask);

            var response = await _client.PostAsJsonAsync("/Notification/topic", request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _factory.FcmMock.Verify(s => s.SendToTopicAsync("geral", "Título", "Mensagem", null), Times.Once);
        }

        [Fact]
        public async Task SendToTopic_ReturnsBadRequest_WhenTopicMissing()
        {
            var request = new SendToTopicRequest { Topic = "", Title = "Título", Body = "Mensagem" };

            var response = await _client.PostAsJsonAsync("/Notification/topic", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendToTopic_ReturnsInternalServerError_WhenServiceThrows()
        {
            var request = new SendToTopicRequest
            {
                Topic = "geral",
                Title = "Título",
                Body = "Mensagem"
            };

            _factory.FcmMock
                .Setup(s => s.SendToTopicAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null))
                .ThrowsAsync(new Exception("Erro simulado"));

            var response = await _client.PostAsJsonAsync("/Notification/topic", request);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
        #endregion
    }
}