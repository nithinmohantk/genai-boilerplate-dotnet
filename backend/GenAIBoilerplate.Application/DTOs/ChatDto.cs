using System.ComponentModel.DataAnnotations;
using GenAIBoilerplate.Core.Entities;

namespace GenAIBoilerplate.Application.DTOs;

/// <summary>
/// Chat session DTO
/// </summary>
public class ChatSessionDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string? Model { get; set; }
    public string? SystemPrompt { get; set; }
    public float? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public int MessageCount { get; set; }
    public List<ChatMessageDto>? Messages { get; set; }
}

/// <summary>
/// Create chat session request DTO
/// </summary>
public class CreateChatSessionRequestDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? ModelName { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Update chat session request DTO
/// </summary>
public class UpdateChatSessionRequestDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? ModelName { get; set; }

    public bool? IsActive { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Chat message DTO
/// </summary>
public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Model { get; set; }
    public int? TokenCount { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public UserDto? User { get; set; }
}

/// <summary>
/// Send message request DTO
/// </summary>
public class SendMessageRequestDto
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ModelName { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Chat completion request DTO
/// </summary>
public class ChatCompletionRequestDto
{
    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ModelName { get; set; }

    [Range(0.0, 2.0)]
    public double? Temperature { get; set; }

    [Range(1, 4000)]
    public int? MaxTokens { get; set; }

    public bool Stream { get; set; } = false;
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Chat completion response DTO
/// </summary>
public class ChatCompletionResponseDto
{
    public Guid MessageId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ModelName { get; set; }
    public int? TokenCount { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Search sessions request DTO
/// </summary>
public class SearchSessionsRequestDto
{
    public string? Query { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Available AI models DTO
/// </summary>
public class AIModelDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxTokens { get; set; }
    public bool SupportsStreaming { get; set; }
    public bool IsAvailable { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Create session request DTO (simplified for new implementation)
/// </summary>
public class CreateSessionRequestDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Model { get; set; }

    [MaxLength(1000)]
    public string? SystemPrompt { get; set; }

    [Range(0.0, 2.0)]
    public float? Temperature { get; set; }

    [Range(1, 8000)]
    public int? MaxTokens { get; set; }
}

/// <summary>
/// Update session request DTO (simplified for new implementation)
/// </summary>
public class UpdateSessionRequestDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(100)]
    public string? Model { get; set; }

    [MaxLength(1000)]
    public string? SystemPrompt { get; set; }

    [Range(0.0, 2.0)]
    public float? Temperature { get; set; }

    [Range(1, 8000)]
    public int? MaxTokens { get; set; }
}

/// <summary>
/// Chat request DTO (simplified for new implementation)
/// </summary>
public class ChatRequestDto
{
    [Required]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Chat response DTO (simplified for new implementation)
/// </summary>
public class ChatResponseDto
{
    public Guid SessionId { get; set; }
    public ChatMessageDto UserMessage { get; set; } = new();
    public ChatMessageDto AssistantMessage { get; set; } = new();
}
