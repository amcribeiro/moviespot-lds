using System.ComponentModel.DataAnnotations;

namespace MovieSpot.DTO_s
{
    public class UserDTO
    {
        /// <summary>
        /// DTO used for creating new users.
        /// Contains only the fields required for POST /User.
        /// </summary>
        public class UserCreateDto
        {
            [Required(ErrorMessage = "Name is required.")]
            [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email format.")]
            [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            [MaxLength(255, ErrorMessage = "Password cannot exceed 255 characters.")]
            public string Password { get; set; } = string.Empty;

            [Phone(ErrorMessage = "Invalid phone number format.")]
            [MaxLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
            public string? Phone { get; set; }

            [MaxLength(20, ErrorMessage = "Role cannot exceed 20 characters.")]
            public string Role { get; set; } = "User";
        }

        /// <summary>
        /// DTO used for updating existing users.
        /// Contains only the fields that can be modified.
        /// </summary>
        public class UserUpdateDto
        {
            [Required(ErrorMessage = "User ID is required.")]
            public int Id { get; set; }

            [Required(ErrorMessage = "Name is required.")]
            [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email format.")]
            [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
            public string Email { get; set; } = string.Empty;

            [MaxLength(255, ErrorMessage = "Password cannot exceed 255 characters.")]
            public string? Password { get; set; }

            [Phone(ErrorMessage = "Invalid phone number format.")]
            [MaxLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
            public string? Phone { get; set; }

            [MaxLength(20, ErrorMessage = "Role cannot exceed 20 characters.")]
            public string Role { get; set; } = "User";

            [MaxLength(20, ErrorMessage = "Account status cannot exceed 20 characters.")]
            public string? AccountStatus { get; set; }
        }

        /// <summary>
        /// DTO used for returning user data to the client.
        /// Used in responses for GET, POST, and PUT requests.
        /// </summary>
        public class UserResponseDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Phone { get; set; }
            public string Role { get; set; } = string.Empty;
            public string AccountStatus { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        /// <summary>
        /// Data Transfer Object (DTO) used to handle password reset requests.
        /// </summary>
        /// <remarks>
        /// This DTO is sent from the client when a user submits the password reset form.
        /// It contains the JWT token (received via email) and the new password to be set.
        /// The token is validated on the server side to identify the user and authorize the reset.
        /// </remarks>
        public class ResetPasswordRequestDto
        {
            public string Token { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }

        public class ForgotPasswordRequestDto
        {
            public string? Email { get; set; }
        }
    }
}
