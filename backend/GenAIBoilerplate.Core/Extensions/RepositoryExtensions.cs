using GenAIBoilerplate.Core.Entities;
using GenAIBoilerplate.Core.Interfaces;

namespace GenAIBoilerplate.Core.Extensions;

public static class RepositoryExtensions
{
    public static async Task<IEnumerable<ChatSession>> GetUserSessionsAsync(this IRepository<ChatSession> repository, Guid userId, CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(s => s.UserId == userId && s.IsActive, cancellationToken);
    }

    public static async Task<ChatSession?> GetByIdWithMessagesAsync(this IRepository<ChatSession> repository, Guid sessionId, CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(sessionId, cancellationToken);
    }

    public static async Task<IEnumerable<ChatMessage>> GetSessionMessagesAsync(this IRepository<ChatMessage> repository, Guid sessionId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var (items, _) = await repository.GetPagedAsync(
            page, 
            pageSize, 
            m => m.SessionId == sessionId && !m.IsDeleted,
            m => m.CreatedAt,
            false,
            cancellationToken);
        
        return items;
    }

    public static async Task<User?> GetByEmailAsync(this IRepository<User> repository, string email, CancellationToken cancellationToken)
    {
        return await repository.FindAsync(u => u.Email == email, cancellationToken);
    }

    public static async Task<IEnumerable<TenantApiKey>> GetByTenantIdAsync(this IRepository<TenantApiKey> repository, Guid tenantId, CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(k => k.TenantId == tenantId && k.IsActive, cancellationToken);
    }

    public static async Task<IEnumerable<ChatMessage>> GetTenantMessagesInRangeAsync(this IRepository<ChatMessage> repository, Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate, cancellationToken);
    }

    public static async Task<IEnumerable<ChatSession>> GetTenantSessionsInRangeAsync(this IRepository<ChatSession> repository, Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(s => s.TenantId == tenantId && s.CreatedAt >= startDate && s.CreatedAt <= endDate, cancellationToken);
    }
}
