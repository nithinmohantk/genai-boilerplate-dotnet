using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using GenAIBoilerplate.Core.Common;

namespace GenAIBoilerplate.Core.Entities;

/// <summary>
/// Message role in a conversation
/// </summary>
public enum MessageRole
{
    /// <summary>
    /// Message from the user
    /// </summary>
    User,
    
    /// <summary>
    /// Message from the AI assistant
    /// </summary>
    Assistant,
    
    /// <summary>
    /// System message (instructions, context)
    /// </summary>
    System
}

/// <summary>
/// Chat message entity for storing individual messages
/// </summary>
public class ChatMessage : BaseEntity
{
    /// <summary>
    /// Chat session this message belongs to
    /// </summary>
    [Required]
    public Guid SessionId { get; set; }

    /// <summary>
    /// User who sent this message (null for AI messages)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Message role (user, assistant, system)
    /// </summary>
    public MessageRole Role { get; set; }

    /// <summary>
    /// Message content
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// AI model that generated this message (for assistant messages)
    /// </summary>
    [MaxLength(100)]
    public string? ModelName { get; set; }

    /// <summary>
    /// Token count for this message
    /// </summary>
    public int? TokenCount { get; set; }

    /// <summary>
    /// Message metadata as JSON (attachments, formatting, etc.)
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Whether this message was edited
    /// </summary>
    public bool IsEdited { get; set; } = false;

    /// <summary>
    /// Whether this message was deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Navigation property for chat session
    /// </summary>
    [ForeignKey(nameof(SessionId))]
    public virtual ChatSession Session { get; set; } = null!;

    /// <summary>
    /// Navigation property for user (null for AI messages)
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    /// <summary>
    /// Get metadata as dictionary
    /// </summary>
    public Dictionary<string, object>? GetMetadata()
    {
        if (string.IsNullOrEmpty(MetadataJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(MetadataJson);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Set metadata from dictionary
    /// </summary>
    public void SetMetadata(Dictionary<string, object>? metadata)
    {
        MetadataJson = metadata != null ? JsonSerializer.Serialize(metadata) : null;
    }

    /// <summary>
    /// Whether this is a user message
    /// </summary>
    public bool IsUserMessage => Role == MessageRole.User;

    /// <summary>
    /// Whether this is an AI assistant message
    /// </summary>
    public bool IsAssistantMessage => Role == MessageRole.Assistant;

    /// <summary>
    /// Whether this is a system message
    /// </summary>
    public bool IsSystemMessage => Role == MessageRole.System;

    /// <summary>
    /// Model property for backward compatibility (maps to ModelName)
    /// </summary>
    public string? Model
    {
        get => ModelName;
        set => ModelName = value;
    }
}
