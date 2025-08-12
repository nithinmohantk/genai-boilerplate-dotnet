using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using GenAIBoilerplate.Core.Common;

namespace GenAIBoilerplate.Core.Entities;

/// <summary>
/// Refresh tokens for JWT authentication
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// User this refresh token belongs to
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Hashed token value
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Whether the token has been revoked
    /// </summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// Token expiration time
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Device/session information as JSON
    /// </summary>
    public string? DeviceInfoJson { get; set; }

    /// <summary>
    /// IP address of the client
    /// </summary>
    [MaxLength(45)] // IPv6 compatible
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Navigation property for user
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Get device info as dictionary
    /// </summary>
    public Dictionary<string, object>? GetDeviceInfo()
    {
        if (string.IsNullOrEmpty(DeviceInfoJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(DeviceInfoJson);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Set device info from dictionary
    /// </summary>
    public void SetDeviceInfo(Dictionary<string, object>? deviceInfo)
    {
        DeviceInfoJson = deviceInfo != null ? JsonSerializer.Serialize(deviceInfo) : null;
    }

    /// <summary>
    /// Check if token is valid (not expired and not revoked)
    /// </summary>
    public bool IsValid => !IsRevoked && ExpiresAt > DateTime.UtcNow;
}
