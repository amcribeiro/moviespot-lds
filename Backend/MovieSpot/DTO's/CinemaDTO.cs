using System;
using System.ComponentModel.DataAnnotations;

namespace MovieSpot.DTO_s
{
    /// <summary>
    /// DTOs related to the Cinema entity.
    /// Used for data transfer between API and client.
    /// </summary>
    public class CinemaDTO
    {
        /// <summary>
        /// DTO used for creating new cinemas.
        /// Contains only the fields required for POST /Cinema.
        /// </summary>
        public class CinemaCreateDto
        {
            [Required(ErrorMessage = "Cinema name is required.")]
            [MaxLength(100, ErrorMessage = "Cinema name cannot exceed 100 characters.")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Street is required.")]
            [MaxLength(150, ErrorMessage = "Street cannot exceed 150 characters.")]
            public string Street { get; set; } = string.Empty;

            [Required(ErrorMessage = "City is required.")]
            [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
            public string City { get; set; } = string.Empty;

            [MaxLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
            public string? State { get; set; }

            [MaxLength(10, ErrorMessage = "Zip code cannot exceed 10 characters.")]
            public string? ZipCode { get; set; }

            [Required(ErrorMessage = "Country is required.")]
            [MaxLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
            public string Country { get; set; } = string.Empty;

            [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
            public decimal Latitude { get; set; }

            [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
            public decimal Longitude { get; set; }
        }

        /// <summary>
        /// DTO used for updating existing cinemas.
        /// Contains only the fields that can be modified.
        /// </summary>
        public class CinemaUpdateDto
        {
            [Required(ErrorMessage = "Cinema ID is required.")]
            public int Id { get; set; }

            [Required(ErrorMessage = "Cinema name is required.")]
            [MaxLength(100, ErrorMessage = "Cinema name cannot exceed 100 characters.")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Street is required.")]
            [MaxLength(150, ErrorMessage = "Street cannot exceed 150 characters.")]
            public string Street { get; set; } = string.Empty;

            [Required(ErrorMessage = "City is required.")]
            [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
            public string City { get; set; } = string.Empty;

            [MaxLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
            public string? State { get; set; }

            [MaxLength(10, ErrorMessage = "Zip code cannot exceed 10 characters.")]
            public string? ZipCode { get; set; }

            [Required(ErrorMessage = "Country is required.")]
            [MaxLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
            public string Country { get; set; } = string.Empty;

            [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
            public decimal Latitude { get; set; }

            [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
            public decimal Longitude { get; set; }
        }

        /// <summary>
        /// DTO used for returning cinema data to the client.
        /// Used in responses for GET, POST, and PUT requests.
        /// </summary>
        public class CinemaResponseDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Street { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string? State { get; set; }
            public string? ZipCode { get; set; }
            public string Country { get; set; } = string.Empty;
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public int? TotalCinemaHalls { get; set; }
        }
    }
}
