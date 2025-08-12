using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Application.Interfaces;
using GenAIBoilerplate.Core.Entities;
using GenAIBoilerplate.Core.Interfaces;
using GenAIBoilerplate.Core.Enums;
using GenAIBoilerplate.Core.Extensions;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace GenAIBoilerplate.Application.Services;

public class TenantService : ITenantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TenantService> _logger;

    public TenantService(IUnitOfWork unitOfWork, ILogger<TenantService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<TenantDto>> GetUserTenantsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Enumerable.Empty<TenantDto>();
        }

        // For simplicity, return the user's tenant
        // In a more complex scenario, users could belong to multiple tenants
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(user.TenantId, cancellationToken);
        if (tenant == null)
        {
            return Enumerable.Empty<TenantDto>();
        }

        return new List<TenantDto>
        {
            new TenantDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Description = tenant.Description,
                Status = tenant.Status.ToString(),
                Settings = tenant.Settings,
                CreatedAt = tenant.CreatedAt,
                UpdatedAt = tenant.UpdatedAt
            }
        };
    }

    public async Task<TenantDto?> GetTenantAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        // Verify user has access to this tenant
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null || user.TenantId != tenantId)
        {
            throw new UnauthorizedAccessException("Access denied to tenant");
        }

        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null)
        {
            return null;
        }

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Description = tenant.Description,
            Status = tenant.Status.ToString(),
            Settings = tenant.Settings,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };
    }

    public async Task<TenantDto> CreateTenantAsync(Guid userId, CreateTenantRequestDto request, CancellationToken cancellationToken)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Status = TenantStatus.Active,
            Settings = request.Settings ?? "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);
        
        // Update user to belong to this tenant if they don't already have one
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user != null && user.TenantId == Guid.Empty)
        {
            user.TenantId = tenant.Id;
            user.Role = UserRole.Admin; // First user becomes admin
            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Description = tenant.Description,
            Status = tenant.Status.ToString(),
            Settings = tenant.Settings,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };
    }

    public async Task<TenantDto?> UpdateTenantAsync(Guid tenantId, Guid userId, UpdateTenantRequestDto request, CancellationToken cancellationToken)
    {
        // Verify user has admin access to this tenant
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null || user.TenantId != tenantId || user.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Admin access required to update tenant");
        }

        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null)
        {
            return null;
        }

        // Update only provided fields
        if (!string.IsNullOrEmpty(request.Name))
            tenant.Name = request.Name;
        if (!string.IsNullOrEmpty(request.Description))
            tenant.Description = request.Description;
        if (!string.IsNullOrEmpty(request.Settings))
            tenant.Settings = request.Settings;

        tenant.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Tenants.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Description = tenant.Description,
            Status = tenant.Status.ToString(),
            Settings = tenant.Settings,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };
    }

    public async Task<bool> DeleteTenantAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        // Verify user has admin access to this tenant
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null || user.TenantId != tenantId || user.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Admin access required to delete tenant");
        }

        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null)
        {
            return false;
        }

        // Set status to inactive instead of hard delete
        tenant.Status = TenantStatus.Suspended;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Tenants.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IEnumerable<TenantApiKeyDto>> GetTenantApiKeysAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        // Verify user has admin access to this tenant
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null || user.TenantId != tenantId || user.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Admin access required to view API keys");
        }

        var apiKeys = await _unitOfWork.TenantApiKeys.GetByTenantIdAsync(tenantId, cancellationToken);
        
        return apiKeys.Select(key => new TenantApiKeyDto
        {
            Id = key.Id,
            TenantId = key.TenantId,
            Name = key.Name,
            KeyHash = key.KeyHash, // In production, you wouldn't return the hash
            IsActive = key.IsActive,
            ExpiresAt = key.ExpiresAt,
            LastUsedAt = key.LastUsedAt,
            CreatedAt = key.CreatedAt
        });
    }

    public async Task<TenantApiKeyDto> CreateApiKeyAsync(Guid tenantId, Guid userId, CreateApiKeyRequestDto request, CancellationToken cancellationToken)
    {
        // Verify user has admin access to this tenant
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null || user.TenantId != tenantId || user.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Admin access required to create API keys");
        }

        // Generate a secure API key
        var keyValue = GenerateApiKey();
        var keyHash = HashApiKey(keyValue);

        var apiKey = new TenantApiKey
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            KeyHash = keyHash,
            IsActive = true,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.TenantApiKeys.AddAsync(apiKey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TenantApiKeyDto
        {
            Id = apiKey.Id,
            TenantId = apiKey.TenantId,
            Name = apiKey.Name,
            KeyHash = keyValue, // Return the actual key only on creation
            IsActive = apiKey.IsActive,
            ExpiresAt = apiKey.ExpiresAt,
            LastUsedAt = apiKey.LastUsedAt,
            CreatedAt = apiKey.CreatedAt
        };
    }

    public async Task<bool> RevokeApiKeyAsync(Guid tenantId, Guid apiKeyId, Guid userId, CancellationToken cancellationToken)
    {
        // Verify user has admin access to this tenant
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null || user.TenantId != tenantId || user.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Admin access required to revoke API keys");
        }

        var apiKey = await _unitOfWork.TenantApiKeys.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey == null || apiKey.TenantId != tenantId)
        {
            return false;
        }

        apiKey.IsActive = false;
        await _unitOfWork.TenantApiKeys.UpdateAsync(apiKey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<TenantUsageDto> GetTenantUsageAsync(Guid tenantId, Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        // Verify user has access to this tenant
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null || user.TenantId != tenantId)
        {
            throw new UnauthorizedAccessException("Access denied to tenant usage");
        }

        // Get messages within date range for the tenant
        var messages = await _unitOfWork.ChatMessages.GetTenantMessagesInRangeAsync(tenantId, startDate, endDate, cancellationToken);
        var sessions = await _unitOfWork.ChatSessions.GetTenantSessionsInRangeAsync(tenantId, startDate, endDate, cancellationToken);

        var totalMessages = messages.Count();
        var totalTokens = messages.Sum(m => m.TokenCount ?? 0);
        var totalSessions = sessions.Count();

        // Group by model
        var modelUsage = messages
            .Where(m => !string.IsNullOrEmpty(m.Model))
            .GroupBy(m => m.Model!)
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by day
        var dailyUsage = messages
            .GroupBy(m => m.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        return new TenantUsageDto
        {
            TenantId = tenantId,
            TotalMessages = totalMessages,
            TotalTokens = totalTokens,
            TotalSessions = totalSessions,
            StartDate = startDate,
            EndDate = endDate,
            ModelUsage = modelUsage,
            DailyUsage = dailyUsage
        };
    }

    public async Task AddUserToTenantAsync(Guid tenantId, string email, UserRole role, Guid requestingUserId, CancellationToken cancellationToken)
    {
        // Verify requesting user has admin access to this tenant
        var requestingUser = await _unitOfWork.Users.GetByIdAsync(requestingUserId, cancellationToken);
        if (requestingUser == null || requestingUser.TenantId != tenantId || requestingUser.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Admin access required to add users to tenant");
        }

        // Find user by email
        var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (user == null)
        {
            throw new ArgumentException("User not found", nameof(email));
        }

        // Update user's tenant and role
        user.TenantId = tenantId;
        user.Role = role;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoveUserFromTenantAsync(Guid tenantId, Guid userIdToRemove, Guid requestingUserId, CancellationToken cancellationToken)
    {
        // Verify requesting user has admin access to this tenant
        var requestingUser = await _unitOfWork.Users.GetByIdAsync(requestingUserId, cancellationToken);
        if (requestingUser == null || requestingUser.TenantId != tenantId || requestingUser.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Admin access required to remove users from tenant");
        }

        // Prevent removing self
        if (requestingUserId == userIdToRemove)
        {
            return false;
        }

        var userToRemove = await _unitOfWork.Users.GetByIdAsync(userIdToRemove, cancellationToken);
        if (userToRemove == null || userToRemove.TenantId != tenantId)
        {
            return false;
        }

        // Remove user from tenant (set to empty guid or create a default tenant)
        userToRemove.TenantId = Guid.Empty;
        userToRemove.Role = UserRole.User;

        await _unitOfWork.Users.UpdateAsync(userToRemove, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string GenerateApiKey()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32]; // 256 bits
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-").TrimEnd('=');
    }

    private static string HashApiKey(string apiKey)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hashBytes);
    }
}
