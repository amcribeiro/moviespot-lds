using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieSpot.Controllers;
using MovieSpot.Models;
using MovieSpot.Services.Vouchers;
using static MovieSpot.DTO_s.VoucherDTO;
using Xunit;

namespace MovieSpot.Tests.Controllers.Vouchers
{
    public class VoucherControllerTest
    {
        private readonly Mock<IVoucherService> _serviceMock;
        private readonly VoucherController _controller;

        public VoucherControllerTest()
        {
            _serviceMock = new Mock<IVoucherService>();
            _controller = new VoucherController(_serviceMock.Object);
        }

        #region Create

        [Fact]
        public void Create_Should_Return_200_With_VoucherResponseDto()
        {
            var v = new Voucher
            {
                Id = 1,
                Code = "ABCD1234",
                Value = 0.2m,
                ValidUntil = DateTime.UtcNow.AddDays(30),
                MaxUsages = 5,
                Usages = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _serviceMock.Setup(s => s.CreateVoucher()).Returns(v);

            var result = _controller.Create() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);

            var returned = Assert.IsType<VoucherResponseDto>(result.Value);
            Assert.Equal("ABCD1234", returned.Code);
            Assert.Equal(0.2m, returned.Value);
        }

        [Fact]
        public void Create_Should_Return_400_When_InvalidOperationException()
        {
            _serviceMock.Setup(s => s.CreateVoucher())
                .Throws(new InvalidOperationException("duplicate"));

            var result = _controller.Create() as BadRequestObjectResult;

            Assert.Equal(400, result!.StatusCode);
            Assert.Contains("duplicate", result.Value!.ToString());
        }

        [Fact]
        public void Create_Should_Return_400_When_ArgumentOutOfRangeException()
        {
            _serviceMock.Setup(s => s.CreateVoucher())
                .Throws(new ArgumentOutOfRangeException("Value"));

            var result = _controller.Create() as BadRequestObjectResult;

            Assert.Equal(400, result!.StatusCode);
            Assert.Contains("Value", result.Value!.ToString());
        }

        [Fact]
        public void Create_Should_Return_400_When_DbUpdateException()
        {
            _serviceMock.Setup(s => s.CreateVoucher())
                .Throws(new DbUpdateException("db fail"));

            var result = _controller.Create() as BadRequestObjectResult;

            Assert.Equal(400, result!.StatusCode);
            Assert.Contains("db fail", result.Value!.ToString());
        }

        [Fact]
        public void Create_Should_Return_400_When_GenericException()
        {
            _serviceMock.Setup(s => s.CreateVoucher())
                .Throws(new Exception("boom"));

            var result = _controller.Create() as BadRequestObjectResult;

            Assert.Equal(400, result!.StatusCode);
            Assert.Contains("Unexpected error", result!.Value!.ToString());
        }

        #endregion

        #region GetById

