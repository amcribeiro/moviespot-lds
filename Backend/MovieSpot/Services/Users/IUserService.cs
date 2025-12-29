using Microsoft.EntityFrameworkCore;
using MovieSpot.Models;
namespace MovieSpot.Services.Users
{
    public interface IUserService
    {
        /// <summary>
        /// Creates a new user in the system.
        /// <param name="newUser">The user object containing the details for registration.</param>
        User CreateUser(User newUser);

        /// <summary>
        /// Registers a new user and returns authentication tokens.
        /// </summary>
        /// <param name="newUser">The user object containing the details for registration.</param>
        /// <returns>
        /// A <see cref="LoginResponseModel"/> containing the access and refresh tokens for the authenticated user.
        /// </returns>
        LoginResponseModel RegisterUser(User newUser);

        /// <summary>
        /// Authenticates a user based on their email and password.
        /// <param name="email">The email of the user attempting to log in.</param>
        /// <param name="password">The password of the user attempting to log in.</param>
        /// <returns>The authenticated user object.</returns>
        LoginResponseModel LoginUser(string email, string password);


        /// <summary>
        /// Updates the details of an existing user.
        /// <param name="id">The unique identifier of the user to be updated.</param>
        /// <param name="updatedUser">The user object containing the updated details.</param>
        User UpdateUser(int id, User updatedUser);


        /// <summary>
        /// Deletes a user from the system.
        /// <param name="id">The unique identifier of the user to be deleted.</param>
        User DeleteUser(int id);


        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>The user associated with the specified ID.</returns>
        User GetUserById(int id);

        /// <summary>
        /// Retrieves all registered users.
        /// </summary>
        /// <returns>An enumerable collection of users.</returns>
        IEnumerable<User> GetAllUsers();

        /// <summary>
        /// Initiates a password reset process by generating and sending a reset token to the user’s email.
        /// </summary>
        /// <param name="email">The email of the user requesting the password reset.</param>
        /// <returns><c>true</c> if the reset email was successfully sent; otherwise, <c>false</c>.</returns>
        Task<bool> ForgotPassword(string email);

        /// <summary>
        /// Resets a user’s password using a valid reset token.
        /// </summary>
        /// <param name="token">The password reset token previously generated.</param>
        /// <param name="newPassword">The new password to set for the user.</param>
        /// <returns><c>true</c> if the password was successfully reset; otherwise, <c>false</c>.</returns>
        bool ResetPassword(string token, string newPassword);

        List<TopUserDto> GetTopUsers(int topN);

        public List<TopSpenderDto> GetTopSpenders(int count);
    }
}