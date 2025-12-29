using System.ComponentModel.DataAnnotations;

namespace MovieSpot.DTO_s
{
    /// <summary>
    /// DTOs related to the Genre entity.
    /// Used for data transfer between API and client.
    /// </summary>
    public class GenreDTO
    {
        /// <summary>
        /// DTO used for creating new genres.
        /// </summary>
        public class GenreCreateDto
        {
            [Required(ErrorMessage = "Genre name is required.")]
            [MaxLength(50, ErrorMessage = "Genre name cannot exceed 50 characters.")]
            public string Name { get; set; } = string.Empty;
        }

        /// <summary>
        /// DTO used for updating existing genres.
        /// </summary>
        public class GenreUpdateDto
        {
            [Required(ErrorMessage = "Genre ID is required.")]
            public int Id { get; set; }

            [Required(ErrorMessage = "Genre name is required.")]
            [MaxLength(50, ErrorMessage = "Genre name cannot exceed 50 characters.")]
            public string Name { get; set; } = string.Empty;
        }

        /// <summary>
        /// DTO used for returning genre data to the client.
        /// </summary>
        public class GenreResponseDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
