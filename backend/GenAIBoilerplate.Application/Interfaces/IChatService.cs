using GenAIBoilerplate.Application.DTOs;

namespace GenAIBoilerplate.Application.Interfaces;

/// <summary>
/// Chat service interface
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Get user's chat sessions
    /// </summary>
    Task<IEnumerable<ChatSessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Create new chat session
    /// </summary>
    Task<ChatSessionDto> CreateSessionAsync(Guid userId, CreateSessionRequestDto request, CancellationToken cancellationToken);

    /// <summary>
    /// Get chat session by ID
    /// </summary>
    Task<ChatSessionDto?> GetSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Update chat session
    /// </summary>
    Task<ChatSessionDto?> UpdateSessionAsync(Guid sessionId, Guid userId, UpdateSessionRequestDto request, CancellationToken cancellationToken);

    /// <summary>
    /// Delete chat session
    /// </summary>
    Task<bool> DeleteSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Get session messages
    /// </summary>
    Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(Guid sessionId, Guid userId, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Send message to chat session
    /// </summary>
    Task<ChatResponseDto> SendMessageAsync(Guid sessionId, Guid userId, ChatRequestDto request, CancellationToken cancellationToken);

    /// <summary>
    /// Send message with streaming response
    /// </summary>
    Task SendMessageStreamAsync(Guid sessionId, Guid userId, ChatRequestDto request, Func<string, Task> onChunkReceived, CancellationToken cancellationToken);
}
