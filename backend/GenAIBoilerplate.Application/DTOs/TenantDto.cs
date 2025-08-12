using System.ComponentModel.DataAnnotations;
using GenAIBoilerplate.Core.Enums;

namespace GenAIBoilerplate.Application.DTOs;

/// <summary>
/// Tenant DTO
/// </summary>
public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Domain { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Settings { get; set; }
    public string? Branding { get; set; }
    public string? Limits { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Create tenant request DTO
/// </summary>
public class CreateTenantRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? Settings { get; set; }
}

/// <summary>
/// Update tenant request DTO
/// </summary>
public class UpdateTenantRequestDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? Settings { get; set; }
}

/// <summary>
/// Tenant API Key DTO
/// </summary>
public class TenantApiKeyDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Create API key request DTO
/// </summary>
public class CreateApiKeyRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Tenant usage DTO
/// </summary>
public class TenantUsageDto
{
    public Guid TenantId { get; set; }
    public int TotalMessages { get; set; }
    public int TotalTokens { get; set; }
    public int TotalSessions { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Dictionary<string, int> ModelUsage { get; set; } = new();
    public Dictionary<DateTime, int> DailyUsage { get; set; } = new();
}
