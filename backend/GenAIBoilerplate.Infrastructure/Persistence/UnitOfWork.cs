using Microsoft.EntityFrameworkCore.Storage;
using GenAIBoilerplate.Core.Interfaces;
using GenAIBoilerplate.Core.Entities;

namespace GenAIBoilerplate.Infrastructure.Persistence;

/// <summary>
/// Unit of work implementation
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<User>? _users;
    private IRepository<Tenant>? _tenants;
    private IRepository<ChatSession>? _chatSessions;
    private IRepository<ChatMessage>? _chatMessages;
    private IRepository<TenantApiKey>? _tenantApiKeys;
    private IRepository<RefreshToken>? _refreshTokens;
    private IRepository<UserAuthProvider>? _userAuthProviders;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Tenant> Tenants => _tenants ??= new Repository<Tenant>(_context);
    public IRepository<ChatSession> ChatSessions => _chatSessions ??= new Repository<ChatSession>(_context);
    public IRepository<ChatMessage> ChatMessages => _chatMessages ??= new Repository<ChatMessage>(_context);
    public IRepository<TenantApiKey> TenantApiKeys => _tenantApiKeys ??= new Repository<TenantApiKey>(_context);
    public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);
    public IRepository<UserAuthProvider> UserAuthProviders => _userAuthProviders ??= new Repository<UserAuthProvider>(_context);

    public bool HasActiveTransaction => _transaction != null;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transaction already started");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit");
        }

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to rollback");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
