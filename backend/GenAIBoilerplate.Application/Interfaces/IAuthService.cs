using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Core.Entities;

namespace GenAIBoilerplate.Application.Interfaces;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    Task<TokenResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Register new user
    /// </summary>
    Task<TokenResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh access token
    /// </summary>
    Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke refresh token
    /// </summary>
    Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logout user by revoking all tokens
    /// </summary>
    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current user information
    /// </summary>
    Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user profile
    /// </summary>
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Change user password
    /// </summary>
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify user email
    /// </summary>
    Task VerifyEmailAsync(Guid userId, string verificationToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send password reset email
    /// </summary>
    Task SendPasswordResetAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset password with token
    /// </summary>
    Task ResetPasswordAsync(string email, string resetToken, string newPassword, CancellationToken cancellationToken = default);
}
