using Microsoft.Extensions.DependencyInjection;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Tests.Integration.Config;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using static MovieSpot.DTO_s.VoucherDTO;

namespace MovieSpot.Tests.Integration.Tests
{
    public class VoucherControllerTests : IntegrationTestBase
    {
        private readonly TestDataFactory _dataFactory;

        public VoucherControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _dataFactory = new TestDataFactory(_client, factory.Services);
            _dataFactory.ClearDatabaseAsync().Wait();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(FakeAuthHandler.AuthenticationScheme);
        }

        #region POST /Voucher
        [Fact]
        public async Task Create_ReturnsOk_WhenStaffAuthorized()
        {
            var response = await _client.PostAsync("/Voucher", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var voucher = await response.Content.ReadFromJsonAsync<VoucherResponseDto>();
            Assert.NotNull(voucher);
            Assert.NotEmpty(voucher.Code);
            Assert.InRange(voucher.Value, 0.01m, 0.99m);
        }
        #endregion

        #region GET /Voucher/{id}
        [Fact]
        public async Task GetById_ReturnsOk_WhenVoucherExists()
        {
            var createResponse = await _client.PostAsync("/Voucher", null);
            var created = await createResponse.Content.ReadFromJsonAsync<VoucherResponseDto>();

            var response = await _client.GetAsync($"/Voucher/{created.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var fetched = await response.Content.ReadFromJsonAsync<VoucherResponseDto>();
            Assert.Equal(created.Id, fetched.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenVoucherDoesNotExist()
        {
            var response = await _client.GetAsync("/Voucher/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region PUT /Voucher/{id}
        [Fact]
        public async Task Update_ReturnsOk_WhenValidData()
        {
            var createResponse = await _client.PostAsync("/Voucher", null);
            var created = await createResponse.Content.ReadFromJsonAsync<VoucherResponseDto>();

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MovieSpot.Data.ApplicationDbContext>();
                var dbVoucher = db.Voucher.Find(created.Id);
                dbVoucher.MaxUsages = 10;
                db.SaveChanges();
            }

            var updateDto = new VoucherUpdateDto
            {
                Id = created.Id,
                Code = created.Code,
                Value = 0.5m,
                ValidUntil = DateTime.UtcNow.AddMonths(2),
                MaxUsages = 10,
                Usages = 0
            };

            var response = await _client.PutAsJsonAsync($"/Voucher/{created.Id}", updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<VoucherResponseDto>();
            Assert.Equal(0.5m, updated.Value);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenInvalidValue()
        {
            var createResponse = await _client.PostAsync("/Voucher", null);
            var created = await createResponse.Content.ReadFromJsonAsync<VoucherResponseDto>();

            var updateDto = new VoucherUpdateDto
            {
                Id = created.Id,
                Code = created.Code,
                Value = 1.5m,
                ValidUntil = DateTime.UtcNow.AddMonths(2),
                MaxUsages = 5,
                Usages = 0
            };

            var response = await _client.PutAsJsonAsync($"/Voucher/{created.Id}", updateDto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenVoucherDoesNotExist()
        {
            var updateDto = new VoucherUpdateDto
            {
                Id = 9999,
                Code = "FAKECODE",
                Value = 0.5m,
                ValidUntil = DateTime.UtcNow.AddMonths(1),
                MaxUsages = 10,
                Usages = 0
            };

            var response = await _client.PutAsJsonAsync("/Voucher/9999", updateDto);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion
    }
}
