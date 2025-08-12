using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Application.Interfaces;
using GenAIBoilerplate.Core.Entities;
using GenAIBoilerplate.Core.Interfaces;
using GenAIBoilerplate.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace GenAIBoilerplate.Application.Services;

public class SimpleChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIService _aiService;
    private readonly ILogger<SimpleChatService> _logger;

    public SimpleChatService(
        IUnitOfWork unitOfWork,
        IAIService aiService,
        ILogger<SimpleChatService> logger)
    {
        _unitOfWork = unitOfWork;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<IEnumerable<ChatSessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sessions = await _unitOfWork.ChatSessions.GetUserSessionsAsync(userId, cancellationToken);
        
        return sessions.Select(s => new ChatSessionDto
        {
            Id = s.Id,
            Title = s.Title,
            Model = s.ModelName,
            SystemPrompt = s.Description,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            MessageCount = s.MessageCount
        });
    }

    public async Task<ChatSessionDto> CreateSessionAsync(Guid userId, CreateSessionRequestDto request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new ArgumentException("User not found", nameof(userId));
        }

        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = user.TenantId,
            Title = request.Title,
            ModelName = request.Model ?? "gpt-3.5-turbo",
            Description = request.SystemPrompt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ChatSessions.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ChatSessionDto
        {
            Id = session.Id,
            Title = session.Title,
            Model = session.ModelName,
            SystemPrompt = session.Description,
            IsActive = session.IsActive,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            MessageCount = 0
        };
    }

    public async Task<ChatSessionDto?> GetSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.ChatSessions.GetByIdAsync(sessionId, cancellationToken);
        
        if (session == null || session.UserId != userId)
        {
            return null;
        }

        return new ChatSessionDto
        {
            Id = session.Id,
            Title = session.Title,
            Model = session.ModelName,
            SystemPrompt = session.Description,
            IsActive = session.IsActive,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            MessageCount = session.MessageCount
        };
    }

    public async Task<ChatSessionDto?> UpdateSessionAsync(Guid sessionId, Guid userId, UpdateSessionRequestDto request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.ChatSessions.GetByIdAsync(sessionId, cancellationToken);
        
        if (session == null || session.UserId != userId)
        {
            return null;
        }

        // Update only provided fields
        if (!string.IsNullOrEmpty(request.Title))
            session.Title = request.Title;
        if (!string.IsNullOrEmpty(request.Model))
            session.ModelName = request.Model;
        if (!string.IsNullOrEmpty(request.SystemPrompt))
            session.Description = request.SystemPrompt;

        session.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ChatSessions.UpdateAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ChatSessionDto
        {
            Id = session.Id,
            Title = session.Title,
            Model = session.ModelName,
            SystemPrompt = session.Description,
            IsActive = session.IsActive,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            MessageCount = session.MessageCount
        };
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.ChatSessions.GetByIdAsync(sessionId, cancellationToken);
        
        if (session == null || session.UserId != userId)
        {
            return false;
        }

        await _unitOfWork.ChatSessions.DeleteAsync(sessionId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true;
    }

    public async Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(Guid sessionId, Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        // Verify user owns the session
        var session = await _unitOfWork.ChatSessions.GetByIdAsync(sessionId, cancellationToken);
        if (session == null || session.UserId != userId)
        {
            return Enumerable.Empty<ChatMessageDto>();
        }

        var messages = await _unitOfWork.ChatMessages.GetSessionMessagesAsync(sessionId, page, pageSize, cancellationToken);
        
        return messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            SessionId = m.SessionId,
            Role = m.Role.ToString().ToLowerInvariant(),
            Content = m.Content,
            TokenCount = m.TokenCount,
            Model = m.ModelName,
            CreatedAt = m.CreatedAt
        });
    }

    public async Task<ChatResponseDto> SendMessageAsync(Guid sessionId, Guid userId, ChatRequestDto request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.ChatSessions.GetByIdAsync(sessionId, cancellationToken);
        
        if (session == null || session.UserId != userId)
        {
            throw new ArgumentException("Session not found or access denied");
        }

        // Create user message
        var userMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = MessageRole.User,
            Content = request.Message,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ChatMessages.AddAsync(userMessage, cancellationToken);

        // Simple AI response (placeholder)
        var aiResponse = $"Echo: {request.Message}";

        // Create assistant message
        var assistantMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = MessageRole.Assistant,
            Content = aiResponse,
            ModelName = session.ModelName,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ChatMessages.AddAsync(assistantMessage, cancellationToken);

        // Update session timestamp
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.ChatSessions.UpdateAsync(session, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ChatResponseDto
        {
            SessionId = sessionId,
            UserMessage = new ChatMessageDto
            {
                Id = userMessage.Id,
                SessionId = userMessage.SessionId,
                Role = userMessage.Role.ToString().ToLowerInvariant(),
                Content = userMessage.Content,
                CreatedAt = userMessage.CreatedAt
            },
            AssistantMessage = new ChatMessageDto
            {
                Id = assistantMessage.Id,
                SessionId = assistantMessage.SessionId,
                Role = assistantMessage.Role.ToString().ToLowerInvariant(),
                Content = assistantMessage.Content,
                Model = assistantMessage.ModelName,
                CreatedAt = assistantMessage.CreatedAt
            }
        };
    }

    public async Task SendMessageStreamAsync(Guid sessionId, Guid userId, ChatRequestDto request, Func<string, Task> onChunkReceived, CancellationToken cancellationToken)
    {
        // Simple implementation - just send the echo response as chunks
        var response = $"Echo: {request.Message}";
        
        for (int i = 0; i < response.Length; i += 5)
        {
            var chunk = response.Substring(i, Math.Min(5, response.Length - i));
            await onChunkReceived(chunk);
            await Task.Delay(100, cancellationToken); // Simulate streaming delay
        }
    }
}
