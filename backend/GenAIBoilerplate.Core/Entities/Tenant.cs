using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using GenAIBoilerplate.Core.Common;
using GenAIBoilerplate.Core.Enums;

namespace GenAIBoilerplate.Core.Entities;

/// <summary>
/// Tenant entity for multi-tenancy
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>
    /// Tenant name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique domain identifier
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Tenant description
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Current tenant status
    /// </summary>
    public TenantStatus Status { get; set; } = TenantStatus.Active;

    /// <summary>
    /// Tenant-specific settings as JSON
    /// </summary>
    public string? SettingsJson { get; set; }

    /// <summary>
    /// Branding configuration as JSON (logo, colors, etc.)
    /// </summary>
    public string? BrandingJson { get; set; }

    /// <summary>
    /// Usage limits and quotas as JSON
    /// </summary>
    public string? LimitsJson { get; set; }

    /// <summary>
    /// User who created this tenant
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Navigation property for users in this tenant
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    /// <summary>
    /// Navigation property for API keys in this tenant
    /// </summary>
    public virtual ICollection<TenantApiKey> ApiKeys { get; set; } = new List<TenantApiKey>();

    /// <summary>
    /// Navigation property for chat sessions in this tenant
    /// </summary>
    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();

    /// <summary>
    /// Get settings as dictionary
    /// </summary>
    public Dictionary<string, object>? GetSettings()
    {
        if (string.IsNullOrEmpty(SettingsJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(SettingsJson);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Set settings from dictionary
    /// </summary>
    public void SetSettings(Dictionary<string, object>? settings)
    {
        SettingsJson = settings != null ? JsonSerializer.Serialize(settings) : null;
    }

    /// <summary>
    /// Get branding as dictionary
    /// </summary>
    public Dictionary<string, object>? GetBranding()
    {
        if (string.IsNullOrEmpty(BrandingJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(BrandingJson);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Set branding from dictionary
    /// </summary>
    public void SetBranding(Dictionary<string, object>? branding)
    {
        BrandingJson = branding != null ? JsonSerializer.Serialize(branding) : null;
    }

    /// <summary>
    /// Get limits as dictionary
    /// </summary>
    public Dictionary<string, object>? GetLimits()
    {
        if (string.IsNullOrEmpty(LimitsJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(LimitsJson);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Set limits from dictionary
    /// </summary>
    public void SetLimits(Dictionary<string, object>? limits)
    {
        LimitsJson = limits != null ? JsonSerializer.Serialize(limits) : null;
    }

    /// <summary>
    /// Settings property for backward compatibility (maps to SettingsJson)
    /// </summary>
    public string? Settings
    {
        get => SettingsJson;
        set => SettingsJson = value;
    }
}
