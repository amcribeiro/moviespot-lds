namespace MovieSpot.Services.Tokens
{
    using MovieSpot.Models;

    /// <summary>
    /// Defines the contract for generating and refreshing authentication tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a new pair of tokens (access + refresh) for the authenticated user.
        /// </summary>
        /// <param name="user">The authenticated user.</param>
        /// <returns>A tuple containing the AccessToken and RefreshToken.</returns>
        (string AccessToken, string RefreshToken) GenerateTokens(User user);

        /// <summary>
        /// Generates a new AccessToken based on a valid RefreshToken.
        /// </summary>
        /// <param name="refreshToken">The refresh token to validate.</param>
        /// <returns>A new AccessToken if the refresh token is valid.</returns>
        (string new_acesstoken, string new_refreshtoken) RefreshAccessToken(string refreshToken);
    }
}
