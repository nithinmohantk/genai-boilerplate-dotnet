using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Application.Interfaces;
using GenAIBoilerplate.Core.Entities;
using GenAIBoilerplate.Core.Interfaces;
using GenAIBoilerplate.Core.Enums;
using GenAIBoilerplate.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace GenAIBoilerplate.Application.Services;

public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIService _aiService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IUnitOfWork unitOfWork,
        IAIService aiService,
        ILogger<ChatService> logger)
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
            MessageCount = s.Messages?.Count ?? 0
        });
    }

    public async Task<ChatSessionDto> CreateSessionAsync(Guid userId, CreateSessionRequestDto request, CancellationToken cancellationToken)
    {
        // Get user to verify they exist and get tenant info
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
            Model = session.Model,
            SystemPrompt = session.SystemPrompt,
            Temperature = session.Temperature,
            MaxTokens = session.MaxTokens,
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
            Model = session.Model,
            SystemPrompt = session.SystemPrompt,
            Temperature = session.Temperature,
            MaxTokens = session.MaxTokens,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            MessageCount = session.Messages?.Count ?? 0
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
            session.Model = request.Model;
        if (!string.IsNullOrEmpty(request.SystemPrompt))
            session.SystemPrompt = request.SystemPrompt;
        if (request.Temperature.HasValue)
            session.Temperature = request.Temperature.Value;
        if (request.MaxTokens.HasValue)
            session.MaxTokens = request.MaxTokens.Value;

        session.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ChatSessions.UpdateAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ChatSessionDto
        {
            Id = session.Id,
            Title = session.Title,
            Model = session.Model,
            SystemPrompt = session.SystemPrompt,
            Temperature = session.Temperature,
            MaxTokens = session.MaxTokens,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            MessageCount = session.Messages?.Count ?? 0
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
            Model = m.Model,
            CreatedAt = m.CreatedAt
        });
    }

    public async Task<ChatResponseDto> SendMessageAsync(Guid sessionId, Guid userId, ChatRequestDto request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.ChatSessions.GetByIdWithMessagesAsync(sessionId, cancellationToken);
        
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

        // Count tokens for user message
        userMessage.TokenCount = await _aiService.CountTokensAsync(request.Message, session.Model, cancellationToken);

        await _unitOfWork.ChatMessages.AddAsync(userMessage, cancellationToken);

        // Prepare conversation history
        var conversationHistory = new List<ChatMessage>();
        
        // Add system message if exists
        if (!string.IsNullOrEmpty(session.SystemPrompt))
        {
            conversationHistory.Add(new ChatMessage
            {
                Role = MessageRole.System,
                Content = session.SystemPrompt
            });
        }

        // Add previous messages (latest first, so reverse for chronological order)
        var previousMessages = session.Messages?.OrderBy(m => m.CreatedAt).ToList() ?? new List<ChatMessage>();
        conversationHistory.AddRange(previousMessages);
        
        // Add current user message
        conversationHistory.Add(userMessage);

        // Get AI response
        var aiResponseContent = await _aiService.GetChatResponseAsync(
            conversationHistory.Select(m => new ChatMessageDto
            {
                Role = m.Role.ToString().ToLowerInvariant(),
                Content = m.Content
            }).ToList(),
            session.Model,
            session.Temperature,
            session.MaxTokens,
            cancellationToken);

        // Create assistant message
        var assistantMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = MessageRole.Assistant,
            Content = aiResponseContent,
            Model = session.Model,
            CreatedAt = DateTime.UtcNow
        };

        // Count tokens for assistant message
        assistantMessage.TokenCount = await _aiService.CountTokensAsync(aiResponseContent, session.Model, cancellationToken);

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
                TokenCount = userMessage.TokenCount,
                CreatedAt = userMessage.CreatedAt
            },
            AssistantMessage = new ChatMessageDto
            {
                Id = assistantMessage.Id,
                SessionId = assistantMessage.SessionId,
                Role = assistantMessage.Role.ToString().ToLowerInvariant(),
                Content = assistantMessage.Content,
                TokenCount = assistantMessage.TokenCount,
                Model = assistantMessage.Model,
                CreatedAt = assistantMessage.CreatedAt
            }
        };
    }

    public async Task SendMessageStreamAsync(Guid sessionId, Guid userId, ChatRequestDto request, Func<string, Task> onChunkReceived, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.ChatSessions.GetByIdWithMessagesAsync(sessionId, cancellationToken);
        
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

        // Count tokens for user message
        userMessage.TokenCount = await _aiService.CountTokensAsync(request.Message, session.Model, cancellationToken);

        await _unitOfWork.ChatMessages.AddAsync(userMessage, cancellationToken);

        // Prepare conversation history
        var conversationHistory = new List<ChatMessage>();
        
        // Add system message if exists
        if (!string.IsNullOrEmpty(session.SystemPrompt))
        {
            conversationHistory.Add(new ChatMessage
            {
                Role = MessageRole.System,
                Content = session.SystemPrompt
            });
        }

        // Add previous messages
        var previousMessages = session.Messages?.OrderBy(m => m.CreatedAt).ToList() ?? new List<ChatMessage>();
        conversationHistory.AddRange(previousMessages);
        
        // Add current user message
        conversationHistory.Add(userMessage);

        // Stream AI response
        var fullResponseContent = new System.Text.StringBuilder();
        
        await _aiService.GetChatResponseStreamAsync(
            conversationHistory.Select(m => new ChatMessageDto
            {
                Role = m.Role.ToString().ToLowerInvariant(),
                Content = m.Content
            }).ToList(),
            session.Model,
            session.Temperature,
            session.MaxTokens,
            async (chunk) =>
            {
                fullResponseContent.Append(chunk);
                await onChunkReceived(chunk);
            },
            cancellationToken);

        // Create assistant message with full response
        var assistantMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = MessageRole.Assistant,
            Content = fullResponseContent.ToString(),
            Model = session.Model,
            CreatedAt = DateTime.UtcNow
        };

        // Count tokens for assistant message
        assistantMessage.TokenCount = await _aiService.CountTokensAsync(assistantMessage.Content, session.Model, cancellationToken);

        await _unitOfWork.ChatMessages.AddAsync(assistantMessage, cancellationToken);

        // Update session timestamp
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.ChatSessions.UpdateAsync(session, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
