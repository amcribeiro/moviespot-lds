using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieSpot.Models;
using MovieSpot.Services.Vouchers;
using static MovieSpot.DTO_s.VoucherDTO;

namespace MovieSpot.Controllers
{
    /// <summary>
    /// Controller responsible for managing vouchers.
    /// Returns 200 on success, 400 for validation or invalid operation errors,
    /// and 404 when the voucher is not found.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        /// <summary>
        /// Initializes a new instance of <see cref="VoucherController"/>.
        /// </summary>
        /// <param name="voucherService">The voucher service.</param>
        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        /// <summary>
        /// Creates a new random voucher (code, value and validity).
        /// </summary>
        /// <returns>The created voucher.</returns>
        /// <response code="200">Voucher successfully created.</response>
        /// <response code="400">Error creating voucher (invalid or duplicated data).</response>
        [HttpPost]
        [ProducesResponseType(typeof(VoucherResponseDto), 200)]
        [ProducesResponseType(400)]
        [Authorize(Roles = "Staff")]
        public IActionResult Create()
        {
            try
            {
                var voucher = _voucherService.CreateVoucher();

                var response = new VoucherResponseDto
                {
                    Id = voucher.Id,
                    Code = voucher.Code,
                    Value = voucher.Value,
                    ValidUntil = voucher.ValidUntil,
                    MaxUsages = voucher.MaxUsages,
                    Usages = voucher.Usages,
                    CreatedAt = voucher.CreatedAt,
                    UpdatedAt = voucher.UpdatedAt
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a voucher by its ID.
        /// </summary>
        /// <param name="id">The voucher identifier.</param>
        /// <returns>The corresponding voucher.</returns>
        /// <response code="200">Voucher found.</response>
        /// <response code="400">Invalid ID.</response>
        /// <response code="404">Voucher not found.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(VoucherResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "Staff,User")]
        public IActionResult GetById(int id)
        {
            try
            {
                var voucher = _voucherService.GetVoucherById(id);

                var response = new VoucherResponseDto
                {
                    Id = voucher.Id,
                    Code = voucher.Code,
                    Value = voucher.Value,
                    ValidUntil = voucher.ValidUntil,
                    MaxUsages = voucher.MaxUsages,
                    Usages = voucher.Usages,
                    CreatedAt = voucher.CreatedAt,
                    UpdatedAt = voucher.UpdatedAt
                };

                return Ok(response);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Updates the data of an existing voucher.
        /// </summary>
        /// <param name="id">The ID of the voucher to update.</param>
        /// <param name="dto">The voucher data with the changes to apply.</param>
        /// <returns>The updated voucher.</returns>
        /// <response code="200">Voucher successfully updated.</response>
        /// <response code="400">Invalid data or incorrect ID.</response>
        /// <response code="404">Voucher not found.</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(VoucherResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "User,Staff")]
        public IActionResult Update(int id, [FromBody] VoucherUpdateDto dto)
        {
            if (dto == null)
                return BadRequest("Voucher cannot be null.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var voucher = new Voucher
                {
                    Id = id,
                    Code = dto.Code,
                    Value = dto.Value,
                    ValidUntil = dto.ValidUntil,
                    MaxUsages = dto.MaxUsages,
                    Usages = dto.Usages,
                    UpdatedAt = DateTime.UtcNow
                };

                _voucherService.UpdateVoucher(voucher);

                var response = new VoucherResponseDto
                {
                    Id = voucher.Id,
                    Code = voucher.Code,
                    Value = voucher.Value,
                    ValidUntil = voucher.ValidUntil,
                    MaxUsages = voucher.MaxUsages,
                    Usages = voucher.Usages,
                    CreatedAt = voucher.CreatedAt,
                    UpdatedAt = voucher.UpdatedAt
                };

                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates a voucher by code.
        /// Used by the frontend (mobile/web) before applying discounts.
        /// </summary>
        /// <param name="code">Voucher alphanumeric code.</param>
        /// <returns>The matching voucher if valid.</returns>
        /// <response code="200">Voucher valid.</response>
        /// <response code="400">Voucher expired or already fully used.</response>
        /// <response code="404">Voucher not found.</response>
        [HttpGet("validate/{code}")]
        [ProducesResponseType(typeof(VoucherResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [AllowAnonymous]
        public IActionResult ValidateVoucher(string code)
        {
            try
            {
                var voucher = _voucherService.GetVoucherByCode(code);

                var response = new VoucherResponseDto
                {
                    Id = voucher.Id,
                    Code = voucher.Code,
                    Value = voucher.Value,
                    ValidUntil = voucher.ValidUntil,
                    MaxUsages = voucher.MaxUsages,
                    Usages = voucher.Usages,
                    CreatedAt = voucher.CreatedAt,
                    UpdatedAt = voucher.UpdatedAt
                };

                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Expirado ou sem usos
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                // Código não encontrado
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retorna um relatório de performance dos vouchers (taxa de utilização).
        /// </summary>
        [HttpGet("stats/performance")]
        [Authorize(Roles = "Staff")]
        public ActionResult<List<VoucherPerformanceDto>> GetVoucherPerformance()
        {
            try
            {
                var stats = _voucherService.GetVoucherPerformance();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, ...);
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}