        [Fact]
        public void GetById_Should_Return_200_With_VoucherResponseDto()
        {
            var v = new Voucher
            {
                Id = 5,
                Code = "OKCODE",
                Value = 0.3m,
                ValidUntil = DateTime.UtcNow.AddDays(10),
                MaxUsages = 10,
                Usages = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _serviceMock.Setup(s => s.GetVoucherById(5)).Returns(v);

            var result = _controller.GetById(5) as OkObjectResult;

            Assert.Equal(200, result!.StatusCode);
            var returned = Assert.IsType<VoucherResponseDto>(result.Value);
            Assert.Equal("OKCODE", returned.Code);
        }

        [Fact]
        public void GetById_Should_Return_400_When_ArgumentOutOfRangeException()
        {
            _serviceMock.Setup(s => s.GetVoucherById(It.IsAny<int>()))
                .Throws(new ArgumentOutOfRangeException("id"));

            var result = _controller.GetById(0) as BadRequestObjectResult;

            Assert.Equal(400, result!.StatusCode);
            Assert.Contains("id", result.Value!.ToString());
        }

        [Fact]
        public void GetById_Should_Return_404_When_KeyNotFoundException()
        {
            _serviceMock.Setup(s => s.GetVoucherById(It.IsAny<int>()))
                .Throws(new KeyNotFoundException("not found"));

            var result = _controller.GetById(99) as NotFoundObjectResult;

            Assert.Equal(404, result!.StatusCode);
            Assert.Contains("not found", result.Value!.ToString());
        }

        #endregion

        #region Update

        [Fact]
        public void Update_Should_Return_400_When_Dto_Null()
        {
            var result = _controller.Update(1, null!) as BadRequestObjectResult;

            Assert.Equal(400, result!.StatusCode);
            Assert.Contains("Voucher cannot be null.", result.Value.ToString());
        }

        [Fact]
        public void Update_Should_Return_200_When_Success()
        {
            var dto = new VoucherUpdateDto
            {
                Code = "CODEOK",
                Value = 0.5m,
                ValidUntil = DateTime.UtcNow.AddMonths(1),
                MaxUsages = 10,
                Usages = 0
            };

            _serviceMock.Setup(s => s.UpdateVoucher(It.IsAny<Voucher>()));

            var result = _controller.Update(1, dto) as OkObjectResult;

            Assert.Equal(200, result!.StatusCode);
            var returned = Assert.IsType<VoucherResponseDto>(result.Value);
            Assert.Equal("CODEOK", returned.Code);
        }

        [Fact]
        public void Update_Should_Return_400_When_ArgumentNullException()
        {
            _serviceMock.Setup(s => s.UpdateVoucher(It.IsAny<Voucher>()))
                .Throws(new ArgumentNullException("voucher"));

            var dto = new VoucherUpdateDto
            {
                Code = "C",
                Value = 0.5m,
                ValidUntil = DateTime.UtcNow.AddMonths(1),
                MaxUsages = 10
            };

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.Equal(400, result!.StatusCode);
            Assert.Contains("voucher", result.Value!.ToString());
        }

        [Fact]
        public void Update_Should_Return_400_When_ArgumentOutOfRangeException()
        {
            _serviceMock.Setup(s => s.UpdateVoucher(It.IsAny<Voucher>()))
                .Throws(new ArgumentOutOfRangeException("id"));

            var dto = new VoucherUpdateDto
            {
                Code = "X",
                Value = 0.4m,
                ValidUntil = DateTime.UtcNow.AddMonths(1),
                MaxUsages = 10
            };

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.Equal(400, result!.StatusCode);
            Assert.Contains("id", result.Value!.ToString());
        }

        [Fact]
        public void Update_Should_Return_404_When_KeyNotFoundException()
        {
            _serviceMock.Setup(s => s.UpdateVoucher(It.IsAny<Voucher>()))
                .Throws(new KeyNotFoundException("not found"));

            var dto = new VoucherUpdateDto
            {
                Code = "Z",
                Value = 0.4m,
                ValidUntil = DateTime.UtcNow.AddMonths(1),
                MaxUsages = 10
            };

            var result = _controller.Update(99, dto) as NotFoundObjectResult;

            Assert.Equal(404, result!.StatusCode);
            Assert.Contains("not found", result.Value!.ToString());
        }

        [Fact]
        public void Update_Should_Return_400_When_InvalidOperationException()
        {
            _serviceMock.Setup(s => s.UpdateVoucher(It.IsAny<Voucher>()))
                .Throws(new InvalidOperationException("invalid"));

            var dto = new VoucherUpdateDto
            {
                Code = "A",
                Value = 0.3m,
                ValidUntil = DateTime.UtcNow.AddMonths(1),
                MaxUsages = 10
            };

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.Equal(400, result!.StatusCode);
            Assert.Contains("invalid", result.Value!.ToString());
        }

        [Fact]
        public void Update_Should_Return_400_When_DbUpdateException()
        {
            _serviceMock.Setup(s => s.UpdateVoucher(It.IsAny<Voucher>()))
                .Throws(new DbUpdateException("db problem"));

            var dto = new VoucherUpdateDto
            {
                Code = "B",
                Value = 0.2m,
                ValidUntil = DateTime.UtcNow.AddMonths(1),
                MaxUsages = 10
            };

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.Equal(400, result!.StatusCode);
            Assert.Contains("db problem", result.Value!.ToString());
        }

        [Fact]
        public void Update_Should_Return_400_When_GenericException()
        {
            _serviceMock.Setup(s => s.UpdateVoucher(It.IsAny<Voucher>()))
                .Throws(new Exception("Boom"));

            var dto = new VoucherUpdateDto
            {
                Code = "G",
                Value = 0.25m,
                ValidUntil = DateTime.UtcNow.AddMonths(1),
                MaxUsages = 10
            };

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.Equal(400, result!.StatusCode);
            Assert.Contains("Unexpected error", result.Value.ToString());
        }

        [Fact]
        public void Update_Should_Return_400_When_ModelState_Invalid()
        {
            var dto = new VoucherUpdateDto
            {
                Value = 0.3m,
                ValidUntil = DateTime.UtcNow.AddMonths(1),
                MaxUsages = 5,
                Usages = 0
            };

            _controller.ModelState.AddModelError("Code", "The code is required.");

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result!.StatusCode);

            if (result.Value is ValidationProblemDetails vpd)
            {
                Assert.True(vpd.Errors.ContainsKey("Code"));
                Assert.Contains("required", vpd.Errors["Code"][0], StringComparison.OrdinalIgnoreCase);
            }
            else if (result.Value is SerializableError se)
            {
                Assert.True(se.ContainsKey("Code"));
                var mensagens = (string[])se["Code"];
                Assert.Contains(mensagens, m => m.Contains("required", StringComparison.OrdinalIgnoreCase));
            }

            _serviceMock.Verify(s => s.UpdateVoucher(It.IsAny<Voucher>()), Times.Never());
        }

        #endregion
    }
}
