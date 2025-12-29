using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Tests.Integration.Config;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MovieSpot.Tests.Integration.Tests
{
    public class PaymentControllerTests : IntegrationTestBase
    {
        private readonly TestDataFactory _dataFactory;

        public PaymentControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _dataFactory = new TestDataFactory(_client, factory.Services);
            _dataFactory.ClearDatabaseAsync().Wait();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(FakeAuthHandler.AuthenticationScheme);
        }

        #region GET /Payment
        [Fact]
        public async Task GetAll_ReturnsNotFound_WhenNoPaymentsExist()
        {
            var response = await _client.GetAsync("/Payment");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WhenPaymentsExist()
        {
            await _dataFactory.CreateTestPaymentAsync();

            var response = await _client.GetAsync("/Payment");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payments = await response.Content.ReadFromJsonAsync<IEnumerable<PaymentResponseDto>>();
            Assert.NotEmpty(payments);
        }
        #endregion

        #region POST /Payment/checkout
        [Fact]
        public async Task Checkout_ReturnsBadRequest_WhenBookingInvalid()
        {
            var dto = new CreatePaymentRequestDto { BookingId = 0 };

            var response = await _client.PostAsJsonAsync("/Payment/checkout", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Checkout_ReturnsNotFound_WhenBookingDoesNotExist()
        {
            var dto = new CreatePaymentRequestDto { BookingId = 9999 };

            var response = await _client.PostAsJsonAsync("/Payment/checkout", dto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region GET /Payment/check-payment-status
        [Fact]
        public async Task CheckPaymentStatus_ReturnsBadRequest_WhenSessionIdEmpty()
        {
            var response = await _client.GetAsync("/Payment/check-payment-status?sessionId=");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /*[Fact]
        public async Task CheckPaymentStatus_ReturnsBadGateway_WhenStripeFails()
        {
            var response = await _client.GetAsync("/Payment/check-payment-status?sessionId=session_xx");
            Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        }*/
        #endregion
    }
}
