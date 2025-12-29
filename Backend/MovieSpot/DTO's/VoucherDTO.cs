using System;
using System.ComponentModel.DataAnnotations;

namespace MovieSpot.DTO_s
{
    public class VoucherDTO
    {
        /// <summary>
        /// DTO used to create new vouchers.
        /// </summary>
        public class VoucherCreateDto
        {
            [Required(ErrorMessage = "The value is required.")]
            [Range(0.01, 0.99, ErrorMessage = "The voucher value must be between 0.01 and 0.99.")]
            public decimal Value { get; set; }

            [Required(ErrorMessage = "The expiration date is required.")]
            public DateTime ValidUntil { get; set; }

            [Required(ErrorMessage = "The maximum number of usages is required.")]
            [Range(1, int.MaxValue, ErrorMessage = "The maximum number of usages must be at least 1.")]
            public int MaxUsages { get; set; }
        }

        /// <summary>
        /// DTO used to update existing vouchers.
        /// </summary>
        public class VoucherUpdateDto
        {
            [Required(ErrorMessage = "The ID is required.")]
            public int Id { get; set; }

            [Required(ErrorMessage = "The code is required.")]
            [MaxLength(16, ErrorMessage = "The code cannot exceed 16 characters.")]
            public string Code { get; set; }

            [Required(ErrorMessage = "The value is required.")]
            [Range(0.01, 0.99, ErrorMessage = "The voucher value must be between 0.01 and 0.99.")]
            public decimal Value { get; set; }

            [Required(ErrorMessage = "The expiration date is required.")]
            public DateTime ValidUntil { get; set; }

            [Required(ErrorMessage = "The maximum number of usages is required.")]
            [Range(1, int.MaxValue, ErrorMessage = "The maximum number of usages must be at least 1.")]
            public int MaxUsages { get; set; }

            public int Usages { get; set; }
        }

        /// <summary>
        /// DTO used to return voucher data to the client.
        /// </summary>
        public class VoucherResponseDto
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public decimal Value { get; set; }
            public DateTime ValidUntil { get; set; }
            public int MaxUsages { get; set; }
            public int Usages { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}
