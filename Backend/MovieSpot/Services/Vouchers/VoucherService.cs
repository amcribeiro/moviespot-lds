using Microsoft.EntityFrameworkCore;
using MovieSpot.Data;
using MovieSpot.Models;

namespace MovieSpot.Services.Vouchers
{
    /// <summary>
    /// Provides services for managing vouchers in the system,
    /// including creation, retrieval, and update.
    /// </summary>
    public class VoucherService : IVoucherService
    {
        private readonly ApplicationDbContext _context;

        public VoucherService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new voucher with a random code, value, and expiration date.
        /// </summary>
        /// <returns>The newly created voucher object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a generated code already exists.</exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while saving the voucher.</exception>
        public Voucher CreateVoucher()
        {
            var random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var code = new string(Enumerable.Range(0, 12)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());

            var voucher = new Voucher
            {
                Code = code,
                Value = Math.Round((decimal)(random.NextDouble() * (0.15 - 0.05) + 0.05), 2),
                ValidUntil = DateTime.UtcNow.AddMonths(1),
                Usages = 0,

                MaxUsages = random.Next(1, 6)
            };

            if (_context.Voucher.Any(v => v.Code == voucher.Code))
                throw new InvalidOperationException("A voucher with this code already exists.");

            if (voucher.ValidUntil <= DateTime.UtcNow)
                throw new ArgumentOutOfRangeException(nameof(voucher.ValidUntil), "The expiration date must be in the future.");

            if (voucher.Value <= 0 || voucher.Value >= 1)
                throw new ArgumentOutOfRangeException(nameof(voucher.Value), "The voucher value must be between 0.01 and 0.99.");

            try
            {
                _context.Voucher.Add(voucher);
                _context.SaveChanges();
                return voucher;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Error creating the voucher in the database.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while creating the voucher.", ex);
            }
        }

        /// <summary>
        /// Retrieves a voucher by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the voucher.</param>
        /// <returns>The corresponding voucher object.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the ID is invalid.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no voucher is found.</exception>
        public Voucher GetVoucherById(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "The voucher ID must be greater than zero.");

            var voucher = _context.Voucher.Find(id);

            if (voucher == null)
                throw new KeyNotFoundException($"Voucher with ID {id} was not found.");

            return voucher;
        }

        /// <summary>
        /// Updates the details of an existing voucher.
        /// </summary>
        /// <param name="voucher">The voucher object with updated details.</param>
        /// <exception cref="ArgumentNullException">Thrown when the voucher object is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the voucher ID is invalid.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the voucher does not exist in the database.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when updated values are invalid.</exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while updating the database.</exception>
        public void UpdateVoucher(Voucher voucher)
        {
            if (voucher == null)
                throw new ArgumentNullException(nameof(voucher), "The voucher cannot be null.");

            if (voucher.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(voucher.Id), "The voucher ID must be greater than zero.");

            var existing = _context.Voucher.Find(voucher.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Voucher with ID {voucher.Id} was not found.");

            if (voucher.ValidUntil <= DateTime.UtcNow)
                throw new ArgumentOutOfRangeException(nameof(voucher.ValidUntil), "The expiration date must be in the future.");

            if (voucher.Value <= 0 || voucher.Value >= 1)
                throw new ArgumentOutOfRangeException(nameof(voucher.Value), "The voucher value must be greater than 0 and less than 1.");

            if (existing.Usages >= existing.MaxUsages)
                throw new InvalidOperationException("The voucher has already reached the maximum number of usages.");

            try
            {
                existing.Usages++;

                existing.Code = voucher.Code;
                existing.Value = voucher.Value;
                existing.ValidUntil = voucher.ValidUntil;
                existing.UpdatedAt = DateTime.UtcNow;

                _context.Voucher.Update(existing);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Error updating the voucher in the database.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while updating the voucher.", ex);
            }
        }

        public Voucher GetVoucherByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentNullException(nameof(code), "Voucher code is required.");

            var voucher = _context.Voucher
                .FirstOrDefault(v => v.Code == code);

            if (voucher == null)
                throw new KeyNotFoundException($"Voucher with Code '{code}' was not found.");

            if (voucher.ValidUntil < DateTime.UtcNow)
                throw new InvalidOperationException("Voucher expired.");

            if (voucher.Usages >= voucher.MaxUsages)
                throw new InvalidOperationException("Voucher fully used.");

            return voucher;
        }
        public List<VoucherPerformanceDto> GetVoucherPerformance()
        {
            return _context.Voucher
                .Select(v => new VoucherPerformanceDto
                {
                    Code = v.Code,
                    UsageCount = v.Usages,
                    MaxUsages = v.MaxUsages,
                    UsagePercentage = v.MaxUsages > 0
                        ? Math.Round(((double)v.Usages / v.MaxUsages) * 100, 1)
                        : 0,
                    IsDepleted = v.Usages >= v.MaxUsages
                })
                .OrderByDescending(v => v.UsagePercentage)
                .ToList();
        }
    }
}
