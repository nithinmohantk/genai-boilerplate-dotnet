using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Core.Enums;

namespace GenAIBoilerplate.Application.Interfaces;

public interface ITenantService
{
    Task<IEnumerable<TenantDto>> GetUserTenantsAsync(Guid userId, CancellationToken cancellationToken);
    Task<TenantDto?> GetTenantAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken);
    Task<TenantDto> CreateTenantAsync(Guid userId, CreateTenantRequestDto request, CancellationToken cancellationToken);
    Task<TenantDto?> UpdateTenantAsync(Guid tenantId, Guid userId, UpdateTenantRequestDto request, CancellationToken cancellationToken);
    Task<bool> DeleteTenantAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken);

    Task<IEnumerable<TenantApiKeyDto>> GetTenantApiKeysAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken);
    Task<TenantApiKeyDto> CreateApiKeyAsync(Guid tenantId, Guid userId, CreateApiKeyRequestDto request, CancellationToken cancellationToken);
    Task<bool> RevokeApiKeyAsync(Guid tenantId, Guid apiKeyId, Guid userId, CancellationToken cancellationToken);

    Task<TenantUsageDto> GetTenantUsageAsync(Guid tenantId, Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
    Task AddUserToTenantAsync(Guid tenantId, string email, UserRole role, Guid requestingUserId, CancellationToken cancellationToken);
    Task<bool> RemoveUserFromTenantAsync(Guid tenantId, Guid userIdToRemove, Guid requestingUserId, CancellationToken cancellationToken);
}
