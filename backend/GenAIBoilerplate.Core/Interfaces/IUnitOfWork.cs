using GenAIBoilerplate.Core.Entities;

namespace GenAIBoilerplate.Core.Interfaces;

/// <summary>
/// Unit of work interface for managing database transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// User repository
    /// </summary>
    IRepository<User> Users { get; }

    /// <summary>
    /// Tenant repository
    /// </summary>
    IRepository<Tenant> Tenants { get; }

    /// <summary>
    /// Chat session repository
    /// </summary>
    IRepository<ChatSession> ChatSessions { get; }

    /// <summary>
    /// Chat message repository
    /// </summary>
    IRepository<ChatMessage> ChatMessages { get; }

    /// <summary>
    /// Tenant API key repository
    /// </summary>
    IRepository<TenantApiKey> TenantApiKeys { get; }

    /// <summary>
    /// Refresh token repository
    /// </summary>
    IRepository<RefreshToken> RefreshTokens { get; }

    /// <summary>
    /// User auth provider repository
    /// </summary>
    IRepository<UserAuthProvider> UserAuthProviders { get; }

    /// <summary>
    /// Save all changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin a database transaction
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit the current transaction
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if there's an active transaction
    /// </summary>
    bool HasActiveTransaction { get; }
}
