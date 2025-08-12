using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using GenAIBoilerplate.Core.Common;

namespace GenAIBoilerplate.Core.Entities;

/// <summary>
/// Chat session entity for organizing conversations
/// </summary>
public class ChatSession : BaseEntity
{
    /// <summary>
    /// Tenant this session belongs to
    /// </summary>
    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// User who owns this session
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Session title
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Session description (optional)
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether the session is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// AI model used in this session
    /// </summary>
    [MaxLength(100)]
    public string? ModelName { get; set; }

    /// <summary>
    /// Session metadata as JSON
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// System prompt for this session
    /// </summary>
    [MaxLength(2000)]
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// AI temperature setting
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Maximum tokens for AI responses
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Last activity timestamp
    /// </summary>
    public DateTime? LastActivityAt { get; set; }

    /// <summary>
    /// Navigation property for tenant
    /// </summary>
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Navigation property for user
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Navigation property for messages in this session
    /// </summary>
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

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
    /// Get message count in this session
    /// </summary>
    public int MessageCount => Messages.Count;

    /// <summary>
    /// Model property for backward compatibility (maps to ModelName)
    /// </summary>
    public string? Model
    {
        get => ModelName;
        set => ModelName = value;
    }
}
