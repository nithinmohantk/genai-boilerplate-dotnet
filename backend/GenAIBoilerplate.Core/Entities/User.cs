using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using GenAIBoilerplate.Core.Common;
using GenAIBoilerplate.Core.Enums;

namespace GenAIBoilerplate.Core.Entities;

/// <summary>
/// User entity
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Tenant this user belongs to
    /// </summary>
    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// User email address
    /// </summary>
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Username (optional)
    /// </summary>
    [MaxLength(100)]
    public string? Username { get; set; }

    /// <summary>
    /// Full name
    /// </summary>
    [MaxLength(200)]
    public string? FullName { get; set; }

    /// <summary>
    /// Hashed password for email authentication
    /// </summary>
    [MaxLength(255)]
    public string? HashedPassword { get; set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the user email is verified
    /// </summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// User role in the system
    /// </summary>
    public UserRole Role { get; set; } = UserRole.TenantUser;

    /// <summary>
    /// Additional permissions as JSON
    /// </summary>
    public string? PermissionsJson { get; set; }

    /// <summary>
    /// Avatar URL
    /// </summary>
    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// User timezone
    /// </summary>
    [MaxLength(50)]
    public string Timezone { get; set; } = "UTC";

    /// <summary>
    /// User language preference
    /// </summary>
    [MaxLength(10)]
    public string Language { get; set; } = "en";

    /// <summary>
    /// User preferences as JSON
    /// </summary>
    public string? PreferencesJson { get; set; }

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Navigation property for tenant
    /// </summary>
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Navigation property for authentication providers
    /// </summary>
    public virtual ICollection<UserAuthProvider> AuthProviders { get; set; } = new List<UserAuthProvider>();

    /// <summary>
    /// Navigation property for chat sessions
    /// </summary>
    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();

    /// <summary>
    /// Navigation property for chat messages
    /// </summary>
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    /// <summary>
    /// Navigation property for refresh tokens
    /// </summary>
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>
    /// Get permissions as dictionary
    /// </summary>
    public Dictionary<string, object>? GetPermissions()
    {
        if (string.IsNullOrEmpty(PermissionsJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(PermissionsJson);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Set permissions from dictionary
    /// </summary>
    public void SetPermissions(Dictionary<string, object>? permissions)
    {
        PermissionsJson = permissions != null ? JsonSerializer.Serialize(permissions) : null;
    }

    /// <summary>
    /// Get preferences as dictionary
    /// </summary>
    public Dictionary<string, object>? GetPreferences()
    {
        if (string.IsNullOrEmpty(PreferencesJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(PreferencesJson);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Set preferences from dictionary
    /// </summary>
    public void SetPreferences(Dictionary<string, object>? preferences)
    {
        PreferencesJson = preferences != null ? JsonSerializer.Serialize(preferences) : null;
    }
}
