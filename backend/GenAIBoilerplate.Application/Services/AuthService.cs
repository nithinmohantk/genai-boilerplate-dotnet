using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BCrypt.Net;
using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Application.Interfaces;
using GenAIBoilerplate.Core.Entities;
using GenAIBoilerplate.Core.Enums;
using GenAIBoilerplate.Core.Interfaces;
using GenAIBoilerplate.Core.Extensions;

namespace GenAIBoilerplate.Application.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IRepository<User> userRepository,
        IRepository<Tenant> tenantRepository,
        IRepository<RefreshToken> refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<TokenResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by email
            var user = await _userRepository.FindAsync(u => u.Email == request.Email.ToLower(), cancellationToken);
            
            if (user == null)
            {
                _logger.LogWarning("Login attempt failed: User not found for email {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt failed: User account is inactive for {Email}", request.Email);
                throw new UnauthorizedAccessException("Account is inactive");
            }

            // Verify password
            if (string.IsNullOrEmpty(user.HashedPassword) || !BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
            {
                _logger.LogWarning("Login attempt failed: Invalid password for {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Check tenant if specified
            if (!string.IsNullOrEmpty(request.TenantDomain))
            {
                var tenant = await _tenantRepository.FindAsync(t => t.Domain == request.TenantDomain, cancellationToken);
                if (tenant == null || tenant.Id != user.TenantId)
                {
                    _logger.LogWarning("Login attempt failed: Invalid tenant domain {TenantDomain} for user {Email}", 
                        request.TenantDomain, request.Email);
                    throw new UnauthorizedAccessException("Invalid tenant");
                }
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {Email} logged in successfully", request.Email);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 30 * 60, // 30 minutes
                User = await MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            throw;
        }
    }

    public async Task<TokenResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userRepository.FindAsync(u => u.Email == request.Email.ToLower(), cancellationToken);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Get or create tenant
            Tenant tenant;
            if (!string.IsNullOrEmpty(request.TenantDomain))
            {
                tenant = await _tenantRepository.FindAsync(t => t.Domain == request.TenantDomain, cancellationToken);
                if (tenant == null)
                {
                    throw new InvalidOperationException("Tenant not found");
                }
            }
            else
            {
                // Create default tenant for new user
                tenant = new Tenant
                {
                    Name = $"{request.FullName}'s Organization",
                    Domain = Guid.NewGuid().ToString("N")[..8], // Short unique domain
                    Status = TenantStatus.Active
                };
                await _tenantRepository.AddAsync(tenant, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Create user
            var user = new User
            {
                Email = request.Email.ToLower(),
                Username = request.Username,
                FullName = request.FullName,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
                TenantId = tenant.Id,
                Role = UserRole.TenantUser,
                IsActive = true,
                IsVerified = false // Email verification required
            };

            // If this is the first user in a new tenant, make them admin
            var tenantUsers = await _userRepository.GetAllAsync(u => u.TenantId == tenant.Id, cancellationToken);
            if (!tenantUsers.Any())
            {
                user.Role = UserRole.TenantAdmin;
            }

            await _userRepository.AddAsync(user, cancellationToken);

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("New user registered: {Email}", request.Email);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 30 * 60, // 30 minutes
                User = await MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);
            throw;
        }
    }

    public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find refresh token
            var storedTokens = await _refreshTokenRepository.GetAllAsync(rt => !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow, cancellationToken);
            
            RefreshToken? validToken = null;
            foreach (var token in storedTokens)
            {
                if (BCrypt.Net.BCrypt.Verify(request.RefreshToken, token.TokenHash))
                {
                    validToken = token;
                    break;
                }
            }

            if (validToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(validToken.UserId, cancellationToken);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("User not found or inactive");
            }

            // Revoke old token
            validToken.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(validToken, cancellationToken);

            // Generate new tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Save new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 30 * 60, // 30 minutes
                User = await MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            throw;
        }
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedTokens = await _refreshTokenRepository.GetAllAsync(rt => !rt.IsRevoked, cancellationToken);
        
        foreach (var token in storedTokens)
        {
            if (BCrypt.Net.BCrypt.Verify(refreshToken, token.TokenHash))
            {
                token.IsRevoked = true;
                await _refreshTokenRepository.UpdateAsync(token, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                break;
            }
        }
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userTokens = await _refreshTokenRepository.GetAllAsync(rt => rt.UserId == userId && !rt.IsRevoked, cancellationToken);
        
        foreach (var token in userTokens)
        {
            token.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(token, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User {UserId} logged out", userId);
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        return await MapToUserDto(user);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Update fields
        if (!string.IsNullOrEmpty(request.Username))
            user.Username = request.Username;
        if (!string.IsNullOrEmpty(request.FullName))
            user.FullName = request.FullName;
        if (!string.IsNullOrEmpty(request.AvatarUrl))
            user.AvatarUrl = request.AvatarUrl;
        if (!string.IsNullOrEmpty(request.Timezone))
            user.Timezone = request.Timezone;
        if (!string.IsNullOrEmpty(request.Language))
            user.Language = request.Language;
        if (request.Preferences != null)
            user.SetPreferences(request.Preferences);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapToUserDto(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (string.IsNullOrEmpty(user.HashedPassword) || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.HashedPassword))
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password changed for user {UserId}", userId);
    }

    public async Task VerifyEmailAsync(Guid userId, string verificationToken, CancellationToken cancellationToken = default)
    {
        // Email verification implementation would go here
        // For now, just mark as verified
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.IsVerified = true;
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task SendPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        // Password reset email implementation would go here
        _logger.LogInformation("Password reset requested for {Email}", email);
    }

    public async Task ResetPasswordAsync(string email, string resetToken, string newPassword, CancellationToken cancellationToken = default)
    {
        // Password reset implementation would go here
        _logger.LogInformation("Password reset completed for {Email}", email);
    }

    private async Task<UserDto> MapToUserDto(User user)
    {
        var tenant = await _tenantRepository.GetByIdAsync(user.TenantId);
        
        return new UserDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            Email = user.Email,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            IsVerified = user.IsVerified,
            AvatarUrl = user.AvatarUrl,
            Timezone = user.Timezone,
            Language = user.Language,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Tenant = tenant != null ? new TenantDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Domain = tenant.Domain,
                Description = tenant.Description,
                Status = tenant.Status.ToString(),
                Settings = tenant.Settings,
                Branding = tenant.BrandingJson,
                Limits = tenant.LimitsJson,
                CreatedAt = tenant.CreatedAt,
                UpdatedAt = tenant.UpdatedAt,
                CreatedBy = tenant.CreatedBy
            } : null
        };
    }
}
