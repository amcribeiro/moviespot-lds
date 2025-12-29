using MovieSpot.Models;

namespace MovieSpot.Services.Vouchers
{
    /// <summary>
    /// Defines the contract for managing vouchers in the system.
    /// </summary>
    public interface IVoucherService
    {
        /// <summary>
        /// Creates a new voucher in the system.
        /// </summary>
        /// <returns>The newly created voucher object.</returns>
        Voucher CreateVoucher();

        /// <summary>
        /// Retrieves a voucher by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the voucher.</param>
        /// <returns>The voucher associated with the specified ID.</returns>
        Voucher GetVoucherById(int id);

        /// <summary>
        /// Updates the details of an existing voucher.
        /// </summary>
        /// <param name="voucher">The voucher object containing the updated details.</param>
        void UpdateVoucher(Voucher voucher);

        /// <summary>
        /// Retrieves a voucher by its CODE (string).
        /// Validates expiration and max usages.
        /// </summary>
        /// <param name="code">Voucher alphanumeric code.</param>
        /// <returns>The corresponding voucher.</returns>
        Voucher GetVoucherByCode(string code);

        List<VoucherPerformanceDto> GetVoucherPerformance();
    }
}
