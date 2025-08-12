using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using GenAIBoilerplate.Core.Common;
using GenAIBoilerplate.Core.Enums;

namespace GenAIBoilerplate.Core.Entities;

/// <summary>
/// OAuth provider connections for users
/// </summary>
public class UserAuthProvider : BaseEntity
{
    /// <summary>
    /// User this provider belongs to
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Authentication provider type
    /// </summary>
    public AuthProvider Provider { get; set; }

    /// <summary>
    /// Provider's user ID
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ProviderUserId { get; set; } = string.Empty;

    /// <summary>
    /// Provider email (if available)
    /// </summary>
    [MaxLength(255)]
    public string? ProviderEmail { get; set; }

    /// <summary>
    /// Additional provider data as JSON
    /// </summary>
    public string? ProviderDataJson { get; set; }

    /// <summary>
    /// Navigation property for user
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Get provider data as dictionary
    /// </summary>
    public Dictionary<string, object>? GetProviderData()
    {
        if (string.IsNullOrEmpty(ProviderDataJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(ProviderDataJson);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Set provider data from dictionary
    /// </summary>
    public void SetProviderData(Dictionary<string, object>? providerData)
    {
        ProviderDataJson = providerData != null ? JsonSerializer.Serialize(providerData) : null;
    }
}
