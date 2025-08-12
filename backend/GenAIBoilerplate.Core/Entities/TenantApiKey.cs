using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using GenAIBoilerplate.Core.Common;

namespace GenAIBoilerplate.Core.Entities;

/// <summary>
/// API keys for tenant integrations
/// </summary>
public class TenantApiKey : BaseEntity
{
    /// <summary>
    /// Tenant this API key belongs to
    /// </summary>
    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// Human-readable name for the API key
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Hashed API key value
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// First few characters for identification
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Provider type (openai, anthropic, google, etc.)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Whether the API key is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Usage limits and quotas as JSON
    /// </summary>
    public string? UsageLimitsJson { get; set; }

    /// <summary>
    /// User who created this API key
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// When this API key expires (optional)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Last time this API key was used
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Navigation property for tenant
    /// </summary>
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Get usage limits as dictionary
    /// </summary>
    public Dictionary<string, object>? GetUsageLimits()
    {
        if (string.IsNullOrEmpty(UsageLimitsJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(UsageLimitsJson);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Set usage limits from dictionary
    /// </summary>
    public void SetUsageLimits(Dictionary<string, object>? usageLimits)
    {
        UsageLimitsJson = usageLimits != null ? JsonSerializer.Serialize(usageLimits) : null;
    }
}
