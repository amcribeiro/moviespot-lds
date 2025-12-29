using System;
using System.ComponentModel.DataAnnotations;

namespace MovieSpot.DTO_s
{
    /// <summary>
    /// DTOs related to the CinemaHall entity.
    /// Used for data transfer between API and client.
    /// </summary>
    public class CinemaHallDTO
    {
        /// <summary>
        /// DTO used to display basic cinema hall information.
        /// </summary>
        public class CinemaHallReadDto
        {
            /// <summary>
            /// Unique identifier of the cinema hall.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Name or number of the cinema hall.
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Identifier of the cinema to which this hall belongs.
            /// </summary>
            public int CinemaId { get; set; }
        }

        /// <summary>
        /// DTO used to create a new cinema hall.
        /// </summary>
        public class CinemaHallCreateDto
        {
            /// <summary>
            /// Name or number of the new cinema hall.
            /// </summary>
            [Required(ErrorMessage = "Cinema hall name is required.")]
            [MaxLength(100, ErrorMessage = "Cinema hall name cannot exceed 100 characters.")]
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Identifier of the cinema where this hall will be created.
            /// </summary>
            [Required(ErrorMessage = "Cinema ID is required.")]
            public int CinemaId { get; set; }
        }

        /// <summary>
        /// DTO used to update an existing cinema hall.
        /// </summary>
        public class CinemaHallUpdateDto
        {
            /// <summary>
            /// Identifier of the cinema hall to be updated.
            /// </summary>
            [Required(ErrorMessage = "Cinema hall ID is required.")]
            public int Id { get; set; }

            /// <summary>
            /// Updated name or number of the cinema hall.
            /// </summary>
            [Required(ErrorMessage = "Cinema hall name is required.")]
            [MaxLength(100, ErrorMessage = "Cinema hall name cannot exceed 100 characters.")]
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Identifier of the cinema this hall belongs to.
            /// </summary>
            [Required(ErrorMessage = "Cinema ID is required.")]
            public int CinemaId { get; set; }
        }

        /// <summary>
        /// DTO used to display detailed information about a cinema hall,
        /// including cinema relationship details.
        /// </summary>
        public class CinemaHallDetailsDto
        {
            /// <summary>
            /// Unique identifier of the cinema hall.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Name or number of the cinema hall.
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// The ID of the parent cinema.
            /// </summary>
            public int CinemaId { get; set; }

            /// <summary>
            /// Optional: The name of the cinema this hall belongs to.
            /// </summary>
            public string? CinemaName { get; set; }

            /// <summary>
            /// Creation timestamp.
            /// </summary>
            public DateTime CreatedAt { get; set; }

            /// <summary>
            /// Last update timestamp.
            /// </summary>
            public DateTime UpdatedAt { get; set; }
        }
    }
}
