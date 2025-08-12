using System.Security.Claims;
using GenAIBoilerplate.Core.Entities;

namespace GenAIBoilerplate.Application.Interfaces;

/// <summary>
/// JWT token service interface
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate access token for user
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generate refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Get principal from expired token
    /// </summary>
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

    /// <summary>
    /// Validate token and get claims
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Get user ID from claims
    /// </summary>
    Guid? GetUserIdFromClaims(ClaimsPrincipal principal);

    /// <summary>
    /// Get tenant ID from claims
    /// </summary>
    Guid? GetTenantIdFromClaims(ClaimsPrincipal principal);
}
